using BookShop.API.Models.Order;
using Microsoft.EntityFrameworkCore;

namespace BookShop.API.Infrastructure.Persistence;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("CreatedAt");

            entity.Property(o => o.UpdatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("UpdatedAt");

            entity.Property(o => o.TotalPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.Price).HasPrecision(18, 2);

            entity.HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>().ToTable("Orders");
        modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
    }

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

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<Order>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
        
        var dateTime = DateTime.UtcNow;

        foreach(var entry in entries)
        {
            entry.Entity.UpdatedAt = dateTime;

            if(entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = dateTime;
            }
        }
    }
}