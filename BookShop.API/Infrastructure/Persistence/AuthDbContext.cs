using BookShop.API.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace BookShop.API.Infrastructure.Persistence;

/// <summary>
/// Represents the Entity Framework Core database context for authentication and authorization.
/// </summary>
/// <remarks>
/// Manages Users, Roles, UserRoles, and RefreshTokens tables in PostgreSQL.
/// Handles soft-delete, automatic timestamp updates, and relationships.
/// </remarks>
/// <param name="options">The options to configure the DbContext, including connection string and provider.</param>
public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the <see cref="User"/> entities.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Gets the <see cref="Role"/> entities.
    /// </summary>
    public DbSet<Role> Roles => Set<Role>();

    /// <summary>
    /// Gets the <see cref="UserRole"/> entities.
    /// </summary>
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    /// <summary>
    /// Gets the <see cref="RefreshToken"/> entities.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// Configures the EF Core model, including table mappings, relationships, indexes, and query filters.
    /// </summary>
    /// <param name="modelBuilder">The builder used to configure entity mappings.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => !u.IsDeleted);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .HasFilter("IsDeleted = false")
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .HasFilter("IsDeleted = false")
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<User>()
            .Property(u => u.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.Tokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<UserRole>().ToTable("UserRoles");
        modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
    }

    /// <summary>
    /// Saves all changes made in this context to the database, updating timestamps as needed.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public override int SaveChanges()
    {
        UpdateTimestamps();

        return base.SaveChanges();
    }

    /// <summary>
    /// Asynchronously saves all changes made in this context to the database, updating timestamps as needed.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates CreatedAt and UpdatedAt timestamps for User entities before saving changes.
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<User>()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);

        foreach(var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;

            if(entry.State  == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Marks a <see cref="User"/> as deleted and invalidates all their refresh tokens.
    /// </summary>
    /// <param name="user">The user to soft-delete.</param>
    public void SoftDelete(User user)
    {
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        Entry(user).State = EntityState.Modified;

        foreach(var token in user.Tokens)
        {
            RefreshTokens.Remove(token);
        }
    }
}
