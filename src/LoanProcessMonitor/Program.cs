using ContosoLoans;
using System.Diagnostics;

if (Debugger.IsAttached) {
    await Task.Delay(5000);
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// connect an orleans client 
builder.Services.AddOrleansClient((clientBuilder) => {
    clientBuilder.UseAzureStorageClustering(options => {
        options.ConfigureTableServiceClient("UseDevelopmentStorage=true;");
    });
});

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
