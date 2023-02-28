var builder = WebApplication.CreateBuilder(args);
builder.BuildSiloFromArguments(args);

var app = builder.Build();
app.MapGet("/healthz", () => "CreditCheck is up");
app.Run();
