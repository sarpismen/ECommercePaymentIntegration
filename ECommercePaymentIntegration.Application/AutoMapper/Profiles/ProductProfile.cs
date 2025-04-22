using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Domain.Entities.Product;

namespace ECommercePaymentIntegration.Application.AutoMapper.Profiles
{
   public class ProductProfile : Profile
   {
      public ProductProfile()
      {
         CreateMap<ProductDto, Product>()
            .ForMember(m => m.ProductId, o => o.MapFrom(p => p.Id))
            .ForMember(m => m.ItemPrice, o => o.MapFrom(p => p.Price));
      }
   }
}
