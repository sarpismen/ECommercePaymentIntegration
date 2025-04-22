using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.Exceptions
{
   public class BalanceManagementServiceException : ServiceExceptionBase
   {
      public BalanceManagementServiceException()
      {
      }

      public BalanceManagementServiceException(string message) : base(message)
      {
      }

      public BalanceManagementServiceException(string message, string error) : base(message, error)
      {
      }

      public BalanceManagementServiceException(string message, Exception innerException) : base(message, innerException)
      {
      }
   }
}
