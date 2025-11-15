using Microsoft.Extensions.DependencyInjection;

namespace SpawnDev.BlazorJS.Photino;
/// <summary>
/// Builds PhotinoBlazorWASMApp
/// </summary>
public class PhotinoBlazorWASMAppBuilder
{
    /// <summary>
    /// PhotinoBlazorWASMApp services collection
    /// </summary>
    public IServiceCollection Services { get; }

    internal PhotinoBlazorWASMAppBuilder()
    {
        Services = new ServiceCollection();
    }
    /// <summary>
    /// Create default
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static PhotinoBlazorWASMAppBuilder CreateDefault(string[]? args = null)
    {
        PhotinoBlazorWASMAppBuilder photinoBlazorAppBuilder = new PhotinoBlazorWASMAppBuilder();
        photinoBlazorAppBuilder.Services.AddSingleton<IServiceCollection>(photinoBlazorAppBuilder.Services);
        photinoBlazorAppBuilder.Services.AddSingleton<IServiceProvider>(sp => sp);
        photinoBlazorAppBuilder.Services.AddSingleton<IWebRootServer, WebRootServer>();
        photinoBlazorAppBuilder.Services.AddSingleton<PhotinoBlazorWASMApp>();
        return photinoBlazorAppBuilder;
    }
    /// <summary>
    /// Builds PhotinoBlazorWASMApp and returns it.
    /// </summary>
    /// <param name="serviceProviderOptions"></param>
    /// <returns></returns>
    public PhotinoBlazorWASMApp Build(Action<IServiceProvider>? serviceProviderOptions = null)
    {
        var serviceProvider = Services.BuildServiceProvider();
        var PhotinoBlazorWASMApp = serviceProvider.GetRequiredService<PhotinoBlazorWASMApp>();
        serviceProviderOptions?.Invoke(serviceProvider);
        return PhotinoBlazorWASMApp;
    }
}
