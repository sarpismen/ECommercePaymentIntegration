namespace ECommercePaymentIntegration.Application.DTO.Responses
{
   public abstract class ResponseBase<T>
   {
      public T Data { get; set; }
      public bool Success { get; set; }
   }
}
