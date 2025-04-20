using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;

namespace ECommercePaymentIntegration.Application.Interfaces.BalanceManagement
{
   public interface IBalanceManagementService
   {
      Task<OrderResultDto> CancelOrderAsync(CancelOrderRequest request);
      Task<OrderResultDto> CompleteOrderAsync(CompleteOrderRequest request);
      Task<UserBalanceDto> GetBalanceAsync();
      Task<IEnumerable<ProductDto>> GetProductsAsync();
      Task<PreOrderResultDto> PreorderAsync(PreorderRequest request);
   }
}
