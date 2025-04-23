using System;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Domain.Entities.Order;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePaymentIntegration.Infrastructure.Persistence
{
   public class OrderRepository : IOrderRepository
   {
      private IServiceProvider _serviceProvider;

      public OrderRepository(IServiceProvider serviceProvider)
      {
         _serviceProvider = serviceProvider;
      }

      public async Task AddAsync(Order order)
      {
         using (var scope = _serviceProvider.CreateAsyncScope())
         {
            var dbContext = scope.ServiceProvider.GetService<ECommercePaymentIntegrationDbContext>();
            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();
         }
      }

      public async Task UpdateAsync(Order order)
      {
         using (var scope = _serviceProvider.CreateAsyncScope())
         {
            var dbContext = scope.ServiceProvider.GetService<ECommercePaymentIntegrationDbContext>();
            order.LastUpdatedAt = DateTimeOffset.UtcNow;
            dbContext.Orders.Update(order);
            await dbContext.SaveChangesAsync();
         }
      }

      public async Task<Order> GetByIdAsync(string key)
      {
         using (var scope = _serviceProvider.CreateAsyncScope())
         {
            var dbContext = scope.ServiceProvider.GetService<ECommercePaymentIntegrationDbContext>();
            return await dbContext.Orders.FindAsync(key);
         }
      }
   }
}
