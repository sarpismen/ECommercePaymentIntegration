using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Domain.Entities.Order
{
   public class OrderItem
   {
      public int Id { get; set; }
      public string ProductId { get; set; }
      public string OrderId { get; set; }
      public int Quantity { get; set; }
      public decimal UnitPrice { get; set; }
      public decimal SubTotal => Quantity * UnitPrice;
      public Order Order { get; set; }
   }
}
