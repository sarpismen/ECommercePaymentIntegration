using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.DTO.Responses
{
   public abstract class ResponseBase<T>
   {
      public T Data { get; set; }
      public bool Success { get; set; }
   }
}
