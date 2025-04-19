using ECommercePaymentIntegration.Domain.Entities.Order;
using ECommercePaymentIntegration.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Infrastructure.Persistence
{
    public class ECommercePaymentIntegrationDbContext : DbContext
    {
      public ECommercePaymentIntegrationDbContext(DbContextOptions<ECommercePaymentIntegrationDbContext> options) : base(options)
      {
      }

      public DbSet<Product> Products { get; set; }
      public DbSet<Order> Orders { get; set; }
      public DbSet<OrderItem> OrderItems { get; set; }
      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         // Configure relationships and constraints here (if needed)
         modelBuilder.Entity<Order>()
             .HasMany(o => o.OrderItems)
             .WithOne(oi => oi.Order)
             .HasForeignKey(oi => oi.OrderId);

         modelBuilder.Entity<OrderItem>()
             .HasKey(oi => oi.Id); // Assuming OrderItem has an Id

         base.OnModelCreating(modelBuilder);
      }
   }
}
