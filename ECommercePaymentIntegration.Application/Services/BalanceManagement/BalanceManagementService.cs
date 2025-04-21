using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.Responses;
using ECommercePaymentIntegration.Application.Exceptions;
using ECommercePaymentIntegration.Application.Interfaces.BalanceManagement;
using ECommercePaymentIntegration.Application.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;

namespace ECommerceApp.Infrastructure.BalanceManagement
{
   public class BalanceManagementService : IBalanceManagementService
   {
      private static readonly HashSet<HttpStatusCode> RetryStatusCodes = new HashSet<HttpStatusCode>
      {
         HttpStatusCode.RequestTimeout, // 408
         HttpStatusCode.InternalServerError, // 500
         HttpStatusCode.BadGateway, // 502
         HttpStatusCode.ServiceUnavailable, // 503
         HttpStatusCode.GatewayTimeout, // 504
      };

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
         return await ResilientHttpOperationAsync<T>(() => _httpClient.GetAsync(endpoint));
      }

      private async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
      {
         return await ResilientHttpOperationAsync<TResponse>(() => _httpClient.PostAsJsonAsync(endpoint, request, JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions));
      }

      private async Task<T> ResilientHttpOperationAsync<T>(Func<Task<HttpResponseMessage>> action)
      {
         var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(30), TimeoutStrategy.Optimistic);

         var retryPolicy = Policy
             .Handle<HttpRequestException>(ex => ex.StatusCode.HasValue && RetryStatusCodes.Contains(ex.StatusCode.Value))
             .Or<TimeoutRejectedException>()
             .WaitAndRetryAsync(
                 retryCount: 5,
                 sleepDurationProvider: attempt => attempt > 2 ? TimeSpan.FromSeconds(Math.Pow(2, attempt - 2)) : TimeSpan.Zero,
                 onRetry: (exception, timespan, retryAttempt, context) =>
                 {
                    _logger.LogInformation($"Retry #{retryAttempt} due to: {exception.Message}");
                 });
         var combinedPolicy = Policy.WrapAsync(retryPolicy, timeoutPolicy);
         try
         {
            var result = await combinedPolicy.ExecuteAsync(async () => await HttpOperationAsync<T>(action));
            return result;
         }
         catch (Exception ex)
         {
            throw new BalanceManagementServiceException("An error occured while sending request", ex);
         }
      }

      private async Task<T> HttpOperationAsync<T>(Func<Task<HttpResponseMessage>> action)
      {
         try
         {
            var response = await action();
            //Throws if error
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<Response<T>>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
            return result.Success ? result.Data : throw new BalanceManagementServiceException();
         }
         catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
         {
            return default;
         }
      }
   }
}