var builder = WebApplication.CreateBuilder(args);
var app = builder.BuildAppFromArguments(args);

app.MapGet("/healthz", () => "CreditCheck is up");

app.Run();
