using System;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePaymentIntegration.Application.AutoMapper
{
   public static class AutoMapperConfiguration
   {
      public static void AddAutoMapperProfiles(this IServiceCollection configuration)
      {
         var profiles = typeof(AutoMapperConfiguration).Assembly.GetTypes().Where(x => typeof(Profile).IsAssignableFrom(x));
         foreach (var profile in profiles)
         {
            configuration.AddAutoMapper(profile);
         }
      }
   }
}