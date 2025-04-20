using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace ECommercePaymentIntegration.Application.DTO.BalanceManagement.Enums
{
   [JsonConverter(typeof(StringEnumConverter))]
   public enum PreOrderStatus
   {
      Blocked,
      Completed,
      Cancelled,
   }
}
