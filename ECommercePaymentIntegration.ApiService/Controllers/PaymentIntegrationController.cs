﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;
using ECommercePaymentIntegration.Application.DTO.Responses;
using ECommercePaymentIntegration.Application.Interfaces.PaymentIntegration;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePaymentIntegration.ApiService.Controllers
{
    [Route("api/")]
    [ApiController]
    public class PaymentIntegrationController : ControllerBase
    {
        private IPaymentIntegrationService _paymentIntegrationService;

        public PaymentIntegrationController(IPaymentIntegrationService paymentIntegrationService)
        {
            _paymentIntegrationService = paymentIntegrationService;
        }

        /// <summary>
        /// Gets the list of available products.
        /// </summary>
        /// <returns>A list of available products.</returns>
        /// <response code="500">Server Error.</response>
        [HttpGet("products")]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IEnumerable<ProductDto>> GetProducts()
        {
            return await _paymentIntegrationService.GetProductsAsync();
        }

        /// <summary>
        /// Completes an existing preorder.
        /// </summary>
        /// <param name="req">Create Order Request.</param>
        /// <returns>Updated Order and Balance.</returns>
        /// <response code="500">Server Error.</response>
        /// <response code="404">Products not found.</response>
        /// <response code="400">Bad request.</response>
        [HttpPost("orders/create")]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<ActionResult<PreOrderResultDto>> CreateOrder([FromBody] CreateOrderRequest req)
        {
            return await _paymentIntegrationService.CreateOrderAsync(req);
        }

        /// <summary>
        /// Completes an existing preorder.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <returns>Updated Order and Balance.</returns>
        /// <response code="500">Server Error.</response>
        /// <response code="404">Preorder not found.</response>
        /// <response code="400">Bad request.</response>
        [HttpPost("orders/{id}/complete")]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<ActionResult<OrderResultDtoBase>> CompleteOrder(string id)
        {
            return await _paymentIntegrationService.CompleteOrder(new CompleteOrderRequest { OrderId = id });
        }
    }
}
