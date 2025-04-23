using System.Collections.Generic;

namespace ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests
{
   public class CreateOrderRequest
   {
      public IEnumerable<OrderItemDto> Items { get; set; }
   }
}
