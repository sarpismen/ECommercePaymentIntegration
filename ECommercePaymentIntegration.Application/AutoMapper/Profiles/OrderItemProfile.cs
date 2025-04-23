using AutoMapper;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration;
using ECommercePaymentIntegration.Domain.Entities.Order;

namespace ECommercePaymentIntegration.Application.AutoMapper.Profiles
{
   public class OrderItemProfile : Profile
   {
      public OrderItemProfile()
      {
         CreateMap<OrderItemDto, OrderItem>()
            .ForMember(m => m.ProductId, o => o.MapFrom(p => p.ProductId))
            .ForMember(m => m.Quantity, o => o.MapFrom(p => p.Quantity))
            .ForMember(m => m.OrderId, o => o.Ignore())
            .ForMember(m => m.SubTotal, o => o.Ignore())
            .ForMember(m => m.Order, o => o.Ignore());
      }
   }
}
