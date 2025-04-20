using ECommercePaymentIntegration.Application.Interfaces.BalanceManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.Services.PaymentIntegration
{
   public class PaymentIntegrationService
   {
      private IBalanceManagementService _balanceManagementService;

      public PaymentIntegrationService(IBalanceManagementService balanceManagementService)
      {
         _balanceManagementService = balanceManagementService;
      }
   }
}
