using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using ECommercePaymentIntegration.Domain.Entities.Product;
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

      public async Task<IEnumerable<Product>> GetProducts()
      {
         var allProductDtos = await _balanceManagementService.GetProductsAsync();
         var availableProductDtos = allProductDtos.Where(x => x.Stock > 0);
         var availableProducts = _mapper.Map<IEnumerable<ProductDto>, IEnumerable<Product>>(availableProductDtos);
         return availableProducts;
      }

      public async Task<PreOrderResultDto> CreateOrder(CreateOrderRequest req)
      {
         var validation = _createOrderRequestValidator.Validate(req);
         if (!validation.IsValid)
         {
            throw new BadRequestException("Invalid request", validation.ToString(";"));
         }
         var allProductDtos = await _balanceManagementService.GetProductsAsync();
         var allProductsById = _mapper.Map<IEnumerable<ProductDto>, IEnumerable<Product>>(allProductDtos).ToDictionary(x => x.ProductId, x => x);
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
            orderItem.ItemPrice = product.ItemPrice;
            orderItem.OrderId = order.OrderId;
            order.OrderItems.Add(orderItem);
         }

         if (notFoundProducts.Any())
         {
            throw new NotFoundException("Some entered products doesn't exist", string.Join(",", notFoundProducts.Select(x => x.ProductId)));
         }

         if (notAvailableProducts.Any())
         {
            throw new NotFoundException("These products are out of stock", string.Join(",", notAvailableProducts.Select(x => x.ProductId)));
         }

         await _orderRepository.AddAsync(order);

         try
         {
            var preorderResult = await _balanceManagementService.PreorderAsync(new PreorderRequest { Amount = order.Total, OrderId = order.OrderId });

            order.Status = OrderStatus.Preordered;
            order.LastUpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            return preorderResult;
         }
         catch (Exception ex)
         {
            order.Status = OrderStatus.Failed;
            order.LastUpdatedAt = DateTime.UtcNow;
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
            (Func<CancellationToken, Task<OrderResultDtoBase>>)(async (_) =>
            {
               var cancelOrderResult = await _balanceManagementService.CancelOrderAsync(new CancelOrderRequest { OrderId = completeOrderRequest.OrderId });
               await UpdateOrderStatus(completeOrderRequest.OrderId, OrderStatus.Cancelled);
               return cancelOrderResult;
            }),
            async (exception) =>
            {
               await AddErrorToExistingOrder(completeOrderRequest.OrderId, exception);
               _logger.LogError($"Error while completing order {completeOrderRequest.OrderId}, cancelling the order", exception);
            });
         var completeResult = await fallbackPolicy.ExecuteAndCaptureAsync(async () =>
         {
            OrderResultDto completeOrderResult = await _balanceManagementService.CompleteOrderAsync(completeOrderRequest);
            await UpdateOrderStatus(completeOrderRequest.OrderId, OrderStatus.Completed);
            return completeOrderResult;
         });
         if (completeResult.Outcome == OutcomeType.Failure)
         {
            throw completeResult.FinalException;
         }
         return completeResult.Result;
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
