using BookShop.API.Models.Auth;
using Microsoft.AspNetCore.Identity;
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
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasQueryFilter(u => !u.IsDeleted);

            entity.HasIndex(u => u.NormalizedUsername)
            .HasFilter("\"IsDeleted\" = false")
            .IsUnique();

            entity.HasIndex(u => u.NormalizedEmail)
            .HasFilter("\"IsDeleted\" = false")
            .IsUnique();

            entity.Property(u => u.Id).HasColumnName("Id");

            entity.Property(u => u.UserName)
            .HasColumnName("UserName")
            .HasMaxLength(50)
            .IsRequired();

            entity.Property(u => u.NormalizedUsername)
            .HasColumnName("NormalizedUsername")
            .HasMaxLength(50)
            .IsRequired();

            entity.Property(u => u.Email)
            .HasColumnName("Email")
            .HasMaxLength(100)
            .IsRequired();

            entity.Property(u => u.NormalizedEmail)
            .HasColumnName("NormalizedEmail")
            .HasMaxLength(100)
            .IsRequired();

            entity.Property(u => u.PasswordHash)
            .HasColumnType("TEXT")
            .HasColumnName("PasswordHash")
            .IsRequired();

            entity.Property(u => u.IsActive)
            .HasColumnName("IsActive")
            .HasDefaultValue(true);

            entity.Property(u => u.IsDeleted)
            .HasColumnName("IsDeleted")
            .HasDefaultValue(false);

            entity.Property(u => u.IsEmailConfirmed)
            .HasColumnName("IsEmailConfirmed")
            .HasDefaultValue(false)
            .IsRequired();

            entity.Property(u => u.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("CreatedAt");

            entity.Property(u => u.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("UpdatedAt");

        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(r => r.Id).HasColumnName("Id");

            entity.Property(r => r.Name)
            .HasColumnName("Name")
            .HasMaxLength(50)
            .IsRequired();

            entity.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasIndex(ur => ur.RoleId);

            entity.HasIndex(ur => ur.UserId);

            entity.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasOne(rt => rt.User)
            .WithMany(u => u.Tokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(rt => rt.TokenHash).IsUnique();

            entity.HasIndex(rt => rt.UserId);

            entity.HasIndex(rt => rt.ExpiresAt);

            entity.Property(rt => rt.TokenHash)
            .HasColumnName("TokenHash")
            .HasMaxLength(128)
            .IsRequired();


            entity.Property(rt => rt.RevokedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("RevokedAt");

            entity.Property(rt => rt.ReplacedByTokenHash)
            .HasColumnName("ReplacedByTokenHash")
            .HasMaxLength(128);

            entity.Property(rt => rt.CreatedByIp)
            .HasColumnName("CreatedByIp")
            .HasMaxLength(45);

            entity.Property(rt => rt.UserAgent)
            .HasColumnName("UserAgent")
            .HasMaxLength(512);

            entity.Property(rt => rt.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("CreatedAt");

            entity.Property(rt => rt.ExpiresAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("ExpiresAt");

            entity.Property(rt => rt.UserId)
            .HasColumnName("UserId");

            entity.Property(rt => rt.Id)
            .HasColumnName("Id");
        });

        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<UserRole>().ToTable("UserRoles");
        modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");

        //The seed data
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "admin" },
            new Role { Id = 2, Name = "user" }
        );

        string admin = "admin";
        string adminEmail = "admin@bookshop.api.com";
        var seededAt = new DateTime(2026, 01, 15, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                UserName = admin,
                NormalizedUsername = NormalizeString(admin),
                Email = adminEmail,
                NormalizedEmail = NormalizeString(adminEmail),
                IsActive = true,
                IsDeleted = false,
                IsEmailConfirmed = true,
                CreatedAt = seededAt,
                UpdatedAt = seededAt,
                PasswordHash = "AQAAAAEAACcQAAAAEFWDfK8QnvlZsT6wjuSYyw2Xe4P1HTcaW5MavWsfHOaFY4CpvcPtbDZWP6XrT3Jkgg=="
            }
         );

        modelBuilder.Entity<UserRole>().HasData(new UserRole { UserId = 1, RoleId = 1 });

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

    /// <summary>
    /// Converts the specified string to its uppercase equivalent using the casing rules of the invariant culture.
    /// </summary>
    /// <param name="input">The string to normalize. Can be null.</param>
    /// <returns>A string in uppercase using the invariant culture, or null if the input is null.</returns>
    private static string NormalizeString(string input) => input.ToUpperInvariant();
}
