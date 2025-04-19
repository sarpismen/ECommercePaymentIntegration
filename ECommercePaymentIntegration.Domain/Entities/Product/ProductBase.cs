using ECommercePaymentIntegration.Domain.Interfaces.Product;

namespace ECommercePaymentIntegration.Domain.Entities.Product
{
   public abstract class ProductBase : IProduct
   {
      public string ProductId { get; set; }
      public decimal Price { get; set; }
   }
}
