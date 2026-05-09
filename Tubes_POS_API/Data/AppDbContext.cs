using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Entities;

namespace Tubes_POS_API.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasIndex(m => m.Name);
            entity.HasIndex(m => m.Category);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(t => t.TransactionCode).IsUnique();
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.CreatedAt);
        });

        modelBuilder.Entity<TransactionItem>(entity =>
        {
            entity.HasOne(ti => ti.Transaction)
                  .WithMany(t => t.Items)
                  .HasForeignKey(ti => ti.TransactionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ti => ti.Menu)
                  .WithMany()
                  .HasForeignKey(ti => ti.MenuId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Dummy data menu
        modelBuilder.Entity<Menu>().HasData(
            new Menu { Id = 1, Name = "Nasi Goreng Spesial", Price = 25000m, Category = "Makanan", IsAvailable = true },
            new Menu { Id = 2, Name = "Es Teh Manis", Price = 5000m, Category = "Minuman", IsAvailable = true },
            new Menu { Id = 3, Name = "Kopi Hitam", Price = 10000m, Category = "Minuman", IsAvailable = true }
        );
    }
}
