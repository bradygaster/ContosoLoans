using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ContosoLoans {
    public static class OpenTelemetryExtensions {

        public static WebApplicationBuilder UseOpenTelemetry(this WebApplicationBuilder builder, string serviceName) {
            builder.Services
                .AddOpenTelemetry()
                    .WithMetrics(metrics => {
                        metrics
                            .AddAspNetCoreInstrumentation()
                            .AddRuntimeInstrumentation()
                            .AddEventCountersInstrumentation(c => {
                                c.AddEventSources(
                                    "Microsoft.AspNetCore.Hosting",
                                    "Microsoft-AspNetCore-Server-Kestrel",
                                    "System.Net.Http",
                                    "System.Net.Sockets",
                                    "System.Net.NameResolution",
                                    "System.Net.Security");
                            })
                            .AddPrometheusExporter()
                            .AddMeter("Microsoft.Orleans")
                            .AddConsoleExporter();
                    })
                    .WithTracing(tracing => { 
                        tracing
                            .AddAspNetCoreInstrumentation()
                            .AddConsoleExporter()
                            .AddSource("Microsoft.Orleans.Runtime")
                            .AddSource("Microsoft.Orleans.Application")
                            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                                .AddService(serviceName: serviceName, serviceVersion: "1.0"));

                        tracing.AddZipkinExporter(zipkin => {
                            zipkin.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
                        });
                    });

            return builder;
        }

        public static WebApplication UsePrometheus(this WebApplication app) {
            app.MapPrometheusScrapingEndpoint();
            return app;
        }
    }
}