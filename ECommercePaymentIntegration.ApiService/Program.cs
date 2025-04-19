using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.SqlServer;
using ECommercePaymentIntegration.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;

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
         });
         builder.Services.AddDbContext<ECommercePaymentIntegrationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ECommercePaymentIntegration")));

         var app = builder.Build();
         if (app.Environment.IsDevelopment())
         {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
               c.SwaggerEndpoint(SwaggerUrl, $"{ApiTitle} {ApiVersion}");
               c.RoutePrefix = string.Empty; // Serve Swagger UI at the root
            });
         }

         app.UseExceptionHandler();

         app.MapDefaultEndpoints();

         app.Run();
      }
   }
}