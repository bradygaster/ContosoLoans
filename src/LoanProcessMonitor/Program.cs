using ContosoLoans;
using System.Diagnostics;

if (Debugger.IsAttached) {
    Console.WriteLine("Pausing to wait for cluster to start. Press any key to continue...");
    Console.ReadKey();
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
