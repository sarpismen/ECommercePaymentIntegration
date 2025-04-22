using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using ECommercePaymentIntegration.Domain.ValueObjects.Order;

namespace ECommercePaymentIntegration.Domain.Entities.Order
{
   public class Order
   {
      public DateTimeOffset? CompletedAt { get; set; }
      public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
      public DateTimeOffset? LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;
      [Key]
      public string OrderId { get; set; } = Guid.NewGuid().ToString();
      public IList<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

      public OrderStatus Status { get; set; }
      [NotMapped]
      public decimal Total => OrderItems.Sum(x => x.SubTotal);

      public string Error { get; set; }
   }
}
