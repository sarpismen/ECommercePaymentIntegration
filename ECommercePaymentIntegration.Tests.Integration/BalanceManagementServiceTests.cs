using System;
using System.Net.Http;
using System.Threading.Tasks;
using ECommerceApp.Infrastructure.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.Exceptions;
using ECommercePaymentIntegration.Application.Interfaces.BalanceManagement;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ECommercePaymentIntegration.Tests.Integration
{
   public class BalanceManagementServiceTests
   {
      private IBalanceManagementService _balanceManagementService;

      [SetUp]
      public void Setup()
      {
         var httpClientFactoryMock = new Mock<IHttpClientFactory>();
         var httpClient = new HttpClient();
         httpClient.BaseAddress = new Uri("https://balance-management-pi44.onrender.com");
         httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                 .Returns(httpClient).Verifiable();
         _balanceManagementService = new BalanceManagementService(httpClientFactoryMock.Object, new Mock<ILogger<BalanceManagementService>>().Object);
      }

      [Retry(5)]
      [Test]
      public async Task GetProductsAsync_ShouldReturn_ListOfProducts()
      {
         var getProducts = async () => await _balanceManagementService.GetProductsAsync();
         var assertion = await getProducts.Should().NotThrowAsync();
         assertion.Subject.Should().NotBeEmpty();
      }

      [Retry(5)]
      [Test]
      public async Task GetBalanceAsync_ShouldReturn_BalanceObject()
      {
         var command = async () => await _balanceManagementService.GetBalanceAsync();
         var assertion = await command.Should().NotThrowAsync();
         assertion.Subject.Should().NotBeNull();
      }

      [Retry(5)]
      [Test]
      public async Task PreOrderAndCancel_ShouldReturn_Responses()
      {
         var preOrderRequest = new PreorderRequest()
         {
            Amount = 1,
            OrderId = "1",
         };
         var preOrderCommand = async () => await _balanceManagementService.PreorderAsync(preOrderRequest);
         var preorderAssertion = await preOrderCommand.Should().NotThrowAsync();
         preorderAssertion.Subject.Should().NotBeNull();
         preorderAssertion.Subject.UpdatedBalance.Should().NotBeNull();
         preorderAssertion.Subject.PreOrder.Should().NotBeNull();
         var cancelRequest = preOrderRequest.ToCancelRequest();
         var cancelCommand = async () => await _balanceManagementService.CancelOrderAsync(cancelRequest);
         var cancelAssertion = await cancelCommand.Should().NotThrowAsync();
         cancelAssertion.Subject.Should().NotBeNull();
         cancelAssertion.Subject.UpdatedBalance.Should().NotBeNull();
         cancelAssertion.Subject.Order.Should().NotBeNull();
      }

      [Retry(5)]
      [Test]
      public async Task PreOrderAndComplete_ShouldReturn_Responses()
      {
         var preOrderRequest = new PreorderRequest()
         {
            Amount = 1,
            OrderId = "1",
         };
         var preOrderCommand = async () => await _balanceManagementService.PreorderAsync(preOrderRequest);
         var preorderAssertion = await preOrderCommand.Should().NotThrowAsync();
         preorderAssertion.Subject.Should().NotBeNull();
         preorderAssertion.Subject.UpdatedBalance.Should().NotBeNull();
         preorderAssertion.Subject.PreOrder.Should().NotBeNull();
         var completeRequest = preOrderRequest.ToCompleteOrderRequest();
         var completeCommand = async () => await _balanceManagementService.CompleteOrderAsync(completeRequest);
         var completeAssertion = await completeCommand.Should().NotThrowAsync();
         completeAssertion.Subject.Should().NotBeNull();
         completeAssertion.Subject.UpdatedBalance.Should().NotBeNull();
         completeAssertion.Subject.Order.Should().NotBeNull();
      }

      [Retry(5)]
      [Test]
      public async Task Cancel_ShouldReturnEmpty_IfNonExistentPreorderEntered()
      {
         var cancelRequest = new CancelOrderRequest()
         {
            OrderId = int.MaxValue.ToString(),
         };
         var cancelCommand = async () => await _balanceManagementService.CancelOrderAsync(cancelRequest);
         var cancelAssertion = await cancelCommand.Should().ThrowAsync<NotFoundException>();
      }

      [Retry(5)]
      [Test]
      public async Task Complete_ShouldReturnEmpty_IfNonExistentPreorderEntered()
      {
         var completeRequest = new CompleteOrderRequest()
         {
            OrderId = int.MaxValue.ToString(),
         };
         var completeCommand = async () => await _balanceManagementService.CompleteOrderAsync(completeRequest);
         var completeAssertion = await completeCommand.Should().ThrowAsync<NotFoundException>();
      }
   }
}
