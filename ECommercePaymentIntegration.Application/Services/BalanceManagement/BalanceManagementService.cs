using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.Responses;
using ECommercePaymentIntegration.Application.Exceptions;
using ECommercePaymentIntegration.Application.Interfaces.BalanceManagement;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ECommerceApp.Infrastructure.BalanceManagement
{
   public class BalanceManagementService : IBalanceManagementService
   {
      private readonly HttpClient _httpClient;
      private readonly ILogger<BalanceManagementService> _logger;

      public BalanceManagementService(IHttpClientFactory httpClientFactory, ILogger<BalanceManagementService> logger)
      {
         _httpClient = httpClientFactory.CreateClient("BalanceManagementApi");
         _logger = logger;
      }

      public async Task<IEnumerable<ProductDto>> GetProductsAsync()
      {
         return await GetAsync<IEnumerable<ProductDto>>("/api/products");
      }

      public async Task<PreOrderResultDto> PreorderAsync(PreorderRequest request)
      {
         return await PostAsync<PreorderRequest, PreOrderResultDto>("/api/balance/preorder", request);
      }

      public async Task<OrderResultDto> CancelOrderAsync(CancelOrderRequest request)
      {
         return await PostAsync<CancelOrderRequest, OrderResultDto>("/api/balance/cancel", request);
      }

      public async Task<OrderResultDto> CompleteOrderAsync(CompleteOrderRequest request)
      {
         return await PostAsync<CompleteOrderRequest, OrderResultDto>("/api/balance/complete", request);
      }

      public async Task<UserBalanceDto> GetBalanceAsync()
      {
         return await GetAsync<UserBalanceDto>("/api/balance");
      }

      private async Task<T> GetAsync<T>(string endpoint)
      {
         var settings = new JsonSerializerSettings();
         settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
         return await HttpOperationAsync<T>(() => _httpClient.GetAsync(endpoint), settings);
      }

      private async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
      {
         var settings = new JsonSerializerSettings();
         settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
         string jsonPayload = JsonConvert.SerializeObject(request, Formatting.Indented, settings);
         var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
         return await HttpOperationAsync<TResponse>(() => _httpClient.PostAsync(endpoint, content), settings);
      }

      private async Task<T> HttpOperationAsync<T>(Func<Task<HttpResponseMessage>> action, JsonSerializerSettings jsonSerializerSettings)
      {
         {
            try
            {
               var response = await action();
               //Throws if error
               response.EnsureSuccessStatusCode();
               string jsonContent = await response.Content.ReadAsStringAsync();
               var result = JsonConvert.DeserializeObject<Response<T>>(jsonContent, jsonSerializerSettings);
               return result.Success ? result.Data : throw new Exception();
            }
            catch (HttpRequestException ex)
            {
               _logger.LogError($"Error calling Balance Management API: {ex.Message}");
               switch (ex.StatusCode)
               {
                  case System.Net.HttpStatusCode.NotFound:
                     return default;
                  default:
                     throw new BalanceManagementServiceException("Service returned error", ex);
               }
            }
            catch (Exception ex)
            {
               _logger.LogError(ex, "Balance Management Service encountered an unexpected error");
               throw;
            }
         }
      }
   }
}