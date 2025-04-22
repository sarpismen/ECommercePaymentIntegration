using System;
using System.Collections.Generic;
using System.Linq;
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
using ECommercePaymentIntegration.Infrastructure;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;

namespace ECommerceApp.Infrastructure.BalanceManagement
{
   public class BalanceManagementService : IBalanceManagementService
   {
      private readonly HttpClient _httpClient;
      private readonly ILogger<BalanceManagementService> _logger;
      private static IEnumerable<HttpStatusCode> TimeoutStatusCodes
      {
         get
         {
            yield return HttpStatusCode.GatewayTimeout;
            yield return HttpStatusCode.RequestTimeout;
         }
      }

      public BalanceManagementService(IHttpClientFactory httpClientFactory, ILogger<BalanceManagementService> logger)
      {
         _httpClient = httpClientFactory.CreateClient(HttpClients.BalanceManagementApi);
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
         var retryPolicy = Policy
             .Handle<BalanceManagementServiceException>(x => !TimeoutStatusCodes.Contains(x.HttpStatusCode))
             .WaitAndRetryAsync(
                 retryCount: 3,
                 sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                 onRetry: (exception, timespan, retryAttempt, context) =>
                 {
                    _logger.LogInformation($"Retry #{retryAttempt} due to: {exception.Message}");
                 });
         var result = await retryPolicy.ExecuteAsync(async () => await HttpOperationAsync<T>(action));
         return result;

      }

      private async Task<T> HttpOperationAsync<T>(Func<Task<HttpResponseMessage>> action)
      {
         var response = await action();
         try
         {
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<Response<T>>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
            return result.Success ? result.Data : throw new BalanceManagementServiceException();
         }
         catch (HttpRequestException ex)
         {
            var errorResponse = await response.Content.ReadFromJsonAsync<ServerErrorResponse>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
            switch (ex.StatusCode)
            {
               case HttpStatusCode.NotFound:
                  throw new NotFoundException($"Balance management service threw an error: {errorResponse.Message}", errorResponse.Error);
               case HttpStatusCode.BadRequest:
                  throw new BadRequestException($"Balance management service threw an error: {errorResponse.Message}", errorResponse.Error);
               default:
                  throw new BalanceManagementServiceException(ex.StatusCode ?? HttpStatusCode.InternalServerError, $"Balance management service threw an error: {errorResponse.Message}", errorResponse.Error);
            }
         }
      }

   }
}