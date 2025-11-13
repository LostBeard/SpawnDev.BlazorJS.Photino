using Photino.NET;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace SpawnDev.BlazorJS.Photino;
public static class NativeMethods
{
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // ShowWindow commands
    public const int SW_HIDE = 0;
    public const int SW_SHOW = 5;
    public const int SW_MINIMIZE = 6;
    public const int SW_RESTORE = 9;
}
/// <summary>
/// Handles interop between a Photino app and Photino windows hosting Blazor WASM apps.
/// </summary>
public class PhotinoBlazorWASMWindow : RemoteDispatcher
{
    //public bool IsWaitingForClose { get; set; }
    //public bool IsClosed { get; set; }
    //public event Action<PhotinoBlazorWASMWindow> Closed = default!;
    public string Id => Window.Id.ToString();
    public PhotinoWindow Window { get; private set; }
    public JsonSerializerOptions SerializerOptions { get; private set; }
    PhotinoBlazorWASMApp PhotinoBlazorWASMApp;
    public PhotinoBlazorWASMWindow(PhotinoBlazorWASMApp photinoBlazorWASMApp, IServiceProvider serviceProvider, PhotinoWindow window, JsonSerializerOptions serializerOptions) : base(serviceProvider, createNewScope: true)
    {
        PhotinoBlazorWASMApp = photinoBlazorWASMApp;
        Window = window;
        SerializerOptions = serializerOptions;
        window.WebMessageReceived += HandleMessage;
        //window.WindowCreated += Window_WindowCreated;
        // don't require remote callable attribute, but add sane limits to what is callable
        RequireRemoteCallableAttribute = false;
        AllowPrivateMethods = true;
        AllowSpecialMethods = true;
        AllowStaticMethods = true;
        AllowNonServiceStaticMethods = true;
    }
    public bool Visible { get; private set; }
    public void Close()
    {
        Window?.Close();
    }
    public void Show(bool show)
    {
        var hWnd = Window.WindowHandle;
        if (show)
        {
            Visible = true;
            // Show the window and restore it if minimized
            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_SHOW);
        }
        else
        {
            Visible = false;
            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_HIDE);
        }
    }
    public void WaitForClose()
    {
        //if (IsWaitingForClose) return;
        //IsWaitingForClose = true;
        Window.WaitForClose();
        //IsWaitingForClose = false;
        //IsClosed = true;
        //Closed?.Invoke(this);
    }
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
    public void HandleMessage(object? sender, string message)
    {
        try
        {
            var args = JsonSerializer.Deserialize<List<JsonElement>>(message, SerializerOptions);
            if (args != null)
            {
                _ = Task.Run(() => HandleCall(args));
            }
        }
        catch
        {
            var nmt = true;
        }
    }
    protected override void SendCall(object?[] args)
    {
        try
        {
            var response = JsonSerializer.Serialize(args, SerializerOptions);
            Window.SendWebMessage(response);
        }
        catch (Exception ex)
        {
            var nmt = ex.Message;
        }
    }
    public override void Dispose()
    {
        base.Dispose();
    }
}
