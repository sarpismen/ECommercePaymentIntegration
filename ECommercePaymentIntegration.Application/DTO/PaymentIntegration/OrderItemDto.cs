using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.DTO.PaymentIntegration
{
   public class OrderItemDto
   {
      public string ProductId { get; set; }
      public int Quantity { get; set; }
   }
}
