using System.Collections.Concurrent;
using System.Text;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace WebUI.Core.Extensions;

/// <summary>
/// Manages extension loading, registration, and lifecycle
/// </summary>
public class ExtensionManager
{
    private readonly ConcurrentDictionary<string, ExtensionInfo> _extensions = new();
    private readonly string _extensionsDirectory;
    private WebView2? _webView;

    public ExtensionManager()
    {
        _extensionsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extensions");
    }

    /// <summary>
    /// Initialize the extension manager with a WebView2 instance
    /// </summary>
    public void Initialize(WebView2 webView)
    {
        _webView = webView;
        SetupVirtualHostMapping();
        RegisterCoreExtensions();
    }

    /// <summary>
    /// Set up virtual host mapping for extensions
    /// </summary>
    private void SetupVirtualHostMapping()
    {
        if (_webView?.CoreWebView2 == null) return;

        try
        {
            Console.WriteLine($"Setting up virtual host mapping to extensions directory: {_extensionsDirectory}");
            
            // Map webui.local to the extensions directory for static resources
            _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "webui.local",
                _extensionsDirectory,
                CoreWebView2HostResourceAccessKind.Allow);
            
            // Add navigation event handlers for debugging
            _webView.CoreWebView2.NavigationStarting += (sender, args) => 
            {
                Console.WriteLine($"Navigation starting to: {args.Uri}");
            };
            
            _webView.CoreWebView2.NavigationCompleted += (sender, args) =>
            {
                Console.WriteLine($"Navigation completed. Success: {args.IsSuccess}");
                if (!args.IsSuccess)
                {
                    Console.WriteLine($"Navigation failed with error: {args.WebErrorStatus}");
                }
            };
            
            Console.WriteLine("Virtual host mapping configured");
            Console.WriteLine($"Extensions directory exists: {Directory.Exists(_extensionsDirectory)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting up virtual host mapping: {ex.Message}");
        }
    }

    /// <summary>
    /// Register built-in core extensions
    /// </summary>
    private void RegisterCoreExtensions()
    {
        // Register core extension with elevated privileges
        RegisterExtension(new ExtensionInfo
        {
            Id = "core",
            Name = "Core UI Components",
            Type = ExtensionType.Core,
            BasePath = Path.Combine(_extensionsDirectory, "core"),
            HasPrivilegedAccess = true
        });
    }

    /// <summary>
    /// Register an extension
    /// </summary>
    public void RegisterExtension(ExtensionInfo extension)
    {
        _extensions[extension.Id] = extension;
        Console.WriteLine($"Registered {extension.Type} extension: {extension.Id}");
    }


    /// <summary>
    /// Generate HTML for an extension panel
    /// </summary>
    public string GenerateExtensionHtml(string extensionId, string panelId)
    {
        if (!_extensions.TryGetValue(extensionId, out var extension))
            throw new InvalidOperationException($"Extension not found: {extensionId}");

        var isDevelopment = extension.Type == ExtensionType.Core && extension.DevServerUrl != null;
        var baseUrl = isDevelopment ? extension.DevServerUrl : $"http://webui.local/{extensionId}";

        // Minimal HTML that just loads and mounts the extension
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>{{extensionId}} - {{panelId}}</title>
                <link rel="stylesheet" href="{{baseUrl}}/dist/style.css">
                <style>
                    * { margin: 0; padding: 0; box-sizing: border-box; }
                    body { font-family: system-ui; height: 100vh; overflow: hidden; }
                </style>
            </head>
            <body>
                <div id="extension-root"></div>
                <script type="module">
                    import('{{baseUrl}}/dist/activate.js')
                        .then(m => {
                            if (m.activate) {
                                m.activate();
                                setTimeout(() => {
                                    if (window.webui?.panel?.mountPanel) {
                                        window.webui.panel.mountPanel('{{panelId}}', 'extension-root');
                                    }
                                }, 100);
                            }
                        })
                        .catch(err => {
                            document.getElementById('extension-root').innerHTML = 
                                `<div style="color: red; padding: 20px;">Failed to load extension: ${err.message}</div>`;
                        });
                </script>
            </body>
            </html>
            """;
    }

    /// <summary>
    /// Enable development mode for a core extension
    /// </summary>
    public void EnableDevMode(string extensionId, string devServerUrl)
    {
        if (_extensions.TryGetValue(extensionId, out var extension) && extension.Type == ExtensionType.Core)
        {
            extension.DevServerUrl = devServerUrl;
            Console.WriteLine($"Enabled dev mode for {extensionId} at {devServerUrl}");
        }
    }
}

/// <summary>
/// Extension metadata
/// </summary>
public class ExtensionInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required ExtensionType Type { get; set; }
    public required string BasePath { get; set; }
    public bool HasPrivilegedAccess { get; set; }
    public string? DevServerUrl { get; set; }
}

/// <summary>
/// Extension types with different privilege levels
/// </summary>
public enum ExtensionType
{
    /// <summary>
    /// Core platform extensions with full access
    /// </summary>
    Core,
    
    /// <summary>
    /// User extensions running in isolated iframes
    /// </summary>
    Panel
}