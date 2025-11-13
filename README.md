# SpawnDev.BlazorJS.Photino
[![NuGet](https://img.shields.io/nuget/dt/SpawnDev.BlazorJS.Photino.svg?label=SpawnDev.BlazorJS.Photino)](https://www.nuget.org/packages/SpawnDev.BlazorJS.Photino) 

Run Blazor WebAssembly in your [Photino.Net](https://github.com/tryphotino/photino.NET) app 
with 2 way interop, similar to SignalR, between the Photino app and Blazor WebAssembly instances.

#### Why
> Photino.Blazor already exists, why use this?  

#### Answer
Like Photino.Blazor, SpawnDev.BlazorJS.Photino provides shared services that run in the Photino app that each window can use.
But SpawnDev.BlazorJS.Photino adds the benefits of Blazor WebAssembly + [SpawnDev.BlazorJS](https://github.com/LostBeard/SpawnDev.BlazorJS) which gives access 
to all of the awesome browser [Web APIs](https://developer.mozilla.org/en-US/docs/Web/API) like WebRTC, Canvas, WebGL, WEbGPU, etc. directly from C#, 
no Javascript required.

### Example

Photino.Net app   
`Program.cs`  
```cs
// Create RemoteServiceProviderBuilder
var appBuilder = PhotinoBlazorWASMAppBuilder.CreateDefault(args);

// Blazor WebAssembly instances can call these services using expressions or 
// an interface DispatchProxy provided by the PhotinoAppDispatcher service
// Singleton services are shared with all windows
// Scoped services are per-window
// Transient are per call

// The demo uses this service via an interface DispatchProxy
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
// Set the Url where the Blazor WebAssembly dev server is hosting when DEBUG
// if not set, the app's "wwwroot/index.html" path will be used.
// In production a release build of your Blazor WASM app could be served from there.
app.SetAppBaseUri("https://localhost:7174/");
#endif

// Start app. Show main window
app.Run();
```

Blazor WebAssembly app  
`Program.cs`  
```cs
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
```

Example usage:  
```razor
Connected to Photino app services: @PhotinoAppDispatcher.IsReady
<br />
<button disabled="@(!PhotinoAppDispatcher.IsReady)" class="btn btn-primary" @onclick="OpenWindow">Open Window</button>
<button disabled="@(!PhotinoAppDispatcher.IsReady)" class="btn btn-primary" @onclick="CloseThisWindow">Close this window</button>

@code {
    [Inject]
    PhotinoAppDispatcher PhotinoAppDispatcher { get; set; } = default!;

    [Inject]
    IConsoleLogger ConsoleLogger { get; set; } = default!;

    private async Task OpenWindow()
    {
        // this calls IConsoleLogger.LogAsync() which relays the call to the Photino host app IConsoleLogger service
        await ConsoleLogger.LogAsync(">> Window being opened by " + PhotinoAppDispatcher.WindowId);

        // call PhotinoBlazorWASMApp.OpenWindow() in the Photino host app on the PhotinoBlazorWASMApp service
        var windowId = await PhotinoAppDispatcher.Run<PhotinoBlazorWASMApp, string>(s => s.OpenWindow());

        // this calls IConsoleLogger.LogAsync() which relays the call to the Photino host app IConsoleLogger service
        await ConsoleLogger.LogAsync(">> Window opened: " + windowId);
    }

    private async Task CloseThisWindow()
    {
        // this calls IConsoleLogger.LogAsync() which relays the call to the Photino host app IConsoleLogger service
        await ConsoleLogger.LogAsync(">> Window closing: " + PhotinoAppDispatcher.WindowId);

        // call PhotinoBlazorWASMWindow.Close() in the Photino host app on this window's PhotinoBlazorWASMWindow instance
        await PhotinoAppDispatcher.Run<PhotinoBlazorWASMWindow>(s => s.Close());
    }
}
```

### Blazor WebAssembly libraries
Blazor WebAssembly libraries ready to use in your next Photino Blazor WebAssembly app.

- [TransformersJS](https://github.com/LostBeard/SpawnDev.BlazorJS.TransformersJS) - Use Transformers.js to run pretrained models with the ONNX Runtime
- [WebTorrents](https://github.com/LostBeard/SpawnDev.BlazorJS.WebTorrents) - WebTorrent peer to peer file sharing
- [SocketIO](https://github.com/LostBeard/SpawnDev.BlazorJS.SocketIO) - Socket.IO bidirectional and low-latency communication for every platform
- [PeerJS](https://github.com/LostBeard/SpawnDev.BlazorJS.PeerJS) - PeerJS simplifies peer-to-peer data, video, and audio calls
- [Cryptography](https://github.com/LostBeard/SpawnDev.BlazorJS.Cryptography) - A cross platform cryptography library ECDSA, ECDH, AES-CBC, etc
- [More](https://github.com/LostBeard) - More Blazor WebAssembly projects by LostBeard
- [Nuget Packages](https://www.nuget.org/profiles/LostBeard) - Blazor WebAssembly Nuget packages by LostBeard
