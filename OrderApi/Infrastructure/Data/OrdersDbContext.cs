using Microsoft.EntityFrameworkCore;
using OrderApi.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace OrderApi.Infrastructure.Data
{
    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.HasMany(e => e.Items)
                    .WithOne(e => e.Order)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OrderItem entity
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductId)
                    .IsRequired();
                entity.Property(e => e.Quantity)
                    .IsRequired();

                entity.HasIndex(e => e.OrderId);
            });

            // Seed data (optional)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var orderId = Guid.NewGuid();

            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    OrderId = orderId,
                    CustomerName = "Sample Customer",
                    CreatedAt = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<OrderItem>().HasData(
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 2
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 1
                }
            );
        }
    }
}
