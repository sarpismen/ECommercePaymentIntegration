using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.Exceptions
{
   public abstract class ServiceExceptionBase : Exception
   {
      public HttpStatusCode HttpStatusCode { get; }
      public string? Error { get; set; }
      public ServiceExceptionBase(HttpStatusCode httpStatusCode, string? message, string? error)
       : base(message) => (HttpStatusCode, Error) = (httpStatusCode, error);

      public ServiceExceptionBase(HttpStatusCode httpStatusCode, string? message)
          : base(message) => HttpStatusCode = httpStatusCode;

      public ServiceExceptionBase(HttpStatusCode httpStatusCode)
          => HttpStatusCode = httpStatusCode;

      public ServiceExceptionBase(HttpStatusCode httpStatusCode, string? message, Exception? innerException)
          : base(message, innerException) => HttpStatusCode = httpStatusCode;
   }
}
