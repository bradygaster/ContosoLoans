using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ContosoLoans {
    public static class OpenTelemetryExtensions {

        public static WebApplicationBuilder UseOpenTelemetry(this WebApplicationBuilder builder) {
            builder.Services
                .AddOpenTelemetry()
                    .WithMetrics(metrics => {
                        metrics
                            .AddPrometheusExporter()
                            .AddMeter("Microsoft.Orleans");
                    })
                    .WithTracing(tracing => {
                        tracing.SetResourceBuilder(
                            ResourceBuilder.CreateDefault()
                                .AddService(serviceName: "GPSTracker", serviceVersion: "1.0"));

                        tracing.AddSource("Microsoft.Orleans.Runtime");
                        tracing.AddSource("Microsoft.Orleans.Application");

                        tracing.AddZipkinExporter(zipkin => {
                            zipkin.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
                        });
                    });

            return builder;
        }
    }
}