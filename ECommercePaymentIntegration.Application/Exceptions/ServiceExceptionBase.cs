using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.Exceptions
{
   public abstract class ServiceExceptionBase : Exception
   {
      public string? Error { get; set; }
      public ServiceExceptionBase(string? message, string? error)
         : base(message)
      {
         Error = error;
      }
      public ServiceExceptionBase(string? message)
         : base(message)
      {
      }

      public ServiceExceptionBase()
      {
      }

      public ServiceExceptionBase(string? message, Exception? innerException)
         : base(message, innerException)
      {
      }
   }
}
