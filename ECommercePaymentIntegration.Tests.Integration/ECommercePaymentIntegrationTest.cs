using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Azure;
using ECommerceApp.Infrastructure.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Enums;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;
using ECommercePaymentIntegration.Application.DTO.Responses;
using ECommercePaymentIntegration.Application.Exceptions;
using ECommercePaymentIntegration.Application.Interfaces.BalanceManagement;
using ECommercePaymentIntegration.Application.Json;
using ECommercePaymentIntegration.Domain.Entities.Order;
using ECommercePaymentIntegration.Domain.Entities.Product;
using ECommercePaymentIntegration.Domain.ValueObjects.Order;
using ECommercePaymentIntegration.Infrastructure;
using ECommercePaymentIntegration.Tests.Integration.ApplicationFactories;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ECommercePaymentIntegration.Tests.Integration
{
   public class ECommercePaymentIntegrationTest
   {
      private string _dbConnectionString;
      private HttpClient _httpClient;
      private BalanceManagementService _balanceManagementService;
      private TestingAspireAppHostFactory _testHost;

      [SetUp]
      public async Task SetUp()
      {

         _testHost = new TestingAspireAppHostFactory("https://balance-management-pi44.onrender.com");

         await _testHost.StartAsync();
         _dbConnectionString = await _testHost.GetConnectionString("ECommercePaymentIntegrationTest");
         _httpClient = _testHost.CreateHttpClient("apiservice");


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

         var products = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
         products.Should().NotBeNullOrEmpty();
      }
      [Test]
      public async Task CreateOrderEndpoint_CreatesPreorder_WhenInvoked()
      {
         var productsResponse = await _httpClient.GetAsync("/api/products");
         var products = await productsResponse.Content.ReadFromJsonAsync<IEnumerable<Product>>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var product = products.FirstOrDefault();

         var request = new CreateOrderRequest
         {
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = product.ProductId, Quantity = 1 }, },
         };

         var balance = await _balanceManagementService.GetBalanceAsync();

         var orderResponse = await _httpClient.PostAsJsonAsync("/api/orders/create", request, JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var expectedAvailableBalance = balance.AvailableBalance - product.ItemPrice;
         var expectedBlockedBalance = balance.BlockedBalance + product.ItemPrice;

         orderResponse.StatusCode.Should().Be(HttpStatusCode.OK);
         var orderInfo = await orderResponse.Content.ReadFromJsonAsync<PreOrderResultDto>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);

         orderInfo.Should().NotBeNull();
         orderInfo.PreOrder.Should().NotBeNull();
         orderInfo.PreOrder.Amount.Should().Be(product.ItemPrice);
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
      public async Task CompleteOrderEndpoint_CompletesPreorder_WhenInvoked()
      {
         var productsResponse = await _httpClient.GetAsync("/api/products");
         var products = await productsResponse.Content.ReadFromJsonAsync<IEnumerable<Product>>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var product = products.FirstOrDefault();

         var createOrderRequest = new CreateOrderRequest
         {
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = product.ProductId, Quantity = 1 }, }
         };

         var balance = await _balanceManagementService.GetBalanceAsync();

         var orderResponse = await _httpClient.PostAsJsonAsync("/api/orders/create", createOrderRequest, JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var expectedAvailableBalance = balance.AvailableBalance - product.ItemPrice;
         var expectedTotalBalance = balance.TotalBalance - product.ItemPrice;
         var preorderInfo = await orderResponse.Content.ReadFromJsonAsync<PreOrderResultDto>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);

         var completeOrderRepsonse = await _httpClient.PostAsJsonAsync($"/api/orders/{preorderInfo.PreOrder.OrderId}/complete", JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var completeOrderInfo = await completeOrderRepsonse.Content.ReadFromJsonAsync<OrderResultDto>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         completeOrderRepsonse.StatusCode.Should().Be(HttpStatusCode.OK);
         completeOrderInfo.Should().NotBeNull();
         completeOrderInfo.Order.Should().NotBeNull();
         completeOrderInfo.Order.Amount.Should().Be(product.ItemPrice);
         completeOrderInfo.Order.Status.Should().Be(PreOrderStatus.Completed);
         completeOrderInfo.UpdatedBalance.Should().NotBeNull();
         completeOrderInfo.UpdatedBalance.BlockedBalance.Should().BeApproximately(preorderInfo.UpdatedBalance.BlockedBalance - product.ItemPrice, 3);
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
         var products = await productsResponse.Content.ReadFromJsonAsync<IEnumerable<Product>>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var product = products.FirstOrDefault();

         var request = new CreateOrderRequest
         {
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = product.ProductId, Quantity = 0 }, }
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
         var products = await productsResponse.Content.ReadFromJsonAsync<IEnumerable<Product>>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var product = products.FirstOrDefault();

         var request = new CreateOrderRequest
         {
            Items = new List<OrderItemDto> { }
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
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = "unittestyazanrobot", Quantity = 1 }, }
         };

         var balance = await _balanceManagementService.GetBalanceAsync();

         var orderResponse = await _httpClient.PostAsJsonAsync("/api/orders/create", request, JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         var orderInfo = await orderResponse.Content.ReadFromJsonAsync<ErrorResponse>(JsonSerializerSettings.BalanceManagementServiceJsonSerializerOptions);
         orderResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
         orderResponse.Should().NotBeNull();
      }

      [TearDown]
      public async Task TearDown()
      {
         _httpClient.Dispose();
         await _testHost.DisposeAsync();
      }
   }
}
