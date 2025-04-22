using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;
using ECommercePaymentIntegration.Application.Interfaces.PaymentIntegration;
using ECommercePaymentIntegration.Domain.Entities.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
      [HttpGet("products")]
      public async Task<IEnumerable<Product>> GetProducts()
      {
         return await _paymentIntegrationService.GetProducts();
      }

      [HttpPost("orders/create")]
      public async Task<ActionResult<PreOrderResultDto>> CreateOrder([FromBody] CreateOrderRequest req)
      {
         return await _paymentIntegrationService.CreateOrder(req);
      }
      [HttpPost("orders/{id}/complete")]
      public async Task<ActionResult<OrderResultDtoBase>> CompleteOrder(string id)
      {
         return await _paymentIntegrationService.CompleteOrder(new CompleteOrderRequest { OrderId = id });
      }
   }
}
