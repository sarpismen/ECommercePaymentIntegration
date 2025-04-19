using ECommercePaymentIntegration.Application.Interfaces.BalanceManagement;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.DTO.PaymentIntegration
{
   public class PaymentIntegrationService
   {
      private readonly IBalanceManagementService _balanceManagementService;
      private readonly ILogger<PaymentIntegrationService> _logger;

      public PaymentIntegrationService(ILogger<PaymentIntegrationService> logger, IBalanceManagementService balanceManagementService)
      {
         _balanceManagementService = balanceManagementService;
         _logger = logger;
      }
   }
}
