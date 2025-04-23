using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;
using ECommercePaymentIntegration.Application.Exceptions;
using ECommercePaymentIntegration.Application.Interfaces.BalanceManagement;
using ECommercePaymentIntegration.Application.Interfaces.PaymentIntegration;
using ECommercePaymentIntegration.Domain.Entities.Order;
using ECommercePaymentIntegration.Domain.ValueObjects.Order;
using ECommercePaymentIntegration.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Polly;

namespace ECommercePaymentIntegration.Application.Services.PaymentIntegration
{
   public class PaymentIntegrationService : IPaymentIntegrationService
   {
      private readonly IBalanceManagementService _balanceManagementService;
      private readonly IMapper _mapper;
      private readonly ILogger<PaymentIntegrationService> _logger;
      private readonly IOrderRepository _orderRepository;
      private readonly IValidator<CompleteOrderRequest> _completeOrderRequestValidator;
      private readonly IValidator<CreateOrderRequest> _createOrderRequestValidator;

      public PaymentIntegrationService(IBalanceManagementService balanceManagementService, IOrderRepository orderRepository, IMapper mapper, ILogger<PaymentIntegrationService> logger, IValidator<CompleteOrderRequest> completeOrderRequestValidator, IValidator<CreateOrderRequest> createOrderRequestValidator)
      {
         _balanceManagementService = balanceManagementService;
         _mapper = mapper;
         _logger = logger;
         _orderRepository = orderRepository;
         _completeOrderRequestValidator = completeOrderRequestValidator;
         _createOrderRequestValidator = createOrderRequestValidator;
      }

      public async Task<IEnumerable<ProductDto>> GetProductsAsync()
      {
         var allProductDtos = await _balanceManagementService.GetProductsAsync();
         var availableProductDtos = allProductDtos.Where(x => x.Stock > 0);
         return availableProductDtos;
      }

      public async Task<PreOrderResultDto> CreateOrderAsync(CreateOrderRequest req)
      {
         var validation = _createOrderRequestValidator.Validate(req);
         if (!validation.IsValid)
         {
            throw new BadRequestException("Invalid request", validation.ToString(";"));
         }

         Order order = await CreateOrderObjectAsync(req);

         await _orderRepository.AddAsync(order);

         try
         {
            return await PreorderAsync(order);
         }
         catch (Exception ex)
         {
            if (ex is ServiceExceptionBase serviceException)
            {
               serviceException.OrderId = order.OrderId;
            }

            order.Status = OrderStatus.Failed;
            order.OrderErrors.Add(new OrderError { Error = ex.Message });
            await _orderRepository.UpdateAsync(order);
            throw;
         }
      }

      public async Task<OrderResultDtoBase> CompleteOrder(CompleteOrderRequest completeOrderRequest)
      {
         var validation = _completeOrderRequestValidator.Validate(completeOrderRequest);
         if (!validation.IsValid)
         {
            throw new BadRequestException("Invalid request", validation.ToString(";"));
         }

         var fallbackPolicy = Policy<OrderResultDtoBase>.Handle<BalanceManagementServiceException>().FallbackAsync(
            async (_) => await CancelOrderAsync(completeOrderRequest),
            async (exception) =>
            {
               await AddErrorToExistingOrder(completeOrderRequest.OrderId, exception);
               _logger.LogError($"Error while completing order {completeOrderRequest.OrderId}, cancelling the order", exception);
            });

         var completeResult = await fallbackPolicy.ExecuteAndCaptureAsync(async () => await CompleteOrderAsync(completeOrderRequest));

         if (completeResult.Outcome == OutcomeType.Failure)
         {
            Exception finalException = completeResult.FinalException;
            if (finalException is ServiceExceptionBase serviceException)
            {
               serviceException.OrderId = completeOrderRequest.OrderId;
            }

            throw finalException;
         }

         return completeResult.Result;
      }

      private async Task<OrderResultDtoBase> CompleteOrderAsync(CompleteOrderRequest completeOrderRequest)
      {
         OrderResultDto completeOrderResult = await _balanceManagementService.CompleteOrderAsync(completeOrderRequest);
         await UpdateOrderStatus(completeOrderRequest.OrderId, OrderStatus.Completed);
         return completeOrderResult;
      }

      private async Task<OrderResultDtoBase> CancelOrderAsync(CompleteOrderRequest completeOrderRequest)
      {
         var cancelOrderResult = await _balanceManagementService.CancelOrderAsync(new CancelOrderRequest { OrderId = completeOrderRequest.OrderId });
         await UpdateOrderStatus(completeOrderRequest.OrderId, OrderStatus.Cancelled);
         return cancelOrderResult;
      }

      private async Task<PreOrderResultDto> PreorderAsync(Order order)
      {
         var preorderResult = await _balanceManagementService.PreorderAsync(new PreorderRequest { Amount = order.Total, OrderId = order.OrderId });

         order.Status = OrderStatus.Preordered;
         order.LastUpdatedAt = DateTime.UtcNow;

         await _orderRepository.UpdateAsync(order);

         return preorderResult;
      }

      private async Task<Order> CreateOrderObjectAsync(CreateOrderRequest req)
      {
         var allProductDtos = await _balanceManagementService.GetProductsAsync();
         var allProductsById = allProductDtos.ToDictionary(x => x.Id, x => x);
         var order = new Order
         {
            Status = OrderStatus.PendingPreorder,
         };
         var notFoundProducts = new List<OrderItemDto>();
         var notAvailableProducts = new List<OrderItemDto>();
         foreach (var orderItemDto in req.Items)
         {
            if (!allProductsById.TryGetValue(orderItemDto.ProductId, out var product))
            {
               notFoundProducts.Add(orderItemDto);
               continue;
            }

            if (orderItemDto.Quantity > product.Stock)
            {
               notAvailableProducts.Add(orderItemDto);
               continue;
            }

            var orderItem = _mapper.Map<OrderItemDto, OrderItem>(orderItemDto);
            orderItem.Order = order;
            orderItem.UnitPrice = product.Price;
            orderItem.OrderId = order.OrderId;
            order.OrderItems.Add(orderItem);
         }

         if (notFoundProducts.Any())
         {
            throw new NotFoundException("Some entered products doesn't exist", string.Join(",", notFoundProducts.Select(x => x.ProductId)));
         }

         if (notAvailableProducts.Any())
         {
            throw new OutOfStockException("These products are out of stock", string.Join(",", notAvailableProducts.Select(x => x.ProductId)));
         }

         return order;
      }

      private async Task AddErrorToExistingOrder(string orderId, DelegateResult<OrderResultDtoBase> exception)
      {
         var record = await _orderRepository.GetByIdAsync(orderId);
         if (record == null)
         {
            _logger.LogCritical($"Record {orderId} is not found in the DB. There could be inconsistencies");
            return;
         }

         record.OrderErrors.Add(new OrderError { Error = exception.Exception.Message });
      }

      private async Task UpdateOrderStatus(string orderId, OrderStatus status)
      {
         var record = await _orderRepository.GetByIdAsync(orderId);
         if (record == null)
         {
            _logger.LogCritical($"Record {orderId} is not found in the DB. There could be inconsistencies");
            return;
         }

         record.Status = status;
         if (record.Status == OrderStatus.Completed)
         {
            record.CompletedAt = DateTimeOffset.UtcNow;
         }

         await _orderRepository.UpdateAsync(record);
      }
   }
}
