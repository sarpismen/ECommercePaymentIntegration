using System;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Enums;

namespace ECommercePaymentIntegration.Application.DTO.BalanceManagement
{
   public class OrderStatusDto
   {
      public string OrderId { get; set; }
      public decimal Amount { get; set; }
      public PreOrderStatus Status { get; set; }
      public DateTime CompletedAt { get; set; }

      public DateTime CancelledAt { get; set; }
   }
}
