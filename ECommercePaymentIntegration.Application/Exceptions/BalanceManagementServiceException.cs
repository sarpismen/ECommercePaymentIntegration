using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.Exceptions
{
   public class BalanceManagementServiceException : Exception
   {
      public BalanceManagementServiceException(string? message)
         : base(message)
      {
      }

      public BalanceManagementServiceException()
      {
      }

      public BalanceManagementServiceException(string? message, Exception? innerException)
         : base(message, innerException)
      {
      }
   }
}
