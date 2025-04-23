using System;
using System.Net;

namespace ECommercePaymentIntegration.Application.Exceptions
{
   public class OutOfStockException : ServiceExceptionBase
   {
      public OutOfStockException()
         : base(HttpStatusCode.NotFound)
      {
      }

      public OutOfStockException(string message)
         : base(HttpStatusCode.NotFound, message)
      {
      }

      public OutOfStockException(string message, string error)
         : base(HttpStatusCode.NotFound, message, error)
      {
      }

      public OutOfStockException(string message, Exception innerException)
         : base(HttpStatusCode.NotFound, message, innerException)
      {
      }
   }
}
