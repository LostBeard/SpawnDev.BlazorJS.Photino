using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.Photino;
using SpawnDev.BlazorJS.Photino.App.Demo.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// BlazorJSRuntime (PhotinoAppDispatcher dependency)
builder.Services.AddBlazorJSRuntime(out var JS);

// PhotinoAppDispatcher lets us call into the Photino hosting app (if available)
builder.Services.AddSingleton<PhotinoAppDispatcher>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Start
await builder.Build().BlazorJSRunAsync();
