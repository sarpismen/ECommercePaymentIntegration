using System.Text.Json;
using System.Text.Json.Serialization;

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
