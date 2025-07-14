using WebUI.Core.Api;
using WebUI.Core.Windows;

namespace WebUI.Core.Extensions;

/// <summary>
/// Enhances a BrowserWindow with extension capabilities through composition
/// </summary>
public class ExtensionHost : IDisposable
{
    private readonly BrowserWindow _browserWindow;
    private readonly ExtensionManager _extensionManager;
    private readonly IpcTransport _ipcTransport;
    private readonly HostApiBridge _hostApi;
    private bool _isInitialized;

    /// <summary>
    /// The underlying browser window
    /// </summary>
    public BrowserWindow Window => _browserWindow;

    /// <summary>
    /// The extension manager for this host
    /// </summary>
    public ExtensionManager Extensions => _extensionManager;

    /// <summary>
    /// Create a new extension host with a browser window
    /// </summary>
    public ExtensionHost(BrowserWindow browserWindow, string hostId)
    {
        _browserWindow = browserWindow ?? throw new ArgumentNullException(nameof(browserWindow));
        _extensionManager = new ExtensionManager();
        _ipcTransport = new IpcTransport(hostId);
        _hostApi = new HostApiBridge(hostId, _ipcTransport, _browserWindow);
    }

    /// <summary>
    /// Initialize the extension host
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        // Wait for WebView2 to be ready
        await _browserWindow.WaitForInitializationAsync();

        // Initialize extension manager with WebView
        _extensionManager.Initialize(_browserWindow.WebView);

        // Add host API bridge to WebView
        await _browserWindow.AddHostObjectAsync("api", _hostApi);

        // Inject WebUI API JavaScript
        await InjectWebUIApiAsync();

        _isInitialized = true;
    }

    /// <summary>
    /// Inject the WebUI API JavaScript from the bundled file
    /// </summary>
    private async Task InjectWebUIApiAsync()
    {
        try
        {
            var apiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "webui-api", "webui-api.js");
            
            if (!File.Exists(apiPath))
            {
                Console.WriteLine($"WebUI API file not found at: {apiPath}");
                return;
            }
            
            var jsContent = await File.ReadAllTextAsync(apiPath);
            
            // Execute the script to make webui globally available
            var initScript = $@"
                {jsContent}
                
                // Ensure webui is globally available
                if (typeof WebUIApi !== 'undefined' && WebUIApi.webui) {{
                    window.webui = WebUIApi.webui;
                }}
            ";
            
            await _browserWindow.WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(initScript);
            Console.WriteLine("WebUI API JavaScript injected successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to inject WebUI API: {ex.Message}");
        }
    }

    /// <summary>
    /// Load an extension URL (e.g., extension://core/main-toolbar)
    /// </summary>
    public async Task LoadExtensionAsync(string extensionUrl)
    {
        if (!_isInitialized)
            await InitializeAsync();

        // Handle extension:// URLs
        if (extensionUrl.StartsWith("extension://"))
        {
            var path = extensionUrl.Replace("extension://", "");
            var parts = path.Split('/');
            
            if (parts.Length == 2)
            {
                var extensionId = parts[0];
                var panelId = parts[1];
                
                // Generate HTML dynamically
                var html = _extensionManager.GenerateExtensionHtml(extensionId, panelId);
                
                // Load HTML directly (no static files needed!)
                await _browserWindow.LoadHtmlAsync(html);
            }
            else
            {
                throw new ArgumentException("Extension URL must be in format: extension://extensionId/panelId");
            }
        }
        else
        {
            throw new ArgumentException("Invalid extension URL format");
        }
    }

    /// <summary>
    /// Enable development mode for a core extension
    /// </summary>
    public void EnableDevMode(string extensionId, string devServerUrl)
    {
        _extensionManager.EnableDevMode(extensionId, devServerUrl);
    }

    /// <summary>
    /// Register a user extension
    /// </summary>
    public void RegisterExtension(ExtensionInfo extension)
    {
        _extensionManager.RegisterExtension(extension);
    }

    public void Dispose()
    {
        _browserWindow?.Dispose();
    }
}

/// <summary>
/// Factory for creating extension hosts
/// </summary>
public static class ExtensionHostFactory
{
    /// <summary>
    /// Create a new extension host with default settings
    /// </summary>
    public static ExtensionHost Create(string title, int width, int height, ExtensionHostOptions? options = null)
    {
        options ??= new ExtensionHostOptions();
        
        var browserWindow = new BrowserWindow(
            title: title,
            width: width,
            height: height,
            resizable: options.Resizable,
            devTools: options.DevTools,
            frameless: options.Frameless
        );

        return new ExtensionHost(browserWindow, options.HostId ?? Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Create a frameless extension host (for toolbars, etc.)
    /// </summary>
    public static ExtensionHost CreateFrameless(string title, int width, int height)
    {
        return Create(title, width, height, new ExtensionHostOptions 
        { 
            Frameless = true,
            Resizable = false,
            DevTools = true  // Enable DevTools to debug
        });
    }
}

/// <summary>
/// Options for creating an extension host
/// </summary>
public class ExtensionHostOptions
{
    public string? HostId { get; set; }
    public bool Resizable { get; set; } = true;
    public bool DevTools { get; set; } = true;
    public bool Frameless { get; set; } = false;
}