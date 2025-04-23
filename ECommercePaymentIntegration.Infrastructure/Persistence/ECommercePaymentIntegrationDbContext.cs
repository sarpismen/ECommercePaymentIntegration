using ECommercePaymentIntegration.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;

namespace ECommercePaymentIntegration.Infrastructure.Persistence
{
   public class ECommercePaymentIntegrationDbContext : DbContext
   {
      public ECommercePaymentIntegrationDbContext(DbContextOptions<ECommercePaymentIntegrationDbContext> options)
         : base(options)
      {
      }

      public DbSet<Order> Orders { get; set; }
      public DbSet<OrderItem> OrderItems { get; set; }

      public DbSet<OrderError> OrderErrors { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         modelBuilder.Entity<Order>()
             .HasMany(o => o.OrderItems)
             .WithOne(oi => oi.Order)
             .HasForeignKey(oi => oi.OrderId);

         modelBuilder.Entity<Order>()
             .HasMany(o => o.OrderErrors)
             .WithOne(oi => oi.Order)
             .HasForeignKey(oi => oi.OrderId);

         base.OnModelCreating(modelBuilder);
      }
   }
}
