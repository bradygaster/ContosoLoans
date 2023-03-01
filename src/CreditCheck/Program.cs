var builder = WebApplication.CreateBuilder(args);
builder.UseOpenTelemetry("CreditCheck");
builder.BuildSiloFromArguments(args);

var app = builder.Build();
app.MapGet("/healthz", () => "CreditCheck is up");
app.UsePrometheus();
app.Run();
