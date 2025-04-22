using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Azure;
using ECommerceApp.Infrastructure.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Enums;
using ECommercePaymentIntegration.Application.DTO.BalanceManagement.Requests;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration;
using ECommercePaymentIntegration.Application.DTO.PaymentIntegration.Requests;
using ECommercePaymentIntegration.Application.DTO.Responses;
using ECommercePaymentIntegration.Application.Exceptions;
using ECommercePaymentIntegration.Application.Interfaces.BalanceManagement;
using ECommercePaymentIntegration.Application.Json;
using ECommercePaymentIntegration.Domain.Entities.Order;
using ECommercePaymentIntegration.Domain.Entities.Product;
using ECommercePaymentIntegration.Domain.ValueObjects.Order;
using ECommercePaymentIntegration.Infrastructure;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WireMock.Client;
using WireMock;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ECommercePaymentIntegration.Tests.Integration.ApplicationFactories
{

   public class TestingAspireAppHostFactory(string balanceManagementServiceUrl) : DistributedApplicationFactory(typeof(Projects.ECommercePaymentIntegration_AppHost))
   {
      protected override void OnBuilderCreated(DistributedApplicationBuilder applicationBuilder)
      {
         applicationBuilder.Services.ConfigureHttpClientDefaults(clientBuilder =>
         {
            clientBuilder.AddStandardResilienceHandler();
         });
         applicationBuilder.Configuration["ApplicationDatabaseName"] = "ECommercePaymentIntegrationTest";
         applicationBuilder.Configuration["BalanceManagementServiceUrl"] = balanceManagementServiceUrl;
         base.OnBuilderCreated(applicationBuilder);
      }

      protected override void OnBuilderCreating(DistributedApplicationOptions applicationOptions, HostApplicationBuilderSettings hostOptions)
      {
         base.OnBuilderCreating(applicationOptions, hostOptions);
      }

      protected override void OnBuilding(DistributedApplicationBuilder applicationBuilder)
      {
         base.OnBuilding(applicationBuilder);
      }

      protected override void OnBuilt(DistributedApplication application)
      {
         var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
         application.ResourceNotifications.WaitForResourceHealthyAsync(
                "apiservice",
                 cts.Token);
         base.OnBuilt(application);
      }
   }
}
