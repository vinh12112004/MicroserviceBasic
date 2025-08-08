using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace InventoryAPI.Implementation
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
        {
        }
        public DbSet<Inventory> Inventories { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inventory>().HasKey(i => i.InventoryID);
            modelBuilder.Entity<Inventory>().Property(i => i.Name).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Inventory>().Property(i => i.Quantity).IsRequired();
            modelBuilder.Entity<Inventory>().ToTable("Inventories");
            modelBuilder.Entity<Inventory>()
                .HasData(
                    new Inventory { InventoryID = 1, Name = "Warehouse A", Quantity = 100 },
                    new Inventory { InventoryID = 2, Name = "Warehouse B", Quantity = 200 },
                    new Inventory { InventoryID = 3, Name = "Warehouse C", Quantity = 300 }
                );
        }
    }
}