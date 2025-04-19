using ECommercePaymentIntegration.Domain.Interfaces.Product;
using System.ComponentModel.DataAnnotations;

namespace ECommercePaymentIntegration.Domain.Entities.Product
{
   public abstract class ProductBase : IProduct
   {
      [Key]
      public string Id { get; set; }
      public decimal Price { get; set; }
   }
}
