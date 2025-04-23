using System.Collections.Generic;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;

namespace ECommercePaymentIntegration.Application.Interfaces.PaymentIntegration
{
   public interface IPaymentIntegrationService
   {
      Task<OrderResultDtoBase> CompleteOrder(CompleteOrderRequest completeOrderRequest);
      Task<PreOrderResultDto> CreateOrderAsync(CreateOrderRequest req);
      Task<IEnumerable<ProductDto>> GetProductsAsync();
   }
}
