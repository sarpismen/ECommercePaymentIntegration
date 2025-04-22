using AutoMapper;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration;
using ECommercePaymentIntegration.Domain.Entities.Order;
using ECommercePaymentIntegration.Domain.Entities.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
