using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Domain.Entities.Product;

namespace ECommercePaymentIntegration.Domain.Entities.Order
{
   public class OrderItem : ProductBase
   {
      [Key]
      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      public int Id { get; set; }
      public string OrderId { get; set; }
      public int Quantity { get; set; }
      public decimal SubTotal => ItemPrice;
      public Order Order { get; set; }
   }
}
