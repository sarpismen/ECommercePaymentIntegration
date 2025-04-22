using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;
using FluentValidation;

namespace ECommercePaymentIntegration.Application.Validators
{
   public class CompleteOrderRequestValidator : AbstractValidator<CompleteOrderRequest>
   {
      public CompleteOrderRequestValidator()
      {
         RuleFor(req => req.OrderId).NotEmpty().NotNull().WithMessage("Order ID must be entered");
      }
   }
}
