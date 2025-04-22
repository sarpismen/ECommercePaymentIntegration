using ECommerceApp.Infrastructure.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Enums;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration;
using ECommercePaymentIntegration.Application.Interfaces.BalanceManagement;
using ECommercePaymentIntegration.Domain.Entities.Product;
using ECommercePaymentIntegration.Infrastructure;
using ECommercePaymentIntegration.Tests.Integration.ApplicationFactories;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WireMock.Admin.Mappings;
using WireMock.Client;
using WireMock.Client.Extensions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using static System.Net.WebRequestMethods;
using ECommercePaymentIntegration.Application.Json;
using FluentAssertions;
using ECommercePaymentIntegration.Application.DTO.Responses;
namespace ECommercePaymentIntegration.Tests.Integration
{
   public class ECommercePaymentFailureTests
   {
      private WireMockServer _server;
      private TestingAspireAppHostFactory _testHost;
      private string _dbConnectionString;
      private HttpClient _httpClient;

      [OneTimeSetUp]
      public void OneTimeSetUp()
      {
         _server = WireMockServer.Start();
      }

      [SetUp]
      public async Task Setup()
      {
         var proxyUrl = "https://balance-management-pi44.onrender.com";
         _server.Given(Request.Create().WithPath("/api/products")).RespondWith(Response.Create().WithProxy(proxyUrl));
         _server.Given(Request.Create().WithPath("/api/balance")).RespondWith(Response.Create().WithProxy(proxyUrl));
         _server.Given(Request.Create().WithPath("/api/balance/preorder")).RespondWith(Response.Create().WithProxy(proxyUrl));
         _server.Given(Request.Create().WithPath("/api/balance/complete")).RespondWith(Response.Create().WithStatusCode(500).WithBodyAsJson(new ServerErrorResponse()));
         _server.Given(Request.Create().WithPath("/api/balance/cancel")).RespondWith(Response.Create().WithProxy(proxyUrl));
         _testHost = new TestingAspireAppHostFactory(_server.Url);
         await _testHost.StartAsync();
         _dbConnectionString = await _testHost.GetConnectionString("ECommercePaymentIntegrationTest");
         _httpClient = _testHost.CreateHttpClient("apiservice");
      }

      [TearDown]
      public void TearDown()
      {
         _testHost.Dispose();
         _httpClient.Dispose();
      }

      [OneTimeTearDown]
      public void OneTimeTearDown()
      {
         _server.Stop();
         _server.Dispose();
      }

      [Test]
      public async Task CompleteOrder_InvokesCancel_WhenFailed()
      {
         var productsResponse = await _httpClient.GetAsync("/api/products");
         var products = await productsResponse.Content.ReadFromJsonAsync<IEnumerable<Product>>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var product = products.FirstOrDefault();

         var createOrderRequest = new CreateOrderRequest
         {
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = product.ProductId, Quantity = 1 }, }
         };

         //var balance = await _balanceManagementService.GetBalanceAsync();

         var orderResponse = await _httpClient.PostAsJsonAsync("/api/orders/create", createOrderRequest, JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var preorderInfo = await orderResponse.Content.ReadFromJsonAsync<PreOrderResultDto>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);

         var completeOrderRepsonse = await _httpClient.PostAsJsonAsync($"/api/orders/{preorderInfo.PreOrder.OrderId}/complete", JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var completeOrderInfo = await completeOrderRepsonse.Content.ReadFromJsonAsync<OrderResultDto>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);

         completeOrderInfo.Order.Status.Should().Be(PreOrderStatus.Cancelled);
      }
   }
}
