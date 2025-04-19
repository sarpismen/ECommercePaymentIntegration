using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.Responses;
using ECommercePaymentIntegration.Application.Interfaces.BalanceManagement;
using Microsoft.Extensions.Logging;

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
         return await GetAsync<IEnumerable<ProductDto>>("/products");
      }

      public async Task<PreOrderDto> PreorderAsync(PreorderRequest request)
      {
         return await PostAsync<PreorderRequest, PreOrderDto>("/balance/preorder", request);
      }

      public async Task<PreOrderDto> CancelOrderAsync(CancelOrderRequest request)
      {
         return await PostAsync<CancelOrderRequest, PreOrderDto>("/balance/cancel", request);
      }

      public async Task<PreOrderDto> CompleteOrderAsync(CompleteOrderRequest request)
      {
         return await PostAsync<CompleteOrderRequest, PreOrderDto>("/balance/complete", request);
      }

      public async Task<UserBalanceDto> GetBalanceAsync()
      {
         return await GetAsync<UserBalanceDto>("/balance");
      }

      private async Task<T> GetAsync<T>(string endpoint)
      {
         return await HttpOperationAsync<T>(() => _httpClient.GetAsync(endpoint));
      }

      private async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
      {
         return await HttpOperationAsync<TResponse>(() => _httpClient.PostAsJsonAsync(endpoint, request));
      }

      private async Task<T> HttpOperationAsync<T>(Func<Task<HttpResponseMessage>> action)
      {
         {
            try
            {
               var response = await action();
               response.EnsureSuccessStatusCode();
               var result = await response.Content.ReadFromJsonAsync<ResponseBase<T>>();
               //Todo: Add custom extception and better exception handling
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
                     throw new Exception();
               }

               throw new Exception();
            }
            catch (Exception ex)
            {
               _logger.LogError($"Unexpected error: {ex.Message}");
               throw;
            }
         }
      }
   }
}