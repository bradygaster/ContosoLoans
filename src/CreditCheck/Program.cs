var builder = WebApplication.CreateBuilder(args);
builder.UseOpenTelemetry();
builder.BuildSiloFromArguments(args);

var app = builder.Build();
app.MapGet("/healthz", () => "CreditCheck is up");
app.Run();
