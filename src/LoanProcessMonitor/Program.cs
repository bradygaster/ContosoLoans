using ContosoLoans;
using System.Diagnostics;

// useful when debugging to wait for the silos to start up
if (Debugger.IsAttached) {
    await Task.Delay(5000);
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.UseOpenTelemetry();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// connect an orleans client 
builder.Services.AddOrleansClient((clientBuilder) => {
    clientBuilder.UseAzureStorageClustering(options => {
        options.ConfigureTableServiceClient("UseDevelopmentStorage=true;");
    });
});

// add a transient service to get the loan process orchestrator grain
builder.Services.AddTransient<ILoanProcessOrchestratorGrain>((services) => {
    var client = services.GetRequiredService<IClusterClient>();
    return client.GetGrain<ILoanProcessOrchestratorGrain>(0);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// health endpoint
app.MapGet("/healthz", () => "Loan Process Monitor is up and running.");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
