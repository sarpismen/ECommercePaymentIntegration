using System;
using System.Net;

namespace ECommercePaymentIntegration.Application.Exceptions
{
   public class BadRequestException : ServiceExceptionBase
   {
      public BadRequestException()
         : base(HttpStatusCode.BadRequest)
      {
      }

      public BadRequestException(string message)
         : base(HttpStatusCode.BadRequest, message)
      {
      }

      public BadRequestException(string message, string error)
         : base(HttpStatusCode.BadRequest, message, error)
      {
      }

      public BadRequestException(string message, Exception innerException)
         : base(HttpStatusCode.BadRequest, message, innerException)
      {
      }
   }
}
