using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorPerApp;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<BlazorPerApp.Services.LogStorage>();
builder.Services.AddScoped<BlazorPerApp.Services.EventService>();

// Configure logging
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);

await builder.Build().RunAsync();
