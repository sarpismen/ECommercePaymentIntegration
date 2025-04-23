using AutoMapper;
using ECommercePaymentIntegration.Application.AutoMapper.Profiles;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;
using ECommercePaymentIntegration.Application.Exceptions;
using ECommercePaymentIntegration.Application.Interfaces.BalanceManagement;
using ECommercePaymentIntegration.Application.Services.PaymentIntegration;
using ECommercePaymentIntegration.Application.Validators;
using ECommercePaymentIntegration.Domain.Entities.Order;
using ECommercePaymentIntegration.Domain.ValueObjects.Order;
using ECommercePaymentIntegration.Infrastructure.Persistence;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
namespace ECommercePaymentIntegration.Tests.UnitTests
{
   public class Tests
   {
      private PaymentIntegrationService _paymentIntegrationService;
      private Mock<IBalanceManagementService> _balanceManagementServiceMock;
      private Mock<IOrderRepository> _orderRepositoryMock;

      [SetUp]
      public async Task Setup()
      {
         var configuration = new MapperConfiguration(cfg =>
         {
            cfg.AddProfile(new OrderItemProfile());
            cfg.AddProfile(new ProductProfile());
         });
         var mapper = new Mapper(configuration);

         _balanceManagementServiceMock = new Mock<IBalanceManagementService>();
         _orderRepositoryMock = new Mock<IOrderRepository>();
         var loggerMock = new Mock<ILogger<PaymentIntegrationService>>();
         var completeOrderRequestValidator = new CompleteOrderRequestValidator();
         var createOrderRequestValidator = new CreateOrderRequestValidator();
         _paymentIntegrationService = new PaymentIntegrationService(_balanceManagementServiceMock.Object, _orderRepositoryMock.Object, mapper, loggerMock.Object, completeOrderRequestValidator, createOrderRequestValidator);
      }

      [Test]
      public async Task CompleteOrder_WhenThrowsBalanceManagementServiceException_ShouldFallbackToCancelOrder()
      {
         Order savedOrder = null;
         _balanceManagementServiceMock.Setup(x => x.CompleteOrderAsync(It.IsAny<CompleteOrderRequest>())).ThrowsAsync(new BalanceManagementServiceException());
         _orderRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(() => new Order { Status = OrderStatus.Preordered });
         _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>())).Callback<Order>((order) => savedOrder = order);
         await _paymentIntegrationService.CompleteOrder(new CompleteOrderRequest { OrderId = "1" });
         _balanceManagementServiceMock.Verify(x => x.CancelOrderAsync(It.IsAny<CancelOrderRequest>()), Times.Once());
         _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>()));
         savedOrder.Should().NotBeNull();
         savedOrder.Status.Should().Be(OrderStatus.Cancelled);
      }
      [Test]
      public async Task CompleteOrder_WhenThrowsNotFound_ShouldRethrow()
      {
         _balanceManagementServiceMock.Setup(x => x.CompleteOrderAsync(It.IsAny<CompleteOrderRequest>())).ThrowsAsync(new NotFoundException());
         var completeOrderAct = () => _paymentIntegrationService.CompleteOrder(new CompleteOrderRequest { OrderId = "1" });
         await completeOrderAct.Should().ThrowAsync<NotFoundException>();
         _balanceManagementServiceMock.Verify(x => x.CancelOrderAsync(It.IsAny<CancelOrderRequest>()), Times.Never());
      }
      [Test]
      public async Task CompleteOrder_WhenThrowsBadRequest_ShouldRethrow()
      {
         _balanceManagementServiceMock.Setup(x => x.CompleteOrderAsync(It.IsAny<CompleteOrderRequest>())).ThrowsAsync(new BadRequestException());
         var completeOrderAct = () => _paymentIntegrationService.CompleteOrder(new CompleteOrderRequest { OrderId = "1" });
         await completeOrderAct.Should().ThrowAsync<BadRequestException>();
         _balanceManagementServiceMock.Verify(x => x.CancelOrderAsync(It.IsAny<CancelOrderRequest>()), Times.Never());
      }
   }
}
