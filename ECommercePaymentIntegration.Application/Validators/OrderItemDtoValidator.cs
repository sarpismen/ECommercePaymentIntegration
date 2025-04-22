using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;
using FluentValidation;

namespace ECommercePaymentIntegration.Application.Validators
{

   internal class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
   {
      public OrderItemDtoValidator()
      {
         RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID cannot be empty");
         RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity cannot be zero or negative");
      }
   }
}
