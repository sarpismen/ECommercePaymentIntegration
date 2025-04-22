using ECommercePaymentIntegration.Domain.Interfaces.Product;
using System.ComponentModel.DataAnnotations;

namespace ECommercePaymentIntegration.Domain.Entities.Product
{
   public abstract class ProductBase : IProduct
   {
      public string ProductId { get; set; }
      public decimal ItemPrice { get; set; }
   }
}
