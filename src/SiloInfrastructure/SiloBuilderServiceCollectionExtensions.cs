using System.Net;
using ContosoLoans;
using Orleans.Configuration;

namespace Microsoft.AspNetCore.Hosting {
    public static class SiloBuilderServiceCollectionExtensions {
        public static WebApplicationBuilder BuildSiloFromArguments(this WebApplicationBuilder builder, string[] args) {
            return builder.BuildSiloFromArguments(args, null);
        }

        public static WebApplicationBuilder BuildSiloFromArguments(this WebApplicationBuilder builder,
            string[] args,
            Action<ISiloBuilder>? action = null) {
            
            var siloPort = string.IsNullOrEmpty(builder.Configuration["OrleansSiloPort"]) 
                ? (args.Length > 0) 
                    ? int.Parse(args[0]) 
                    : 11111
                : int.Parse(builder.Configuration["OrleansSiloPort"]);


            var gatewayPort = string.IsNullOrEmpty(builder.Configuration["OrleansGatewayPort"])
                ? (args.Length > 1)
                    ? int.Parse(args[1])
                    : 30000
                : int.Parse(builder.Configuration["OrleansGatewayPort"]);


            var httpPort = string.IsNullOrEmpty(builder.Configuration["HttpPort"])
                ? (args.Length > 2)
                    ? int.Parse(args[2])
                    : 5001
                : int.Parse(builder.Configuration["HttpPort"]);


            var siloName = string.IsNullOrEmpty(builder.Configuration["SiloName"])
                ? (args.Length > 3)
                    ? args[3]
                    : "Silo"
                : builder.Configuration["SiloName"];


            var connectionString = string.IsNullOrEmpty(builder.Configuration["AZURE_TABLE_SERVICE_CONNECTION_STRING"])
                ? (args.Length > 4)
                    ? args[4]
                    : "UseDevelopmentStorage=true;"
                : builder.Configuration["AZURE_TABLE_SERVICE_CONNECTION_STRING"];

            builder.Host.UseOrleans(siloBuilder => {
                siloBuilder
                    .ConfigureEndpoints(IPAddress.Loopback, siloPort, gatewayPort)
                    .Configure<ClusterOptions>(options => {
                        options.ClusterId = "dev";
                        options.ServiceId = "ContosoLoans";
                    })
                    .Configure<SiloOptions>(options => {
                        options.SiloName = siloName;
                    })
                    .AddAzureTableGrainStorageAsDefault(options =>
                        options.ConfigureTableServiceClient(connectionString))
                    .UseAzureStorageClustering(options =>
                        options.ConfigureTableServiceClient(connectionString))
                    .AddActivityPropagation();

                if (action != null)
                    action(siloBuilder);
            });

            return builder;
        }
    }
}