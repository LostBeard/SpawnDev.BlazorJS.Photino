using Microsoft.Extensions.DependencyInjection;
using SpawnDev;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.Photino;
using SpawnDev.BlazorJS.Photino.App.Demo.Client.Services;

namespace HelloPhotinoApp
{
    //NOTE: To hide the console window, go to the project properties and change the Output Type to Windows Application.
    // Or edit the .csproj file and change the <OutputType> tag from "WinExe" to "Exe".
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // create RemoteServiceProviderBuilder
            var builder = PhotinoBlazorWASMAppBuilder.CreateDefault(args);

            builder.Services.AddBackgroundServiceManager();

            // add services that Blazor WebAssembly instances can call
            builder.Services.AddSingleton<IConsoleLogger, ConsoleLogger>(GlobalScope.All);

            // build
            var app = builder.Build();

            // start background services
            app.Services.StartBackgroundServices().Wait();

#if DEBUG
            // url where the Blazor WebAssembly dev server is hosting when DEBUG
            app.Load("https://localhost:7174");
#else
            // path where the Blazor WebAssembly app is when !DEBUG
            app.Load("wwwroot/index.html");
#endif
            app.Run();
        }
    }
}
