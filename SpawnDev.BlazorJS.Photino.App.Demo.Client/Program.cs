using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.Photino;
using SpawnDev.BlazorJS.Photino.App.Demo.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// BlazorJSRuntime (dependency)
builder.Services.AddBlazorJSRuntime(out var JS);

// PhotinoAppDispatcher lets us call into the Native runtime hosting the app (if available)
// See Counter page for simple demo
builder.Services.AddSingleton<PhotinoAppDispatcher>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().BlazorJSRunAsync();
