using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ContosoLoans {
    public static class OpenTelemetryExtensions {
        
        public static WebApplicationBuilder UseOpenTelemetry(this WebApplicationBuilder builder) {
            Action<ResourceBuilder> configureResource = r
                => r.AddService(
                    serviceVersion: "1.0.0",
                    serviceName: "ContosoLoans",
                    serviceInstanceId: "ContosoLoansInstance",
                    serviceNamespace: "ContosoLoansNS");

            var azureMonitorConnectionString = builder.Configuration["AzureMonitorConnectionString"];
            
            if(!string.IsNullOrEmpty(azureMonitorConnectionString)) {
                builder.Services.ConfigureOpenTelemetryTracerProvider((serviceProvider, options) => {
                    options.AddSource("Microsoft.Orleans.Runtime");
                    options.AddSource("Microsoft.Orleans.Application");
                    options.SetSampler(new AlwaysOnSampler());
                    options.AddConsoleExporter();
                    options.AddAspNetCoreInstrumentation();
                    options.AddAzureMonitorTraceExporter(options => {
                        options.ConnectionString = azureMonitorConnectionString;
                    });
                });

                builder.Services.ConfigureOpenTelemetryMeterProvider((serviceProvider, options) => {
                    options.AddMeter("Microsoft.Orleans");
                    options.AddConsoleExporter();
                    options.AddAspNetCoreInstrumentation();
                    options.AddAzureMonitorMetricExporter(options => {
                        options.ConnectionString = azureMonitorConnectionString;
                    });
                });

                builder.Logging.ClearProviders();

                builder.Logging.AddOpenTelemetry(options => {
                    var resourceBuilder = ResourceBuilder.CreateDefault();
                    configureResource(resourceBuilder);
                    options.SetResourceBuilder(resourceBuilder);
                    options.AddConsoleExporter();
                });
            }

            return builder;
        }
    }
}