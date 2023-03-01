var builder = WebApplication.CreateBuilder(args);
builder.UseOpenTelemetry("LoanReception");
builder.BuildSiloFromArguments(args);

// openapi stuff
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// build the app
var app = builder.Build();

// turn on swagger ui in development mode
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

// get a grain factory and an orchestrator to use in the endpoints
var grainFactory = app.Services.GetRequiredService<IGrainFactory>();
var orchestrator = grainFactory.GetGrain<ILoanProcessOrchestratorGrain>(0);

// the "new loan app" API endpoint
app.MapPost("/loans", async (LoanApplicationRequest request) => {
    await orchestrator.StartEvaluation(new LoanApplication {
        ApplicationId = Guid.NewGuid(),
        CustomerId = request.CustomerId,
        LoanAmount = request.LoanAmount
    });
    return Results.Accepted();
});

// get a list of the active loans in the system
app.MapGet("/loans", async () => {
    var loans = await orchestrator.GetLoansInProgress();
    return Results.Ok(loans);
});

// get a list of the active loans in the system
app.MapGet("/healthz", () => "Loan Reception is up and running!");

// start the app
app.UsePrometheus();
app.Run();

// gives the API payload a shape so we don't have to pass the domain objects around
public record LoanApplicationRequest(Guid CustomerId, int LoanAmount);