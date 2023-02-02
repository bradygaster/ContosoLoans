using System.Net;

namespace Microsoft.AspNetCore.Hosting
{
    public static class SiloBuilderServiceCollectionExtensions
    {
        public static WebApplication BuildAppFromArguments(this WebApplicationBuilder builder, string[] args)
        {
            return builder.BuildAppFromArguments(args, null);
        }

        public static WebApplication BuildAppFromArguments(this WebApplicationBuilder builder, 
            string[] args, 
            Action<ISiloBuilder>? action = null)
        {
            var siloPort = (args.Length > 0) ? int.Parse(args[0]) : 11111;
            var gatewayPort = (args.Length > 1) ? int.Parse(args[1]) : 30000;
            var httpPort = (args.Length > 2) ? int.Parse(args[2]) : 5001;
            var useDashboard = (args.Length > 4) ? bool.Parse(args[4]) : true;

            const string tblServiceConfig = "AZURE_TABLE_SERVICE_CONNECTION_STRING";
            var tblServiceCnStr = Environment.GetEnvironmentVariable(tblServiceConfig) != null
                ? Environment.GetEnvironmentVariable(tblServiceConfig)
                : "UseDevelopmentStorage=true;";

            builder.Host.UseOrleans(siloBuilder =>
            {
                siloBuilder
                    .ConfigureEndpoints(IPAddress.Loopback, siloPort, gatewayPort)
                    .AddAzureTableGrainStorageAsDefault(options =>
                        options.ConfigureTableServiceClient(tblServiceCnStr))
                    .UseAzureStorageClustering(options =>
                        options.ConfigureTableServiceClient(tblServiceCnStr));

                if (useDashboard)
                    siloBuilder
                        .UseDashboard(options => options.HostSelf = true)
                        .ConfigureServices(services => services.AddServicesForSelfHostedDashboard());

                if (action != null)
                    action(siloBuilder);
            });

            var app = builder.Build();
            if (useDashboard)
                app.UseOrleansDashboard();

            return app;
        }
    }
}