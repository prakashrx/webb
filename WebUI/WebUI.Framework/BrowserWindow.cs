using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace WebUI.Framework;

/// <summary>
/// A simple, instantiable browser window similar to Tauri's API
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
    /// <param name="frameless">Whether to create a frameless window (HTML controls title bar)</param>
    public BrowserWindow(string title = "Browser Window", int width = 800, int height = 600, bool resizable = true, bool devTools = true, bool frameless = false)
    {
        // Create form
        _form = new Form
        {
            Text = title,
            Width = width,
            Height = height,
            FormBorderStyle = frameless ? FormBorderStyle.None : (resizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle),
            MaximizeBox = resizable && !frameless,
            StartPosition = FormStartPosition.CenterScreen
        };

        // Create WebView host
        _webViewHost = new WebViewHost();
        _webViewHost.WebView.Dock = DockStyle.Fill;
        _form.Controls.Add(_webViewHost.WebView);

        // Wire up events
        _form.FormClosed += (s, e) => Closed?.Invoke(this, EventArgs.Empty);
        _webViewHost.MessageReceived += (s, e) => MessageReceived?.Invoke(this, e);

        // Initialize with defaults
        _initializationTask = InitializeAsync(devTools, frameless);
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
    /// Set window position
    /// </summary>
    public void SetPosition(int x, int y)
    {
        _form.Location = new Point(x, y);
    }

    /// <summary>
    /// Set window size
    /// </summary>
    public void SetSize(int width, int height)
    {
        _form.Size = new Size(width, height);
    }

    /// <summary>
    /// Set window title
    /// </summary>
    public void SetTitle(string title)
    {
        _form.Text = title;
    }

    /// <summary>
    /// Make window always on top
    /// </summary>
    public void SetAlwaysOnTop(bool alwaysOnTop)
    {
        _form.TopMost = alwaysOnTop;
    }

    /// <summary>
    /// Minimize the window
    /// </summary>
    public void Minimize()
    {
        _form.WindowState = FormWindowState.Minimized;
    }

    /// <summary>
    /// Maximize the window
    /// </summary>
    public void Maximize()
    {
        _form.WindowState = FormWindowState.Maximized;
    }

    /// <summary>
    /// Restore the window
    /// </summary>
    public void Restore()
    {
        _form.WindowState = FormWindowState.Normal;
    }

    private async Task InitializeAsync(bool devTools, bool frameless)
    {
        var options = new WebViewOptions
        {
            EnableDeveloperTools = devTools,
            EnableScriptDebugging = devTools,
            EnableWebSecurity = true,
            AllowInsecureContent = false,
            EnableHighDpiSupport = true
        };

        await _webViewHost.InitializeAsync(options);
        
        // Add window controls for frameless windows
        if (frameless)
        {
            var windowControls = new WindowControls(_form);
            _webViewHost.AddHostObject("windowControls", windowControls);
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _webViewHost?.Dispose();
        _form?.Dispose();
        _isDisposed = true;
    }
} 