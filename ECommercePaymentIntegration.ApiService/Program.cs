using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using ECommerceApp.Infrastructure.BalanceManagement;
using ECommercePaymentIntegration.ApiService.Middlewares;
using ECommercePaymentIntegration.Application.AutoMapper;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;
using ECommercePaymentIntegration.Application.Interfaces.BalanceManagement;
using ECommercePaymentIntegration.Application.Interfaces.PaymentIntegration;
using ECommercePaymentIntegration.Application.Services.PaymentIntegration;
using ECommercePaymentIntegration.Application.Validators;
using ECommercePaymentIntegration.Infrastructure;
using ECommercePaymentIntegration.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
namespace ECommercePaymentIntegration.ApiService
{
   public class Program
   {
      private const string ApiTitle = "E-Commerce Payment Integration Api";
      private const string ApiVersion = "v1";
      private const string SwaggerUrl = "/swagger/v1/swagger.json";

      public static void Main(string[] args)
      {
         var builder = WebApplication.CreateBuilder(args);

         builder.AddServiceDefaults();

         builder.Services.AddProblemDetails();

         builder.Services.AddControllers();
         builder.Services.AddEndpointsApiExplorer();
         builder.Services.AddSwaggerGen(c =>
         {
            c.SwaggerDoc(ApiVersion, new OpenApiInfo { Title = ApiTitle, Version = ApiVersion });
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
         });
         var sqlDatabaseName = builder.Configuration.GetValue<string>("SqlDatabaseName");
         builder.Services.AddDbContext<ECommercePaymentIntegrationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString(sqlDatabaseName)));
         var balanceManagementServiceurl = builder.Configuration.GetValue<string>("BalanceManagementServiceUrl");
         builder.Services.AddHttpClient(HttpClients.BalanceManagementApi, client =>
         {
            client.BaseAddress = new Uri(balanceManagementServiceurl);
            client.Timeout = TimeSpan.FromSeconds(100);
         });
         builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
         builder.Services.AddAutoMapperProfiles();
         builder.Services.AddSingleton(typeof(IOrderRepository), typeof(OrderRepository));

         builder.Services.AddSingleton<IValidator<CompleteOrderRequest>, CompleteOrderRequestValidator>();
         builder.Services.AddSingleton<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();
         builder.Services.AddSingleton(typeof(IBalanceManagementService), typeof(BalanceManagementService));
         builder.Services.AddSingleton(typeof(IPaymentIntegrationService), typeof(PaymentIntegrationService));
         var app = builder.Build();
         using var serviceScope = app.Services.CreateScope();
         var dbContext = serviceScope.ServiceProvider.GetRequiredService<ECommercePaymentIntegrationDbContext>();
         dbContext.Database.Migrate();
         app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
         if (app.Environment.IsDevelopment())
         {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
               c.SwaggerEndpoint(SwaggerUrl, $"{ApiTitle} {ApiVersion}");
               c.RoutePrefix = "documentation";
            });
         }

         app.MapDefaultEndpoints();
         app.MapControllers();

         app.Run();
      }
   }
}