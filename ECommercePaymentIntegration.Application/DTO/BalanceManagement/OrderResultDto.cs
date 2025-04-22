using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Enums;
using ECommercePaymentIntegration.Application.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.DTO.BalanceManagement
{
   public class OrderResultDto : OrderResultDtoBase
   {
      public OrderStatusDto Order { get; set; }
   }
}
