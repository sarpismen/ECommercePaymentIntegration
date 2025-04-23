using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ECommerceApp.Infrastructure.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Enums;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;
using ECommercePaymentIntegration.Application.DTO.Responses;
using ECommercePaymentIntegration.Application.Json;
using ECommercePaymentIntegration.Domain.ValueObjects.Order;
using ECommercePaymentIntegration.Tests.Integration.ApplicationFactories;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ECommercePaymentIntegration.Tests.Integration
{
   public class ECommercePaymentIntegrationTest
   {
      private string _dbConnectionString;
      private HttpClient _httpClient;
      private BalanceManagementService _balanceManagementService;
      private TestingAspireAppHostFactory _testHost;
      private WireMockServer _server;

      [OneTimeSetUp]
      public void OneTimeSetUp()
      {
         _server = WireMockServer.Start();
      }

      [SetUp]
      public async Task SetUp()
      {
         var proxyUrl = "https://balance-management-pi44.onrender.com";
         _server.Given(Request.Create().WithPath("/api/products")).RespondWith(Response.Create().WithProxy(proxyUrl));
         _server.Given(Request.Create().WithPath("/api/balance")).RespondWith(Response.Create().WithProxy(proxyUrl));
         _server.Given(Request.Create().WithPath("/api/balance/preorder")).RespondWith(Response.Create().WithProxy(proxyUrl));
         _server.Given(Request.Create().WithPath("/api/balance/complete")).RespondWith(Response.Create().WithProxy(proxyUrl));
         _server.Given(Request.Create().WithPath("/api/balance/cancel")).RespondWith(Response.Create().WithProxy(proxyUrl));
         _testHost = new TestingAspireAppHostFactory(_server.Url);
         await _testHost.StartAsync();
         _dbConnectionString = await _testHost.GetConnectionString("ECommercePaymentIntegrationTest");
         var hostHttpClient = _testHost.CreateHttpClient("apiservice");
         _httpClient = new HttpClient();
         _httpClient.BaseAddress = hostHttpClient.BaseAddress;

         var httpClientFactoryMock = new Mock<IHttpClientFactory>();
         httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                 .Returns(new HttpClient() { BaseAddress = new Uri("https://balance-management-pi44.onrender.com") }).Verifiable();
         _balanceManagementService = new BalanceManagementService(httpClientFactoryMock.Object, new Mock<ILogger<BalanceManagementService>>().Object);
      }

      [Test]
      public async Task ProductsEndpoint_ReturnsListOfProducts_WhenInvoked()
      {
         var response = await _httpClient.GetAsync("/api/products");

         response.StatusCode.Should().Be(HttpStatusCode.OK);

         var products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>();
         products.Should().NotBeNullOrEmpty();
      }

      [Test]
      public async Task CreateOrderEndpoint_CreatesPreorder_WhenInvoked()
      {
         var productsResponse = await _httpClient.GetAsync("/api/products");
         var products = await productsResponse.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var product = products.FirstOrDefault();

         var request = new CreateOrderRequest
         {
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = product.Id, Quantity = 1 }, },
         };

         var balance = await _balanceManagementService.GetBalanceAsync();

         var orderResponse = await _httpClient.PostAsJsonAsync("/api/orders/create", request, JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var expectedAvailableBalance = balance.AvailableBalance - product.Price;
         var expectedBlockedBalance = balance.BlockedBalance + product.Price;

         orderResponse.StatusCode.Should().Be(HttpStatusCode.OK);
         var orderInfo = await orderResponse.Content.ReadFromJsonAsync<PreOrderResultDto>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);

         orderInfo.Should().NotBeNull();
         orderInfo.PreOrder.Should().NotBeNull();
         orderInfo.PreOrder.Amount.Should().Be(product.Price);
         orderInfo.PreOrder.Status.Should().Be(PreOrderStatus.Blocked);
         orderInfo.UpdatedBalance.Should().NotBeNull();
         orderInfo.UpdatedBalance.BlockedBalance.Should().BeApproximately(expectedBlockedBalance, 3);
         orderInfo.UpdatedBalance.AvailableBalance.Should().BeApproximately(expectedAvailableBalance, 3);

         using var connection = new SqlConnection(_dbConnectionString);
         await connection.OpenAsync();
         using var command = connection.CreateCommand();
         command.CommandText = $"SELECT Status FROM Orders WHERE OrderId = '{orderInfo.PreOrder.OrderId}'";
         var orderStatus = (int?)await command.ExecuteScalarAsync();
         orderStatus.Should().NotBeNull();
         orderStatus.Should().Be((int)OrderStatus.Preordered);
         command.CommandText = $"SELECT Count(*) FROM OrderItems WHERE OrderId = '{orderInfo.PreOrder.OrderId}'";
         var orderItemsCount = (int?)await command.ExecuteScalarAsync();
         orderItemsCount.Should().Be(1);
         //Rollback
         await _balanceManagementService.CancelOrderAsync(new CancelOrderRequest { OrderId = orderInfo.PreOrder.OrderId });
      }

      [Test]
      public async Task CreateOrderEndpoint_UpdatesStatusToFailed_WhenExceptionIsThrown()
      {
         _server.Given(Request.Create().WithPath("/api/balance/preorder")).RespondWith(Response.Create().WithStatusCode(500).WithBodyAsJson(new ServerErrorResponse()));
         var productsResponse = await _httpClient.GetAsync("/api/products");
         var products = await productsResponse.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var product = products.FirstOrDefault();

         var request = new CreateOrderRequest
         {
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = product.Id, Quantity = 1 }, },
         };

         var balance = await _balanceManagementService.GetBalanceAsync();

         var orderResponse = await _httpClient.PostAsJsonAsync("/api/orders/create", request, JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var expectedAvailableBalance = balance.AvailableBalance - product.Price;
         var expectedBlockedBalance = balance.BlockedBalance + product.Price;
         var errorResponse = await orderResponse.Content.ReadFromJsonAsync<ErrorResponse>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);

         errorResponse.Should().NotBeNull();

         using var connection = new SqlConnection(_dbConnectionString);
         await connection.OpenAsync();
         using var command = connection.CreateCommand();
         command.CommandText = $"SELECT Status FROM Orders WHERE OrderId = '{errorResponse.OrderId}'";
         var orderStatus = (int?)await command.ExecuteScalarAsync();
         orderStatus.Should().NotBeNull();
         orderStatus.Should().Be((int)OrderStatus.Failed);
      }

      [Test]
      public async Task CompleteOrderEndpoint_CompletesPreorder_WhenInvoked()
      {
         var productsResponse = await _httpClient.GetAsync("/api/products");
         var products = await productsResponse.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var product = products.FirstOrDefault();

         var createOrderRequest = new CreateOrderRequest
         {
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = product.Id, Quantity = 1 }, },
         };

         var balance = await _balanceManagementService.GetBalanceAsync();

         var orderResponse = await _httpClient.PostAsJsonAsync("/api/orders/create", createOrderRequest, JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var expectedAvailableBalance = balance.AvailableBalance - product.Price;
         var expectedTotalBalance = balance.TotalBalance - product.Price;
         var preorderInfo = await orderResponse.Content.ReadFromJsonAsync<PreOrderResultDto>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);

         var completeOrderRepsonse = await _httpClient.PostAsJsonAsync($"/api/orders/{preorderInfo.PreOrder.OrderId}/complete", JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var completeOrderInfo = await completeOrderRepsonse.Content.ReadFromJsonAsync<OrderResultDto>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         completeOrderRepsonse.StatusCode.Should().Be(HttpStatusCode.OK);
         completeOrderInfo.Should().NotBeNull();
         completeOrderInfo.Order.Should().NotBeNull();
         completeOrderInfo.Order.Amount.Should().Be(product.Price);
         completeOrderInfo.Order.Status.Should().Be(PreOrderStatus.Completed);
         completeOrderInfo.UpdatedBalance.Should().NotBeNull();
         completeOrderInfo.UpdatedBalance.BlockedBalance.Should().BeApproximately(preorderInfo.UpdatedBalance.BlockedBalance - product.Price, 3);
         completeOrderInfo.UpdatedBalance.AvailableBalance.Should().BeApproximately(expectedAvailableBalance, 3);
         completeOrderInfo.UpdatedBalance.TotalBalance.Should().BeApproximately(expectedTotalBalance, 3);
         using var connection = new SqlConnection(_dbConnectionString);
         await connection.OpenAsync();
         using var command = connection.CreateCommand();
         command.CommandText = $"SELECT Status FROM Orders WHERE OrderId = '{completeOrderInfo.Order.OrderId}'";
         var orderStatus = (int?)await command.ExecuteScalarAsync();
         orderStatus.Should().NotBeNull();
         orderStatus.Should().Be((int)OrderStatus.Completed);
         command.CommandText = $"SELECT Count(*) FROM OrderItems WHERE OrderId = '{completeOrderInfo.Order.OrderId}'";
         var orderItemsCount = (int?)await command.ExecuteScalarAsync();
         orderItemsCount.Should().Be(1);
      }

      [Test]
      public async Task CompleteOrderEndpoint_ReturnsNotFound_WhenInvokedWithNotExistingId()
      {
         var completeOrderRepsonse = await _httpClient.PostAsJsonAsync($"/api/orders/hello123/complete", JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var completeOrderInfo = await completeOrderRepsonse.Content.ReadFromJsonAsync<ErrorResponse>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);

         completeOrderRepsonse.StatusCode.Should().Be(HttpStatusCode.NotFound);
         completeOrderInfo.Should().NotBeNull();
      }

      [Test]
      public async Task CreateOrderEndpoint_ReturnsBadRequest_WhenInvokedWithZeroQuantity()
      {
         var productsResponse = await _httpClient.GetAsync("/api/products");
         var products = await productsResponse.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var product = products.FirstOrDefault();

         var request = new CreateOrderRequest
         {
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = product.Id, Quantity = 0 }, },
         };

         var balance = await _balanceManagementService.GetBalanceAsync();

         var orderResponse = await _httpClient.PostAsJsonAsync("/api/orders/create", request, JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var orderInfo = await orderResponse.Content.ReadFromJsonAsync<ErrorResponse>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         orderResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
         orderResponse.Should().NotBeNull();
      }

      [Test]
      public async Task CreateOrderEndpoint_ReturnsBadRequest_WhenInvokedWithEmptyList()
      {
         var productsResponse = await _httpClient.GetAsync("/api/products");
         var products = await productsResponse.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var product = products.FirstOrDefault();

         var request = new CreateOrderRequest
         {
            Items = new List<OrderItemDto>(),
         };

         var balance = await _balanceManagementService.GetBalanceAsync();

         var orderResponse = await _httpClient.PostAsJsonAsync("/api/orders/create", request, JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var orderInfo = await orderResponse.Content.ReadFromJsonAsync<ErrorResponse>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         orderResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
         orderResponse.Should().NotBeNull();
      }

      [Test]
      public async Task CreateOrderEndpoint_ReturnsNotFound_WhenInvokedNonExistentItem()
      {
         var request = new CreateOrderRequest
         {
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = "unittestyazanrobot", Quantity = 1 }, },
         };

         var balance = await _balanceManagementService.GetBalanceAsync();

         var orderResponse = await _httpClient.PostAsJsonAsync("/api/orders/create", request, JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var orderInfo = await orderResponse.Content.ReadFromJsonAsync<ErrorResponse>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         orderResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
         orderResponse.Should().NotBeNull();
      }

      [Test]
      public async Task CompleteOrder_InvokesCancel_WhenFailed()
      {
         _server.Given(Request.Create().WithPath("/api/balance/complete")).RespondWith(Response.Create().WithStatusCode(500).WithBodyAsJson(new ServerErrorResponse()));
         var productsResponse = await _httpClient.GetAsync("/api/products");
         var products = await productsResponse.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var product = products.FirstOrDefault();

         var createOrderRequest = new CreateOrderRequest
         {
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = product.Id, Quantity = 1 }, },
         };

         var balance = await _balanceManagementService.GetBalanceAsync();

         var expectedAvailableBalance = balance.AvailableBalance - product.Price;

         var orderResponse = await _httpClient.PostAsJsonAsync("/api/orders/create", createOrderRequest, JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var preorderInfo = await orderResponse.Content.ReadFromJsonAsync<PreOrderResultDto>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);

         var completeOrderRepsonse = await _httpClient.PostAsJsonAsync($"/api/orders/{preorderInfo.PreOrder.OrderId}/complete", JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var completeOrderInfo = await completeOrderRepsonse.Content.ReadFromJsonAsync<OrderResultDto>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);

         completeOrderInfo.Order.Status.Should().Be(PreOrderStatus.Cancelled);
         completeOrderInfo.Should().NotBeNull();
         completeOrderInfo.Order.Should().NotBeNull();
         completeOrderInfo.Order.Amount.Should().Be(product.Price);
         completeOrderInfo.Order.Status.Should().Be(PreOrderStatus.Cancelled);
         completeOrderInfo.UpdatedBalance.Should().NotBeNull();
         completeOrderInfo.UpdatedBalance.BlockedBalance.Should().BeApproximately(preorderInfo.UpdatedBalance.BlockedBalance - product.Price, 3);
         completeOrderInfo.UpdatedBalance.AvailableBalance.Should().BeApproximately(balance.AvailableBalance, 3);
         completeOrderInfo.UpdatedBalance.TotalBalance.Should().BeApproximately(balance.TotalBalance, 3);
         using var connection = new SqlConnection(_dbConnectionString);
         await connection.OpenAsync();
         using var command = connection.CreateCommand();
         command.CommandText = $"SELECT Status FROM Orders WHERE OrderId = '{completeOrderInfo.Order.OrderId}'";
         var orderStatus = (int?)await command.ExecuteScalarAsync();
         orderStatus.Should().NotBeNull();
         orderStatus.Should().Be((int)OrderStatus.Cancelled);
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
   }
}
