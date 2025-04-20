using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests
{
   public class PreorderRequest : OrderRequestBase
   {
      public decimal Amount { get; set; }

      public CancelOrderRequest ToCancelRequest() => new CancelOrderRequest { OrderId = OrderId };

      public CompleteOrderRequest ToCompleteOrderRequest() => new CompleteOrderRequest { OrderId = OrderId };
   }
}
