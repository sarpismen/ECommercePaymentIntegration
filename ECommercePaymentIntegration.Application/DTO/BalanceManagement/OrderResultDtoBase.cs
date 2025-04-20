using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.DTO.BalanceManagement
{
   public abstract class OrderResultDtoBase
   {
      public UserBalanceDto UpdatedBalance { get; set; }
   }
}
