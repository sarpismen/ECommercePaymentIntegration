using ECommercePaymentIntegration.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Infrastructure.Persistence
{
   public interface IOrderRepository
   {
      Task AddAsync(Order order);
      Task UpdateAsync(Order order);

      Task<Order?> GetByIdAsync(string key);
   }

}
