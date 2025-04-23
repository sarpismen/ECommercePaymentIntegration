using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
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
