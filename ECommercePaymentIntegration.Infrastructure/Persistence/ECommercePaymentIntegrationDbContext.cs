using ECommercePaymentIntegration.Domain.Entities.Order;
using ECommercePaymentIntegration.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Infrastructure.Persistence
{
   public class ECommercePaymentIntegrationDbContext : DbContext
   {
      public ECommercePaymentIntegrationDbContext(DbContextOptions<ECommercePaymentIntegrationDbContext> options) : base(options)
      {
      }
      public DbSet<Order> Orders { get; set; }
      public DbSet<OrderItem> OrderItems { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         modelBuilder.Entity<Order>()
             .HasMany(o => o.OrderItems)
             .WithOne(oi => oi.Order)
             .HasForeignKey(oi => oi.OrderId);
         base.OnModelCreating(modelBuilder);
      }
   }

}
