using System;
using System.Net;

namespace ECommercePaymentIntegration.Application.Exceptions
{
   public class BalanceManagementServiceException : ServiceExceptionBase
   {
      public BalanceManagementServiceException(HttpStatusCode httpStatusCode)
         : base(httpStatusCode)
      {
      }

      public BalanceManagementServiceException(HttpStatusCode httpStatusCode, string message)
         : base(httpStatusCode, message)
      {
      }

      public BalanceManagementServiceException(HttpStatusCode httpStatusCode, string message, string error)
         : base(httpStatusCode, message, error)
      {
      }

      public BalanceManagementServiceException(HttpStatusCode httpStatusCode, string message, Exception innerException)
         : base(httpStatusCode, message, innerException)
      {
      }

      public BalanceManagementServiceException()
         : base(HttpStatusCode.InternalServerError)
      {
      }

      public BalanceManagementServiceException(string message)
         : base(HttpStatusCode.InternalServerError, message)
      {
      }

      public BalanceManagementServiceException(string message, string error)
         : base(HttpStatusCode.InternalServerError, message, error)
      {
      }

      public BalanceManagementServiceException(string message, Exception innerException)
         : base(HttpStatusCode.InternalServerError, message, innerException)
      {
      }
   }
}
