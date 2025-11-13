using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace SpawnDev.BlazorJS.Photino;

public class PhotinoBlazorWASMAppBuilder
{
    public IServiceCollection Services { get; }

    internal PhotinoBlazorWASMAppBuilder()
    {
        Services = new ServiceCollection();
    }

    public static PhotinoBlazorWASMAppBuilder CreateDefault(string[]? args = null)
    {
        PhotinoBlazorWASMAppBuilder photinoBlazorAppBuilder = new PhotinoBlazorWASMAppBuilder();
        photinoBlazorAppBuilder.Services.AddSingleton<IServiceCollection>(photinoBlazorAppBuilder.Services);
        photinoBlazorAppBuilder.Services.AddSingleton<IServiceProvider>(sp => sp);
        photinoBlazorAppBuilder.Services.AddSingleton<PhotinoBlazorWASMApp>();
        return photinoBlazorAppBuilder;
    }

    public PhotinoBlazorWASMApp Build(Action<IServiceProvider>? serviceProviderOptions = null)
    {
        ServiceProvider serviceProvider = Services.BuildServiceProvider();
        var requiredService = serviceProvider.GetRequiredService<PhotinoBlazorWASMApp>();
        serviceProviderOptions?.Invoke(serviceProvider);
        return requiredService;
    }
}
