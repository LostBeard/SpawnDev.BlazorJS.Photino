using Microsoft.Extensions.DependencyInjection;
using SpawnDev;
using SpawnDev.BlazorJS.Photino;
using SpawnDev.BlazorJS.Photino.App.Demo.Client.Services;

namespace HelloPhotinoApp
{
    // NOTE: To hide the console window, go to the project properties and change the Output Type to Windows Application.
    // Or edit the .csproj file and change the <OutputType> tag from "WinExe" to "Exe".
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
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
        }
    }
}
