using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ECommercePaymentIntegration.Application.AutoMapper.Profiles;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;
using ECommercePaymentIntegration.Application.Exceptions;
using ECommercePaymentIntegration.Application.Interfaces.BalanceManagement;
using ECommercePaymentIntegration.Application.Services.PaymentIntegration;
using ECommercePaymentIntegration.Application.Validators;
using ECommercePaymentIntegration.Domain.Entities.Order;
using ECommercePaymentIntegration.Domain.ValueObjects.Order;
using ECommercePaymentIntegration.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
namespace ECommercePaymentIntegration.Tests.UnitTests
{
   public class PaymentIntegrationServiceTests
   {
      private PaymentIntegrationService _paymentIntegrationService;
      private Mock<IBalanceManagementService> _balanceManagementServiceMock;
      private Mock<IOrderRepository> _orderRepositoryMock;

      [SetUp]
      public void Setup()
      {
         var configuration = new MapperConfiguration(cfg =>
         {
            cfg.AddProfile(new OrderItemProfile());
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
         await _paymentIntegrationService.CompleteOrder(new CompleteOrderRequest { OrderId = "bunuyaparkendepremoldu" });
         _balanceManagementServiceMock.Verify(x => x.CancelOrderAsync(It.IsAny<CancelOrderRequest>()), Times.Once());
         _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
         savedOrder.Should().NotBeNull();
         savedOrder.Status.Should().Be(OrderStatus.Cancelled);
      }

      [Test]
      public async Task CompleteOrder_WhenThrowsNotFound_ShouldRethrow()
      {
         _balanceManagementServiceMock.Setup(x => x.CompleteOrderAsync(It.IsAny<CompleteOrderRequest>())).ThrowsAsync(new NotFoundException());
         var completeOrderAct = async () => await _paymentIntegrationService.CompleteOrder(new CompleteOrderRequest { OrderId = "1" });
         await completeOrderAct.Should().ThrowAsync<NotFoundException>();
         _balanceManagementServiceMock.Verify(x => x.CancelOrderAsync(It.IsAny<CancelOrderRequest>()), Times.Never());
      }

      [Test]
      public async Task CompleteOrder_WhenThrowsBadRequest_ShouldRethrow()
      {
         _balanceManagementServiceMock.Setup(x => x.CompleteOrderAsync(It.IsAny<CompleteOrderRequest>())).ThrowsAsync(new BadRequestException());
         var completeOrderAct = async () => await _paymentIntegrationService.CompleteOrder(new CompleteOrderRequest { OrderId = "1" });
         await completeOrderAct.Should().ThrowAsync<BadRequestException>();
         _balanceManagementServiceMock.Verify(x => x.CancelOrderAsync(It.IsAny<CancelOrderRequest>()), Times.Never());
      }

      [Test]
      public async Task CreateOrder_WhenThrowsBadRequest_ShouldRethrowAndUpdateDbToFailed()
      {
         Order updateOrder = null;
         _orderRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(() => new Order { Status = OrderStatus.PendingPreorder });
         _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>())).Callback<Order>(cb => updateOrder = cb);
         _balanceManagementServiceMock.Setup(x => x.PreorderAsync(It.IsAny<PreorderRequest>())).ThrowsAsync(new BadRequestException());
         _balanceManagementServiceMock.Setup(x => x.GetProductsAsync()).ReturnsAsync(new List<ProductDto>() { new ProductDto { Id = "a", Stock = 1, Price = 1 } });
         var createOrderAct = async () => await _paymentIntegrationService.CreateOrderAsync(new CreateOrderRequest { Items = new List<OrderItemDto> { new OrderItemDto { ProductId = "a", Quantity = 1 } } });
         await createOrderAct.Should().ThrowAsync<BadRequestException>();
         _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
         updateOrder.Should().NotBeNull();
         updateOrder.Status.Should().Be(OrderStatus.Failed);
      }

      [Test]
      public async Task CreateOrder_WhenStockIsInsufficient_ThrowsOutOfStockException()
      {
         _balanceManagementServiceMock.Setup(x => x.GetProductsAsync()).ReturnsAsync(new List<ProductDto>() { new ProductDto { Id = "a", Stock = 0, Price = 1 } });
         var createOrderAct = async () => await _paymentIntegrationService.CreateOrderAsync(new CreateOrderRequest { Items = new List<OrderItemDto> { new OrderItemDto { ProductId = "a", Quantity = 1 } } });
         await createOrderAct.Should().ThrowAsync<OutOfStockException>();
      }

      [Test]
      public async Task CreateOrder_ProductDoesntExist_ThrowsNotFoundException()
      {
         _balanceManagementServiceMock.Setup(x => x.GetProductsAsync()).ReturnsAsync(new List<ProductDto>() { new ProductDto { Id = "a", Stock = 1, Price = 1 } });
         var createOrderAct = async () => await _paymentIntegrationService.CreateOrderAsync(new CreateOrderRequest { Items = new List<OrderItemDto> { new OrderItemDto { ProductId = "b", Quantity = 1 } } });
         await createOrderAct.Should().ThrowAsync<NotFoundException>();
      }

      [Test]
      public async Task CreateOrder_WhenInvoked_ShouldReturnResponse()
      {
         Order updateOrder = null;
         Order addOrder = null;
         _balanceManagementServiceMock.Setup(x => x.PreorderAsync(It.IsAny<PreorderRequest>())).ReturnsAsync(() => new PreOrderResultDto { PreOrder = new OrderStatusDto(), UpdatedBalance = new UserBalanceDto()});
         _orderRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(() => new Order { Status = OrderStatus.Preordered });
         _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>())).Callback<Order>(cb => updateOrder = cb );
         _orderRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Order>())).Callback<Order>(cb => addOrder = cb); ;
         _balanceManagementServiceMock.Setup(x => x.GetProductsAsync()).ReturnsAsync(new List<ProductDto>() { new ProductDto { Id = "a", Stock = 1, Price = 1 } });
         var response = await _paymentIntegrationService.CreateOrderAsync(new CreateOrderRequest { Items = new List<OrderItemDto> { new OrderItemDto { ProductId = "a", Quantity = 1 } } });

         _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
         _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
         _balanceManagementServiceMock.Verify(x => x.PreorderAsync(It.IsAny<PreorderRequest>()), Times.Once);
         updateOrder.Should().NotBeNull();
         updateOrder.Status.Should().Be(OrderStatus.Preordered);
         addOrder.Should().NotBeNull();
         addOrder.Status.Should().Be(OrderStatus.PendingPreorder);
         response.Should().NotBeNull();
      }
      [Test]
      public async Task CompleteOrder_WhenInvoked_ShouldUpdateOrder()
      {
         Order savedOrder = null;
         _balanceManagementServiceMock.Setup(x => x.CompleteOrderAsync(It.IsAny<CompleteOrderRequest>())).ReturnsAsync(() => new OrderResultDto() { Order = new OrderStatusDto(), UpdatedBalance = new UserBalanceDto()});
         _orderRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(() => new Order { Status = OrderStatus.Preordered });
         _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>())).Callback<Order>((order) => savedOrder = order);
         await _paymentIntegrationService.CompleteOrder(new CompleteOrderRequest { OrderId = "bunuyaparkendepremoldu" });
         _balanceManagementServiceMock.Verify(x => x.CompleteOrderAsync(It.IsAny<CompleteOrderRequest>()), Times.Once());
         _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
         savedOrder.Should().NotBeNull();
         savedOrder.Status.Should().Be(OrderStatus.Completed);
      }

   }
}
