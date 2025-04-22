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
   public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
   {
      public CreateOrderRequestValidator()
      {
         RuleFor(req => req.Items).NotNull().NotEmpty().WithMessage("Items cannot be empty");
         RuleForEach(req => req.Items).SetValidator(new OrderItemDtoValidator());
      }
   }
}
