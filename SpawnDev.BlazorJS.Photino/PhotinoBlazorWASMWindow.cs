using Photino.NET;
using SpawnDev.BlazorJS.Photino.Native;
using System.Text.Json;

namespace SpawnDev.BlazorJS.Photino;
/// <summary>
/// Handles interop between a Photino app and Photino windows hosting Blazor WASM apps.
/// </summary>
public class PhotinoBlazorWASMWindow : RemoteDispatcher
{
    /// <summary>
    /// Window Guid as a string
    /// </summary>
    public string Id => Window.Id.ToString();
    /// <summary>
    /// Photino window
    /// </summary>
    public PhotinoWindow Window { get; }
    /// <summary>
    /// Returns true if the window can be hidden
    /// </summary>
    public bool CanHide => PhotinoWindow.IsWindowsPlatform;
    JsonSerializerOptions SerializerOptions;
    PhotinoBlazorWASMApp PhotinoBlazorWASMApp;
    /// <summary>
    /// New instance
    /// </summary>
    /// <param name="photinoBlazorWASMApp"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="window"></param>
    /// <param name="serializerOptions"></param>
    public PhotinoBlazorWASMWindow(PhotinoBlazorWASMApp photinoBlazorWASMApp, IServiceProvider serviceProvider, PhotinoWindow window, JsonSerializerOptions serializerOptions) : base(serviceProvider, createNewScope: true)
    {
        PhotinoBlazorWASMApp = photinoBlazorWASMApp;
        Window = window;
        SerializerOptions = serializerOptions;
        window.WebMessageReceived += HandleMessage;
        RequireRemoteCallableAttribute = false;
        AllowPrivateMethods = true;
        AllowSpecialMethods = true;
        AllowStaticMethods = true;
        AllowNonServiceStaticMethods = true;
    }
    /// <summary>
    /// True if the window is visible
    /// </summary>
    public bool Visible
    {
        get => _Visible;
        set => Show(value);
    }
    bool _Visible = true;
    /// <summary>
    /// Closes the window
    /// </summary>
    public void Close()
    {
        Window?.Close();
    }
    /// <summary>
    /// Show or hide the window
    /// </summary>
    /// <param name="show"></param>
    public bool Show(bool show)
    {
        if (PhotinoWindow.IsWindowsPlatform)
        {
            var hWnd = Window.WindowHandle;
            if (show)
            {
                _Visible = true;
                // Show the window and restore it if minimized
                NativeMethodsWindows.ShowWindow(hWnd, NativeMethodsWindows.SW_RESTORE);
                NativeMethodsWindows.ShowWindow(hWnd, NativeMethodsWindows.SW_SHOW);
            }
            else
            {
                _Visible = false;
                NativeMethodsWindows.ShowWindow(hWnd, NativeMethodsWindows.SW_HIDE);
            }
            return true;
        }
        return false;
    }
    /// <summary>
    /// Wait for close (only waits if it starts a message pump, such as the MainWindow)<br/>
    /// If it doesn't start a message pump, it just starts the window and shows it
    /// </summary>
    public void WaitForClose()
    {
        Window.WaitForClose();
    }
    /// <summary>
    /// Add this PhotinoWindow and PhotinoBlazorWASMWindow to services this instance can access
    /// </summary>
    /// <param name="parameterType"></param>
    /// <returns></returns>
    protected override async Task<object?> GetServiceAsync(Type parameterType)
    {
        if (parameterType == typeof(PhotinoWindow))
        {
            return Window;
        }
        else if (parameterType == typeof(PhotinoBlazorWASMWindow))
        {
            return this;
        }
        return await base.GetServiceAsync(parameterType);
    }
    async void HandleMessage(object? sender, string message)
    {
        try
        {
            var args = JsonSerializer.Deserialize<List<JsonElement>>(message, SerializerOptions);
            if (args != null)
            {
                await Task.Run(() => HandleCall(args));
            }
        }
        catch
        { 
            // invalid message likely
        }
    }
    /// <inheritdoc/>
    protected override void SendCall(object?[] args)
    {
        var response = JsonSerializer.Serialize(args, SerializerOptions);
        Window.SendWebMessage(response);
    }
    /// <inheritdoc/>
    public override void Dispose()
    {
        Window?.Close();
        base.Dispose();
    }
}
