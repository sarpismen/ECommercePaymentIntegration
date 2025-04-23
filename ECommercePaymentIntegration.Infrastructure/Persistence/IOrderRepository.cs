using System;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;

namespace ECommercePaymentIntegration.Infrastructure.Persistence
{
   public interface IOrderRepository
   {
      Task AddAsync(Order order);
      Task UpdateAsync(Order order);

      Task<Order> GetByIdAsync(string key);
   }
}
