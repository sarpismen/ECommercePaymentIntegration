using System;
using System.Collections.Generic;
using ECommercePaymentIntegration.Domain.ValueObjects.Order;

namespace ECommercePaymentIntegration.Domain.Entities.Order
{
   public class Order
   {
      public Guid UserId { get; set; }
      public Guid OrderId { get; } = Guid.NewGuid();
      public string OrderIdString => OrderId.ToString();
      public IList<OrderItem> OrderItems { get; set; }
      public OrderStatus OrderStatus { get; set; }
      public DateTime CreationTimestamp { get; set; }

   }
}
