using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ProductAPI.Implementation
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<Product>().Property(p => p.Name).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Product>().Property(p => p.Description).HasMaxLength(500);
            modelBuilder.Entity<Product>().Property(p => p.Price).IsRequired();
            modelBuilder.Entity<Product>().Property(p => p.Quantity).IsRequired();
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Product>()
                .HasData(
                    new Product { Id = 1, Name = "Product1", Description = "Description1", Price = 100, Quantity = 10 },
                    new Product { Id = 2, Name = "Product2", Description = "Description2", Price = 200, Quantity = 20 },
                    new Product { Id = 3, Name = "Product3", Description = "Description3", Price = 300, Quantity = 30 }
                );
        }
    }
}