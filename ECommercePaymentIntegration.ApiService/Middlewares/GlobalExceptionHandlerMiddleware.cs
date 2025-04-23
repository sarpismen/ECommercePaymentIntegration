using System;
using System.Net;
using System.Threading.Tasks;
using ECommercePaymentIntegration.Application.DTO.Responses;
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
         var errorResponse = new ErrorResponse
         {
            Message = "An unexpected error occurred.",
            Error = "Error",
         };
         if (exception is ServiceExceptionBase service)
         {
            statusCode = service.HttpStatusCode;
            errorResponse.Message = service.Message;
            errorResponse.Error = service.Error;
            errorResponse.OrderId = service.OrderId;
         }
         else
         {
            errorResponse.Message = exception.Message;
            errorResponse.Error = exception.ToString();
         }

         _logger.LogError(exception, errorResponse.Message);

         context.Response.ContentType = "application/json";
         context.Response.StatusCode = (int)statusCode;

         await context.Response.WriteAsJsonAsync(errorResponse);
      }
   }
}
