namespace ECommercePaymentIntegration.Domain.Interfaces.Product
{
   public interface IProduct
   {
      string ProductId { get; }
      decimal ItemPrice { get; }
   }
}