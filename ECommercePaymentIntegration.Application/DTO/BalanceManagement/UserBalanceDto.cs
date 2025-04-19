using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.DTO.BalanceManagement
{
   public class UserBalanceDto
   {
      public string UserId { get; set; }
      public decimal TotalBalance { get; set; }
      public decimal AvailableBalance { get; set; }
      public decimal BlockedBalance { get; set; }
      public string Currency { get; set; }
      public DateTime LastUpdated { get; set; }
   }
}
