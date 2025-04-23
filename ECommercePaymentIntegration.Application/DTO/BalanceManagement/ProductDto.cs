namespace ECommercePaymentIntegration.Application.DTO.BalanceManagement
{
   public class ProductDto
   {
      public string Id { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
      public string Currency { get; set; }
      public string Category { get; set; }
      public int Stock { get; set; }
      public decimal Price { get; set; }
   }
}
