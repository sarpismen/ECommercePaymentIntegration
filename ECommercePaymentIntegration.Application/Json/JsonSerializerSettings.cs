using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ECommercePaymentIntegration.Application.Json
{
   public static class JsonSerializerSettings
   {
      public static JsonSerializerOptions BalanceManagementServiceJsonSerializerOptions => new JsonSerializerOptions
      {
         PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
         PropertyNameCaseInsensitive = true,
         Converters = { new JsonStringEnumConverter() },
      };
   }
}
