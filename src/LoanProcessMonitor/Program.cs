using ContosoLoans;

// wait for the other services to come up
await Task.Delay(10000);

var builder = WebApplication.CreateBuilder(args);
builder.BuildSiloFromArguments(args);

// Add services to the container.
builder.UseOpenTelemetry("LoanProcessMonitor");
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// add a transient service to get the loan process orchestrator grain
builder.Services.AddTransient<ILoanProcessOrchestratorGrain>((services) => {
    var grainFactory = services.GetRequiredService<IGrainFactory>();
    return grainFactory.GetGrain<ILoanProcessOrchestratorGrain>(0);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// health endpoint
app.MapGet("/health", () => "Loan Process Monitor is up and running.");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.UsePrometheus();
app.Run();
