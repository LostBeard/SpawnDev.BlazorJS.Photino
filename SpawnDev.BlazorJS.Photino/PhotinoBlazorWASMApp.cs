using Photino.NET;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.Photino.JsonConverters;
using System.Text.Json;

namespace SpawnDev.BlazorJS.Photino;

public class PhotinoBlazorWASMApp
{
    TaskCompletionSource exitTokenSource = new TaskCompletionSource();
    SynchronizationContext syncContext;
    public int GetWindowCount()
    {
        return Windows.Count;
    }
    public List<PhotinoBlazorWASMWindow> Windows { get; } = new List<PhotinoBlazorWASMWindow>();
    public JsonSerializerOptions SerializerOptions { get; private set; } = new JsonSerializerOptions();
    /// <summary>
    /// Service provider
    /// </summary>
    public IServiceProvider Services { get; private set; }
    public PhotinoBlazorWASMApp(IServiceProvider serviceProvider)
    {
        syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
        Services = serviceProvider;
        SerializerOptions.Converters.Add(new IntPtrJsonConverter());
    }
    public void Exit()
    {
        exitTokenSource.TrySetResult();
    }
    public void Invoke(Action action)
    {
        syncContext.Send(Call!, action);
    }
    void Call(object obj)
    {
        if (obj is Action action) action();
    }
    public void Run()
    {
        RootWindow!.WaitForClose();
        exitTokenSource.Task.Wait();
    }
    public PhotinoBlazorWASMWindow? GetPhotinoWindowInstance(PhotinoWindow window)
    {
        return Windows.FirstOrDefault(x => x.Window == window);
    }
    public PhotinoBlazorWASMWindow AddWindow(PhotinoWindow window)
    {
        var instance = Windows.FirstOrDefault(x => x.Window == window);
        if (instance != null) return instance;
        instance = new PhotinoBlazorWASMWindow(this, Services, window, SerializerOptions);
        Windows.Add(instance);
        window.WindowClosing += Window_WindowClosing;
        return instance;
    }
    bool RootWindowFauxClosed = false;
    private bool Window_WindowClosing(object sender, EventArgs e)
    {
        var photinoWindow = (PhotinoWindow)sender!;
        if (photinoWindow == RootWindow!.Window)
        {
            RootWindowFauxClosed = true;
            if (NonRootWindows.Any())
            {
                // can't close the main window until it is the last window
                RootWindow.Show(false);
                return true;
            }
        }
        RemoveWindow(photinoWindow);
        return false;
    }
    public string AppUri { get; set; }
    public PhotinoBlazorWASMWindow Load(string uri)
    {
        AppUri = uri;
        var window = new PhotinoWindow();
        // load from the Blazor WASM dev server when in DEBUG
#if DEBUG
        // when debug build, this is the root path
        window.Load(AppUri);
#else
            // when normal build, this is the root path
            window.Load("wwwroot/index.html");
#endif
        // add remote dispatcher to the window
        var win = AddWindow(window);
        return win;
    }
    public PhotinoBlazorWASMWindow? RootWindow => Windows.FirstOrDefault();
    public List<PhotinoBlazorWASMWindow> NonRootWindows => Windows.Count < 2 ? new List<PhotinoBlazorWASMWindow>() : Windows.Skip(1).ToList();
    public string OpenWindow()
    {
        if (RootWindow == null) throw new Exception("Root window not loaded.");
        var cts = new TaskCompletionSource<string>();
        RootWindow.Window.Invoke(() =>
        {
            var window = new PhotinoWindow(RootWindow.Window)
                .SetTitle("");
            window.Load(AppUri);
            var win = AddWindow(window);
            cts.SetResult(win.Id);
            win.WaitForClose();
        });
        var id = cts.Task.Result;
        return id;
    }
    public string OpenWindow(string url)
    {
        if (RootWindow == null) throw new Exception("Root window not loaded.");
        var cts = new TaskCompletionSource<string>();
        RootWindow.Window.Invoke(() =>
        {
            var window = new PhotinoWindow(RootWindow.Window)
                .SetTitle("");
            window.Load(url);
            var win = AddWindow(window);
            cts.SetResult(win.Id);
            win.WaitForClose();
        });
        var id = cts.Task.Result;
        return id;
    }
    PhotinoBlazorWASMWindow? RemoveWindow(PhotinoWindow window)
    {
        var instance = Windows.FirstOrDefault(x => x.Window == window);
        if (instance == null) return instance;
        RemoveWindow(instance);
        return instance;
    }
    void RemoveWindow(PhotinoBlazorWASMWindow instance)
    {
        if (!Windows.Contains(instance)) return;
        Windows.Remove(instance);
        instance.Window.WindowClosing -= Window_WindowClosing;
        if (!NonRootWindows.Any() && RootWindowFauxClosed)
        {
            Exit();
        }
    }
}
