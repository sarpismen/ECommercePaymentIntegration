using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.DTO.BalanceManagement
{
   public class OrderDto
   {
      public string OrderId { get; set; }
      public decimal Amount { get; set; }
      public PreOrderStatus Status { get; set; }
      public DateTime CompletedAt { get; set; }

      public DateTime CancelledAt { get; set; }
   }
}
