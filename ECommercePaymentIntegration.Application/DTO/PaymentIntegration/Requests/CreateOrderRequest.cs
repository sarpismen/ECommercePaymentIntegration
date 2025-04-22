using ECommercePaymentIntegration.Application.DTO.Responses;
using ECommercePaymentIntegration.Domain.Entities.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests
{
   public class CreateOrderRequest
   {
      public IEnumerable<OrderItemDto> Items { get; set; }
   }
}
