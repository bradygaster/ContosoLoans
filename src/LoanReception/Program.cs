var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.BuildAppFromArguments(args);
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.InjectStylesheet("/swagger-ui/SwaggerDark.css");
    });
    app.MapGet("/swagger-ui/SwaggerDark.css", async (CancellationToken cancellationToken,
        IWebHostEnvironment env) => {
            var css = await File.ReadAllBytesAsync($"{env.WebRootPath}\\swagger-ui\\SwaggerDark.css", cancellationToken);
            return Results.File(css, "text/css");
        }).ExcludeFromDescription();
}

var grainFactory = app.Services.GetRequiredService<IGrainFactory>();
var orchestrator = grainFactory.GetGrain<ILoanProcessOrchestratorGrain>(0);

app.MapPost("/loans", async (LoanApplicationRequest request) => {
    await orchestrator.StartEvaluation(new LoanApplication {
        ApplicationId = Guid.NewGuid(),
        CustomerId = request.CustomerId,
        LoanAmount = request.LoanAmount
    });
    return Results.Accepted();
});

app.MapGet("/loans", async () => {
    var loans = await orchestrator.GetLoansInProgress();
    return Results.Ok(loans);
});

app.Run();

public record LoanApplicationRequest(Guid CustomerId, int LoanAmount);