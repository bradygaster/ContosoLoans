var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddKeyPerFile("/secrets", optional: true);
builder.UseOpenTelemetry("CreditCheck");
builder.BuildSiloFromArguments(args);

var app = builder.Build();
app.MapGet("/", () => "CreditCheck is up and running.");
app.UsePrometheus();
app.Run();
