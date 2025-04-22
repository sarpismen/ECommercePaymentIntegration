using System;
using System.Data;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ECommercePaymentIntegration.ApiService.Middlewares
{
   public class GlobalExceptionHandlerMiddleware
   {
      private readonly RequestDelegate _next;
      private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

      public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
      {
         _next = next;
         _logger = logger;
      }

      public async Task InvokeAsync(HttpContext context)
      {
         try
         {
            await _next(context);
         }
         catch (Exception ex)
         {
            await HandleExceptionAsync(context, ex);
         }
      }

      private async Task HandleExceptionAsync(HttpContext context, Exception exception)
      {
         HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
         string message = "An unexpected error occurred.";
         string error = "Error";
         if (exception is ServiceExceptionBase service)
         {
            statusCode = service.HttpStatusCode;
            message = service.Message;
            error = service.Error;
         }
         else
         {
            message = exception.Message;
            error = exception.ToString();
         }

         _logger.LogError(exception, message);

         context.Response.ContentType = "application/json";
         context.Response.StatusCode = (int)statusCode;

         var errorResponse = new
         {
            Error = error,
            Message = message,
         };

         var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
         {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
         });
         await context.Response.WriteAsync(jsonResponse);
      }
   }
}
