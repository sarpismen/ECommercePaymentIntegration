using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests
{
   public abstract class OrderRequestBase
   {
      public string OrderId { get; set; }
   }
}
