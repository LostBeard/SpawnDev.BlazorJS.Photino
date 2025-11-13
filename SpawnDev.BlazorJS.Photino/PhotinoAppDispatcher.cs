using System.Text.Json;

namespace SpawnDev.BlazorJS.Photino;
/// <summary>
/// Handles interop between a Blazor WebAssembly app and the hosting Photino app
/// </summary>
public class PhotinoAppDispatcher : RemoteDispatcher, IAsyncBackgroundService
{
    Task? _Ready = null;
    /// <inheritdoc/>
    public Task Ready => _Ready ??= InitAsync();
    BlazorJSRuntime JS;
    public JsonSerializerOptions SerializerOptions { get; private set; } = new JsonSerializerOptions();
    ActionCallback<string>? External_OnMessageCallback;
    /// <summary>
    /// Returns true if the app appears to be running in a Photino window
    /// </summary>
    public bool PhotinoFound { get; }
    /// <summary>
    /// This will be the PhotinoBlazorWASMWindow.Id after the Photino app has connected.
    /// </summary>
    public string? WindowId { get; private set; }
    /// <summary>
    /// New instance
    /// </summary>
    /// <param name="js"></param>
    /// <param name="serviceProvider"></param>
    public PhotinoAppDispatcher(BlazorJSRuntime js, IServiceProvider serviceProvider) : base(serviceProvider, createNewScope: false)
    {
        JS = js;
        RequireRemoteCallableAttribute = false;
        AllowPrivateMethods = true;
        AllowSpecialMethods = true;
        AllowStaticMethods = true;
        AllowNonServiceStaticMethods = true;
        if (JS.IsBrowser)
        {
            PhotinoFound = !JS.IsUndefined("external?.sendMessage") && !JS.IsUndefined("external?.receiveMessage");
        }
    }
    async  Task InitAsync()
    {
        if (PhotinoFound)
        {
            External_OnMessageCallback = new ActionCallback<string>(External_OnMessage);
            JS.CallVoid("external.receiveMessage", External_OnMessageCallback);
            SendReadyFlag();
            await WhenReady;
            WindowId = await Run<PhotinoBlazorWASMWindow, string>(s => s.Id);
#if DEBUG
            JS.Log($"WindowId: {WindowId}");
#endif
        }
    }
    async void External_OnMessage(string message)
    {
        try
        {
            var args = JsonSerializer.Deserialize<List<JsonElement>>(message, SerializerOptions);
            if (args != null)
            {
                await HandleCall(args);
            }
        }
        catch { }
    }
    protected override void SendCall(object?[] args)
    {
        if (!PhotinoFound) return;
        try
        {
            var response = JsonSerializer.Serialize(args, SerializerOptions);
            JS.CallVoid("external.sendMessage", response);
        }
        catch { }
    }
    public override void Dispose()
    {
        base.Dispose();
    }
}
