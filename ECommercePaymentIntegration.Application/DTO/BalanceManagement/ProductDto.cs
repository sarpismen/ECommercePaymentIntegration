using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.DTO.BalanceManagement
{
   public class ProductDto
   {
      public int Id { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
      public string Currency { get; set; }
      public string Category { get; set; }
      public int Stock { get; set; }

   }
}
