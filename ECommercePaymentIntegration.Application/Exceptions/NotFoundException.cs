using System;
using System.Net;

namespace ECommercePaymentIntegration.Application.Exceptions
{
   public class NotFoundException : ServiceExceptionBase
   {
      public NotFoundException() : base(HttpStatusCode.NotFound)
      {
      }

      public NotFoundException(string message) : base(HttpStatusCode.NotFound, message)
      {
      }

      public NotFoundException(string message, string error) : base(HttpStatusCode.NotFound, message, error)
      {
      }

      public NotFoundException(string message, Exception innerException) : base(HttpStatusCode.NotFound, message, innerException)
      {
      }
   }
}
