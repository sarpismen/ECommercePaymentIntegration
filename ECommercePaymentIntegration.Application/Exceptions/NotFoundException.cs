using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.Exceptions
{
   public class NotFoundException : ServiceExceptionBase
   {
      public NotFoundException()
      {
      }

      public NotFoundException(string message) : base(message)
      {
      }

      public NotFoundException(string message, string error) : base(message, error)
      {
      }

      public NotFoundException(string message, Exception innerException) : base(message, innerException)
      {
      }
   }
}
