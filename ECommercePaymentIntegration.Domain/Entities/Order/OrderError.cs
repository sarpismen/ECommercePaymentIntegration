using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Domain.Entities.Order
{
   public class OrderError
   {
      [Key]
      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      public int Id { get; set; }
      public string OrderId { get; set; }
      public string Error { get; set; }
      public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

      public Order Order { get; set; }
   }
}
