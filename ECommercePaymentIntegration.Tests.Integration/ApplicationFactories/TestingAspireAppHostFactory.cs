using System;
using System.Threading;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
         application.ResourceNotifications.WaitForResourceHealthyAsync("apiservice");
         base.OnBuilt(application);
      }
   }
}
