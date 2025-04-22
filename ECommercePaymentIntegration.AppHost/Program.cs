using Aspire.Hosting;

namespace ECommercePaymentIntegration.AppHost
{
   public class Program
   {
      public static void Main(string[] args)
      {
         var builder = DistributedApplication.CreateBuilder(args);

         var sqlServer = builder.AddSqlServer("PAYINTSQL01", port: 51886).WithLifetime(Aspire.Hosting.ApplicationModel.ContainerLifetime.Persistent);

         var sqlDb = sqlServer.AddDatabase("ECommercePaymentIntegration");
         var apiService = builder.AddProject<Projects.ECommercePaymentIntegration_ApiService>("apiservice").WithReference(sqlDb).WaitFor(sqlDb);
         builder.Build().Run();
      }
   }
}