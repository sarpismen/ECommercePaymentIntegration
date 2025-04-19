namespace ECommercePaymentIntegration.Domain.Entities.Product
{
   public class Product : ProductBase
   {
      public string Name { get; set; }
      public string Description { get; set; }
      public string Currency { get; set; }
      public string Category { get; set; }
      public int Stock { get; set; }
   }
}
