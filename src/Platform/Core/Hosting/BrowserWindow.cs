using System.Drawing;
using System.Windows.Forms;

namespace WebUI.Core.Hosting;

/// <summary>
/// A simple, reusable browser window wrapper for WebView2
/// </summary>
public sealed class BrowserWindow : IDisposable
{
    private readonly Form _form;
    private readonly WebViewHost _webViewHost;
    private readonly Task _initializationTask;
    private bool _isDisposed;

    /// <summary>
    /// The underlying WebView2 control
    /// </summary>
    public Microsoft.Web.WebView2.WinForms.WebView2 WebView => _webViewHost.WebView;

    /// <summary>
    /// The underlying Windows Form
    /// </summary>
    public Form Form => _form;

    /// <summary>
    /// Event fired when the browser window is closed
    /// </summary>
    public event EventHandler? Closed;

    /// <summary>
    /// Event fired when a message is received from JavaScript
    /// </summary>
    public event EventHandler<string>? MessageReceived;

    /// <summary>
    /// Create a new browser window
    /// </summary>
    /// <param name="title">Window title</param>
    /// <param name="width">Window width</param>
    /// <param name="height">Window height</param>
    /// <param name="resizable">Whether the window is resizable</param>
    /// <param name="devTools">Whether to enable developer tools</param>
    /// <param name="frameless">Whether to create a frameless window</param>
    public BrowserWindow(string title = "Browser Window", int width = 800, int height = 600, bool resizable = true, bool devTools = true, bool frameless = false)
    {
        // Create form
        _form = new Form
        {
            Text = title,
            Width = width,
            Height = height,
            FormBorderStyle = frameless ? FormBorderStyle.None : (resizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle),
            MaximizeBox = resizable,
            StartPosition = FormStartPosition.CenterScreen
        };

        // Create WebView host
        _webViewHost = new WebViewHost();
        _webViewHost.WebView.Dock = DockStyle.Fill;
        _form.Controls.Add(_webViewHost.WebView);

        // Wire up events
        _form.FormClosed += (s, e) => Closed?.Invoke(this, EventArgs.Empty);
        _webViewHost.MessageReceived += (s, e) => MessageReceived?.Invoke(this, e);

        // Initialize
        _initializationTask = InitializeAsync(devTools);
    }

    /// <summary>
    /// Navigate to a URL
    /// </summary>
    public async Task NavigateAsync(string url)
    {
        await _initializationTask;
        await _webViewHost.NavigateAsync(url);
    }

    /// <summary>
    /// Load HTML content
    /// </summary>
    public async Task LoadHtmlAsync(string html)
    {
        await _initializationTask;
        await _webViewHost.NavigateAsync(html, isHtml: true);
    }

    /// <summary>
    /// Execute JavaScript code
    /// </summary>
    public async Task<string> EvaluateAsync(string script)
    {
        await _initializationTask;
        return await _webViewHost.ExecuteScriptAsync(script);
    }

    /// <summary>
    /// Add a host object for JavaScript interop
    /// </summary>
    public async Task AddHostObjectAsync(string name, object hostObject)
    {
        await _initializationTask;
        _webViewHost.AddHostObject(name, hostObject);
    }

    /// <summary>
    /// Wait for initialization to complete
    /// </summary>
    public async Task WaitForInitializationAsync()
    {
        await _initializationTask;
    }
    
    /// <summary>
    /// Set up virtual host mapping for serving local files
    /// </summary>
    public async Task SetVirtualHostMappingAsync(string hostname, string folderPath)
    {
        await _initializationTask;
        await _webViewHost.SetVirtualHostMappingAsync(hostname, folderPath);
    }

    /// <summary>
    /// Show the browser window
    /// </summary>
    public void Show()
    {
        _form.Show();
    }

    /// <summary>
    /// Hide the browser window
    /// </summary>
    public void Hide()
    {
        _form.Hide();
    }

    /// <summary>
    /// Close the browser window
    /// </summary>
    public void Close()
    {
        _form.Close();
    }

    /// <summary>
    /// Get the current window state
    /// </summary>
    public FormWindowState WindowState
    {
        get => _form.WindowState;
        set => _form.WindowState = value;
    }

    /// <summary>
    /// Get or set the window location
    /// </summary>
    public Point Location
    {
        get => _form.Location;
        set => _form.Location = value;
    }

    /// <summary>
    /// Get or set the window size
    /// </summary>
    public Size Size
    {
        get => _form.Size;
        set => _form.Size = value;
    }

    /// <summary>
    /// Initialize the browser window
    /// </summary>
    private async Task InitializeAsync(bool devTools)
    {
        var options = new WebViewOptions
        {
            EnableDeveloperTools = devTools,
            EnableScriptDebugging = devTools,
            EnableWebSecurity = true,
            EnableHighDpiSupport = true
        };

        await _webViewHost.InitializeAsync(options);
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _webViewHost?.Dispose();
        _form?.Dispose();
        _isDisposed = true;
    }
}