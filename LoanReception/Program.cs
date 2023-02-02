using Orleans.Runtime;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.BuildAppFromArguments(args);
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var grainFactory = app.Services.GetRequiredService<IGrainFactory>();
var orchestrator = grainFactory.GetGrain<ILoanProcessOrchestratorGrain>(0);

app.MapPost("/loans", async (LoanApplicationRequest request) =>
{
    await orchestrator.StartEvaluation(new LoanApplication
    {
        ApplicationId = Guid.NewGuid(),
        CustomerId = request.CustomerId,
        LoanAmount = request.LoanAmount
    });
    return Results.Accepted();
});

app.MapGet("/loans", async () =>
{
    var loans = await orchestrator.GetLoansInProgress();
    return Results.Ok(loans);
});

app.Run();

public record LoanApplicationRequest(Guid CustomerId, int LoanAmount);