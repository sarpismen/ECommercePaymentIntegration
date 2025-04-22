using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.Exceptions
{
   public class BadRequestException : ServiceExceptionBase
   {
      public BadRequestException() : base(HttpStatusCode.BadRequest)
      {
      }

      public BadRequestException(string message) : base(HttpStatusCode.BadRequest, message)
      {
      }

      public BadRequestException(string message, string error) : base(HttpStatusCode.BadRequest, message, error)
      {
      }

      public BadRequestException(string message, Exception innerException) : base(HttpStatusCode.BadRequest, message, innerException)
      {
      }
   }
}
