using Aspire.Hosting;
namespace ECommercePaymentIntegration.AppHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = DistributedApplication.CreateBuilder(args);

            var cache = builder.AddRedis("cache");

            var apiService = builder.AddProject<Projects.ECommercePaymentIntegration_ApiService>("apiservice");

            builder.Build().Run();
        }
    }
}