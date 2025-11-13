using Photino.NET;
using SpawnDev.BlazorJS.Photino.JsonConverters;
using System.Text.Json;
using File = System.IO.File;

namespace SpawnDev.BlazorJS.Photino;
/// <summary>
/// 
/// </summary>
public class PhotinoBlazorWASMApp
{
    //TaskCompletionSource exitTokenSource = new TaskCompletionSource();
    /// <summary>
    /// Returns the window count
    /// </summary>
    /// <returns></returns>
    public int GetWindowCount()
    {
        return Windows.Count;
    }
    /// <summary>
    /// Active windows
    /// </summary>
    public List<PhotinoBlazorWASMWindow> Windows { get; } = new List<PhotinoBlazorWASMWindow>();
    /// <summary>
    /// Serialization options used for interop
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; private set; } = new JsonSerializerOptions();
    /// <summary>
    /// Service provider
    /// </summary>
    public IServiceProvider Services { get; private set; }
    /// <summary>
    /// New instance
    /// </summary>
    /// <param name="serviceProvider"></param>
    public PhotinoBlazorWASMApp(IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
        SerializerOptions.Converters.Add(new IntPtrJsonConverter());
        SerializerOptions.Converters.Add(new ClaimsIdentityConverter());
    }
    /// <summary>
    /// Returns true if running
    /// </summary>
    public bool Running { get; private set; }
    /// <summary>
    /// Starts the root window
    /// </summary>
    public void Run()
    {
        if (Running) return;
        Running = true;
        if (MainWindow == null)
        {
            OpenWindow();
        }
        MainWindow!.WaitForClose();
        // wait for all windows to close
        //exitTokenSource.Task.Wait();
    }
    PhotinoBlazorWASMWindow AddWindow(PhotinoWindow window)
    {
        var instance = Windows.FirstOrDefault(x => x.Window == window);
        if (instance != null) return instance;
        instance = new PhotinoBlazorWASMWindow(this, Services, window, SerializerOptions);
        Windows.Add(instance);
        window.WindowClosing += Window_WindowClosing;
        return instance;
    }
    /// <summary>
    /// If true, closing the main window will hide it instead of closing it.<br/>
    /// This allows the app to stay alive until all windows are closed.<br/>
    /// NOTE: Only supported when PhotinoWindow.IsWindowsPlatform == true<br/>
    /// Default: false
    /// </summary>
    public bool IndependentWindows
    {
        get => _IndependentWindows;
        set
        {
            if (_IndependentWindows == value) return;
            if (PhotinoWindow.IsWindowsPlatform)
            {
                _IndependentWindows = value;
            }
        }
    }
    bool _IndependentWindows = false;
    /// <summary>
    /// If true the app will not exit when there are no windows except invisible MainWindow.<br/>
    /// Setting this to true is useful for a system tray icon that can be used to create a new window or show the main one.<br/>
    /// NOTE: Only supported when PhotinoWindow.IsWindowsPlatform == true<br/>
    /// Default: false
    /// </summary>
    public bool InvisibleKeepAlive
    {
        get => _InvisibleKeepAlive;
        set
        {
            if (_InvisibleKeepAlive == value) return;
            if (PhotinoWindow.IsWindowsPlatform)
            {
                _InvisibleKeepAlive = value;
            }
        }
    }
    bool _InvisibleKeepAlive = false;
    private bool Window_WindowClosing(object sender, EventArgs e)
    {
        var cancelClose = false;
        var photinoWindow = (PhotinoWindow)sender!;
        if (photinoWindow == MainWindow?.Window)
        {
            if (IndependentWindows && MainWindow.CanHide && MainWindow.Visible)
            {
                // cancel the close and just hide the main window (only supported on Windows)
                // I read that on Mac OS, the main window can be safely closed and the other windows will still work
                MainWindow.Visible = false;
                cancelClose = true;
                return cancelClose;
            }
        }
        RemoveWindow(photinoWindow);
        return cancelClose;
    }
    /// <summary>
    /// The first Uri of the first window opened (root window) AppUri if not already set.<br/>
    /// used for new windows
    /// </summary>
    public Uri? AppBaseUri { get; private set; }
    /// <summary>
    /// Set the app's base Uri
    /// </summary>
    /// <param name="path"></param>
    public void SetAppBaseUri(string path)
    {
        Log(".Load(" + path + ")");
        if (path.Contains("http://") || path.Contains("https://"))
        {
            SetAppBaseUri(new Uri(path));
            return;
        }
        string text = Path.GetFullPath(path);
        if (!File.Exists(text))
        {
            text = AppContext.BaseDirectory + "/" + path;
            if (!File.Exists(text))
            {
                Log(" ** File \"" + path + "\" could not be found.");
                return;
            }
        }
        SetAppBaseUri(new Uri(text, UriKind.Absolute));
    }
    /// <summary>
    /// Set the app's base Uri
    /// </summary>
    public void SetAppBaseUri(Uri path)
    {
        var appUri = GetAppUri(path);
        if (appUri != null)
        {
            AppBaseUri = appUri;
        }
    }
    /// <summary>
    /// Get the Uri, relative to the AppBaseUri if the given uri is relative
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Uri? GetAppUri(Uri path)
    {
        if (!path.IsAbsoluteUri)
        {
            if (AppBaseUri == null)
            {
                var text = Path.GetFullPath(path.ToString());
                if (!File.Exists(text))
                {
                    text = AppContext.BaseDirectory + "/" + path;
                    if (!File.Exists(text))
                    {
                        Log(" ** File \"" + path + "\" could not be found.");
                        return null;
                    }
                }
                path = new Uri(text, UriKind.Absolute);
            }
            else
            {
                path = new Uri(AppBaseUri, path);
            }
        }
        return path;
    }
    void Log(string msg)
    {
        Console.WriteLine(msg);
    }
    /// <summary>
    /// Main window
    /// </summary>
    public PhotinoBlazorWASMWindow? MainWindow => Windows.FirstOrDefault();
    /// <summary>
    /// All windows that are not main
    /// </summary>
    public List<PhotinoBlazorWASMWindow> NonMainWindows => Windows.Where(o => o != MainWindow).ToList();
    /// <summary>
    /// Visible windows
    /// </summary>
    public List<PhotinoBlazorWASMWindow> VisibleWindows => Windows.Where(o => o.Visible).ToList();
    /// <summary>
    /// Create the main window
    /// </summary>
    /// <returns></returns>
    PhotinoBlazorWASMWindow CreateMainWindow()
    {
        if (MainWindow == null)
        {
            if (AppBaseUri == null) SetAppBaseUri("wwwroot/index.html");
            var window = new PhotinoWindow()
                .Load(AppBaseUri);
            AddWindow(window);
        }
        return MainWindow!;
    }
    /// <summary>
    /// Open new app window
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string OpenWindow()
    {
        if (MainWindow == null)
        {
            return CreateMainWindow().Id;
        }
        if (MainWindow == null) throw new Exception($"{nameof(MainWindow)} not set");
        var cts = new TaskCompletionSource<string>();
        MainWindow!.Window.Invoke(() =>
        {
            var window = new PhotinoWindow()
                .SetTitle("");
            window.Load(AppBaseUri);
            var win = AddWindow(window);
            cts.SetResult(win.Id);
            win.WaitForClose();
        });
        var id = cts.Task.Result;
        return id;
    }
    /// <summary>
    /// Open window at the specified url
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string OpenWindow(Uri url)
    {
        if (url == null) throw new Exception($"{nameof(url)} not set");
        if (MainWindow == null)
        {
            SetAppBaseUri(url);
            return CreateMainWindow().Id;
        }
        var cts = new TaskCompletionSource<string>();
        MainWindow!.Window.Invoke(() =>
        {
            var window = new PhotinoWindow()
                .SetTitle("");
            window.Load(url);
            var win = AddWindow(window);
            cts.SetResult(win.Id);
            win.WaitForClose();
        });
        var id = cts.Task.Result;
        return id;
    }
    /// <summary>
    /// Returns the PhotinoBlazorWASMWindow that owns the given PhotinoWindow, or null if not found
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public PhotinoBlazorWASMWindow? GetBlazorWASMWindow(PhotinoWindow window) => Windows.FirstOrDefault(x => x.Window == window);
    void RemoveWindow(PhotinoWindow window)
    {
        var instance = Windows.FirstOrDefault(x => x.Window == window);
        if (instance == null) return;
        RemoveWindow(instance);
    }
    void RemoveWindow(PhotinoBlazorWASMWindow instance)
    {
        if (Windows.Contains(instance))
        {
            if (instance != MainWindow)
            {
                Windows.Remove(instance);
                instance.Window.WindowClosing -= Window_WindowClosing;
                instance.Dispose();
                //if (!NonMainWindows.Any() && RootWindowFauxClosed)
                //{
                //    exitTokenSource.TrySetResult();
                //}
                if (!NonMainWindows.Any() && MainWindow?.Visible != true && !InvisibleKeepAlive)
                {
                    // no other windows and main window is not visible and InvisibleKeepAlive == false
                    MainWindow!.Dispose();
                }
            }
            else
            {
                // when the root window is closed, all windows need to
                // be closed to prevent orphaned windows from freezing
                foreach (var win in NonMainWindows.ToList())
                {
                    win.Close();
                }
            }
        }
    }
}
