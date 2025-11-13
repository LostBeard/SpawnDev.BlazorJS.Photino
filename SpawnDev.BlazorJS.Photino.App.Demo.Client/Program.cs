using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.Photino;
using SpawnDev.BlazorJS.Photino.App.Demo.Client;
using SpawnDev.BlazorJS.Photino.App.Demo.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// BlazorJSRuntime (PhotinoAppDispatcher dependency)
builder.Services.AddBlazorJSRuntime(out var JS);

// PhotinoAppDispatcher lets us call into the Photino hosting app (if available)
builder.Services.AddSingleton<PhotinoAppDispatcher>();

// This adds IConsoleLogger provided by PhotinoAppDispatcher which will relay all 
// async method calls to the Photino app instance via an interface DispatchProxy
builder.Services.AddSingleton<IConsoleLogger>(sp => sp.GetRequiredService<PhotinoAppDispatcher>().GetService<IConsoleLogger>());

// Start
await builder.Build().BlazorJSRunAsync();