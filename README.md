# SpawnDev.BlazorJS.Photino
[![NuGet](https://img.shields.io/nuget/dt/SpawnDev.BlazorJS.Photino.svg?label=SpawnDev.BlazorJS.Photino)](https://www.nuget.org/packages/SpawnDev.BlazorJS.Photino) 

SpawnDev.BlazorJS.Photino provides tools for 2 way interop, similar to SignalR, between the native Photino app and Blazor WebAssembly apps running in PhotinoWindow instances.


Photino.Net app `Program.cs`  
```cs
// create RemoteServiceProviderBuilder
var appBuilder = PhotinoBlazorWASMAppBuilder.CreateDefault(args);

// add services that Blazor WebAssembly instances can call using the WebAssembly service PhotinoAppDispatcher
// Singleton services are shared with all windows
// Scoped services are per-window
// Transient are per call

appBuilder.Services.AddSingleton<IConsoleLogger, ConsoleLogger>();

// build
var app = appBuilder.Build();

/// <summary>
/// If true, closing the main window will hide it instead of closing it.<br/>
/// This allows the app to stay alive until all windows are closed.<br/>
/// NOTE: Only supported when PhotinoWindow.IsWindowsPlatform == true<br/>
/// Default: false
/// </summary>
app.IndependentWindows = false;

/// <summary>
/// If true the app will not exit when there are no windows except invisible MainWindow.<br/>
/// Setting this to true is useful for a system tray icon that can be used to create a new window or show the main one.<br/>
/// NOTE: Only supported when PhotinoWindow.IsWindowsPlatform == true<br/>
/// Default: false
/// </summary>
app.InvisibleKeepAlive = false;

#if DEBUG
// url where the Blazor WebAssembly dev server is hosting when DEBUG
// if not set, the app's "wwwroot/index.html" path will be used.
// In production a release build of your Blazor WASM app could be served from there.
app.SetAppBaseUri("https://localhost:7174/");
#endif

// Start app. Show main window
app.Run();
```

Blazor WebAssembly app `Program.cs`  
```cs
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
```

Example usage:  
```razor
@page "/"
@using SpawnDev.BlazorJS.JSObjects

<PageTitle>Home</PageTitle>

<h1>Home</h1>

Connected to Photino app services: @PhotinoAppDispatcher.IsReady
<br />

<button disabled="@(!PhotinoAppDispatcher.IsReady)" class="btn btn-primary" @onclick="OpenWindow">Open Window</button>
<button disabled="@(!PhotinoAppDispatcher.IsReady)" class="btn btn-primary" @onclick="CloseThisWindow">Close this window</button>

@code {
    [Inject]
    PhotinoAppDispatcher PhotinoAppDispatcher { get; set; } = default!;

    private async Task OpenWindow()
    {
        // call PhotinoBlazorWASMApp.OpenWindow() in the Photino host app on the PhotinoBlazorWASMApp service
        await PhotinoAppDispatcher.Run<PhotinoBlazorWASMApp>(s => s.OpenWindow());
    }
    private async Task CloseThisWindow()
    {
        // call PhotinoBlazorWASMWindow.Close() in the Photino host app on this window's PhotinoBlazorWASMWindow instance
        await PhotinoAppDispatcher.Run<PhotinoBlazorWASMWindow>(s => s.Close());
    }
}
```