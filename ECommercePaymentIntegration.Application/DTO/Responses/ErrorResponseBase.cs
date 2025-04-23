namespace ECommercePaymentIntegration.Application.DTO.Responses
{
   public abstract class ErrorResponseBase
   {
      public string Error { get; set; }
      public string Message { get; set; }
   }
}
