using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;
using ECommercePaymentIntegration.Domain.Entities.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.Interfaces.PaymentIntegration
{
   public interface IPaymentIntegrationService
   {
      Task<OrderResultDtoBase> CompleteOrder(CompleteOrderRequest completeOrderRequest);
      Task<PreOrderResultDto> CreateOrder(CreateOrderRequest req);
      Task<IEnumerable<Product>> GetProducts();
   }
}
