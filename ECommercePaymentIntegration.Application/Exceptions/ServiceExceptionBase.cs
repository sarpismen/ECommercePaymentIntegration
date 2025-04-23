using System;
using System.Net;

namespace ECommercePaymentIntegration.Application.Exceptions
{
   public abstract class ServiceExceptionBase : Exception
   {
      public ServiceExceptionBase(HttpStatusCode httpStatusCode, string message, string error)
       : base(message) => (HttpStatusCode, Error) = (httpStatusCode, error);

      public ServiceExceptionBase(HttpStatusCode httpStatusCode, string message)
          : base(message) => HttpStatusCode = httpStatusCode;

      public ServiceExceptionBase(HttpStatusCode httpStatusCode)
          => HttpStatusCode = httpStatusCode;

      public ServiceExceptionBase(HttpStatusCode httpStatusCode, string message, Exception innerException)
          : base(message, innerException) => HttpStatusCode = httpStatusCode;
      public string OrderId { get; set; }
      public HttpStatusCode HttpStatusCode { get; }
      public string Error { get; set; }
   }
}
