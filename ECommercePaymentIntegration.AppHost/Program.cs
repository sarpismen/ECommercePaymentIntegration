using Aspire.Hosting;
using Microsoft.Extensions.Configuration;

namespace ECommercePaymentIntegration.AppHost
{
   public class Program
   {
      public static void Main(string[] args)
      {
         var builder = DistributedApplication.CreateBuilder(args);

         var sqlServer = builder.AddSqlServer("PAYINTSQL01", port: 51886).WithLifetime(Aspire.Hosting.ApplicationModel.ContainerLifetime.Persistent);

         var integrationTestDatabaseName = builder.Configuration.GetValue<string>("IntegrationTestSqlDatabaseName");

         var balanceManagementServiceUrl = builder.Configuration.GetValue<string>("BalanceManagementServiceUrl");
         var applicationDatabaseName = builder.Configuration.GetValue<string>("ApplicationDatabaseName");
         var sqlDb = sqlServer.AddDatabase(builder.Configuration.GetValue<string>("SqlDatabaseName"));
         var integrationSqlDb = sqlServer.AddDatabase(integrationTestDatabaseName);
         var apiService = builder.AddProject<Projects.ECommercePaymentIntegration_ApiService>("apiservice")
            .WithEnvironment("SqlDatabaseName", applicationDatabaseName)
            .WithEnvironment("BalanceManagementServiceUrl", balanceManagementServiceUrl)
            .WithReference(sqlDb)
            .WaitFor(sqlDb);
         if (applicationDatabaseName == builder.Configuration.GetValue<string>("IntegrationTestSqlDatabaseName"))
         {
            apiService = apiService.WithReference(integrationSqlDb)
            .WaitFor(integrationSqlDb);
         }

         builder.Build().Run();
      }
   }
}