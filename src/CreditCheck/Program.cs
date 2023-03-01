var builder = WebApplication.CreateBuilder(args);
builder.UseOpenTelemetry("CreditCheck");
builder.BuildSiloFromArguments(args);

var app = builder.Build();
app.MapGet("/", () => "CreditCheck is up and running.");
app.UsePrometheus();
app.Run();
