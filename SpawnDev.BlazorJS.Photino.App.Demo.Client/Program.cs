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
builder.Services.AddBlazorJSRuntime();

// PhotinoAppDispatcher lets Blazor WASM call into the Photino hosting app (if available) using:
// Expressions:
// var result = await PhotinoAppDispatcher.Run<TService, TResult>(service => service.SomeMethod(someVariable1, someVariable2));
// - or  -
// Interface DispatchProxy:
// var service = PhotinoAppDispatcher.GetService<TService>() where TService : interface
// var result = await service.SomeMethod(someVariable1, someVariable2);
// - or -
// Register Photino host app service interface DispatchProxy and use as a normal service
// (See IConsoleLogger below)
builder.Services.AddSingleton<PhotinoAppDispatcher>();

// This adds IConsoleLogger provided by PhotinoAppDispatcher which will relay all 
// async method calls to the Photino app instance via an interface DispatchProxy
builder.Services.AddSingleton(sp => sp.GetRequiredService<PhotinoAppDispatcher>().GetService<IConsoleLogger>());

// Start
await builder.Build().BlazorJSRunAsync();