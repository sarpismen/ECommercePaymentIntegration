using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Enums;

namespace ECommercePaymentIntegration.Application.DTO.BalanceManagement
{
   public class PreOrderResultDto : OrderResultDtoBase
   {
      public OrderDto PreOrder { get; set; }
   }
}
