using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using WebUI.Core.Api;
using WebUI.Core.Extensions;

namespace WebUI.Core.Windows;

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
    /// Load HTML content or extension URL
    /// </summary>
    public async Task LoadHtmlAsync(string htmlOrUrl)
    {
        await _initializationTask;
        
        // Check if this is an extension:// URL
        if (htmlOrUrl.StartsWith("extension://"))
        {
            Console.WriteLine($"Loading extension URL: {htmlOrUrl}");
            
            // Parse extension and panel from URL
            var urlParts = htmlOrUrl.Replace("extension://", "").Split('/');
            if (urlParts.Length == 2)
            {
                var extensionId = urlParts[0]; // "core"
                var panelId = urlParts[1];     // "main-toolbar"
                
                Console.WriteLine($"Generating HTML for extension: {extensionId}, panel: {panelId}");
                
                // Generate HTML directly and load it
                var html = GenerateExtensionPanelHtml(extensionId, panelId);
                await _webViewHost.NavigateAsync(html, isHtml: true);
                Console.WriteLine("Extension HTML loaded directly");
            }
            else
            {
                Console.WriteLine($"Invalid extension URL format: {htmlOrUrl}");
            }
        }
        else
        {
            await _webViewHost.NavigateAsync(htmlOrUrl, isHtml: true);
        }
    }

    /// <summary>
    /// Set up virtual host mapping for extensions
    /// </summary>
    private void SetupExtensionHostMapping()
    {
        try
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var extensionsDirectory = Path.Combine(baseDirectory, "extensions");
            Console.WriteLine($"Setting up virtual host mapping to extensions directory: {extensionsDirectory}");
            
            // Map webui.local to the extensions directory so extensions can be served
            _webViewHost.WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "webui.local",
                extensionsDirectory,
                CoreWebView2HostResourceAccessKind.Allow);
                
            Console.WriteLine("Virtual host mapping set up successfully");
            
            // Intercept extension panel requests to generate HTML on-the-fly
            _webViewHost.WebView.CoreWebView2.WebResourceRequested += OnExtensionResourceRequested;
            _webViewHost.WebView.CoreWebView2.AddWebResourceRequestedFilter("http://webui.local/*", CoreWebView2WebResourceContext.All);
            
            // Also add navigation event handlers for debugging
            _webViewHost.WebView.CoreWebView2.NavigationStarting += (sender, args) => 
            {
                Console.WriteLine($"Navigation starting to: {args.Uri}");
            };
            
            _webViewHost.WebView.CoreWebView2.NavigationCompleted += (sender, args) =>
            {
                Console.WriteLine($"Navigation completed. Success: {args.IsSuccess}");
                if (!args.IsSuccess)
                {
                    Console.WriteLine($"Navigation failed with error: {args.WebErrorStatus}");
                }
            };
            
            Console.WriteLine("Resource request filter added");
            Console.WriteLine($"Extensions directory exists: {Directory.Exists(extensionsDirectory)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting up extension host mapping: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle extension panel requests and generate HTML on-the-fly
    /// </summary>
    private void OnExtensionResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
        try
        {
            Console.WriteLine($"Resource requested: {e.Request.Uri}");
            
            var uri = new Uri(e.Request.Uri);
            var path = uri.AbsolutePath.TrimStart('/'); // "core/main-toolbar"
            
            Console.WriteLine($"Parsed path: {path}");
            
            // Parse extension and panel from path
            var parts = path.Split('/');
            if (parts.Length == 2)
            {
                var extensionId = parts[0]; // "core" 
                var panelId = parts[1];     // "main-toolbar"
                
                Console.WriteLine($"Generating HTML for extension: {extensionId}, panel: {panelId}");
                
                // Generate HTML for this extension panel
                var html = GenerateExtensionPanelHtml(extensionId, panelId);
                
                // Create response with generated HTML
                var response = _webViewHost.WebView.CoreWebView2.Environment.CreateWebResourceResponse(
                    new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html)),
                    200,
                    "OK",
                    "Content-Type: text/html");
                    
                e.Response = response;
                Console.WriteLine("HTML response created successfully");
            }
            else
            {
                Console.WriteLine($"Invalid path format: {path} (expected extensionId/panelId)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnExtensionResourceRequested: {ex.Message}");
        }
    }

    /// <summary>
    /// Generate HTML for extension panel
    /// </summary>
    private string GenerateExtensionPanelHtml(string extensionId, string panelId)
    {
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>{{extensionId}} - {{panelId}}</title>
                <link rel="stylesheet" href="http://webui.local/{{extensionId}}/dist/style.css">
                <style>
                    * { margin: 0; padding: 0; box-sizing: border-box; }
                    body { font-family: system-ui; height: 100vh; overflow: hidden; }
                </style>
            </head>
            <body>
                <div id="extension-root"></div>
                
                <script type="module">
                    // Load and activate the extension
                    try {
                        console.log('Starting extension loading for {{extensionId}}/{{panelId}}');
                        console.log('WebUI API available:', !!window.webui);
                        
                        const module = await import('http://webui.local/{{extensionId}}/dist/activate.js');
                        console.log('Module loaded:', module);
                        console.log('Module keys:', Object.keys(module));
                        console.log('Module.activate exists:', !!module.activate);
                        console.log('Module.default exists:', !!module.default);
                        
                        if (module.activate) {
                            console.log('Calling activate function...');
                            module.activate(); // This registers panels with webui.panel.registerPanel()
                            console.log('Activate function called');
                        } else if (module.default && module.default.activate) {
                            console.log('Calling default.activate function...');
                            module.default.activate(); // This registers panels with webui.panel.registerPanel()
                            console.log('Default activate function called');
                        } else {
                            console.error('No activate function found in module');
                        }
                        
                        console.log('Available panels:', window.__webuiPanels ? Array.from(window.__webuiPanels.keys()) : 'none');
                        
                        // Get the specific panel component and mount it directly
                        const panelComponent = window.__webuiPanels?.get('{{panelId}}');
                        if (panelComponent) {
                            console.log('Found panel component, mounting...');
                            new panelComponent({ target: document.getElementById('extension-root') });
                            console.log('Panel mounted successfully');
                        } else {
                            console.error('Panel component not found:', '{{panelId}}');
                            console.error('Available panels:', window.__webuiPanels ? Array.from(window.__webuiPanels.keys()) : 'none');
                            document.getElementById('extension-root').innerHTML = 
                                '<div style="padding: 20px; color: red;">Panel "{{panelId}}" not found in extension "{{extensionId}}"</div>';
                        }
                        
                    } catch (error) {
                        console.error('Extension loading error:', error);
                        document.getElementById('extension-root').innerHTML = 
                            '<div style="padding: 20px; color: red;">Failed to load extension: ' + error.message + '</div>';
                    }
                </script>
            </body>
            </html>
            """;
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
    /// Add the WebUI API bridge for extension support
    /// </summary>
    public async Task AddWebUIApiAsync(string extensionId = "workbench")
    {
        await _initializationTask;
        var ipcTransport = new IpcTransport(extensionId);
        var apiBridge = new HostApiBridge(extensionId, ipcTransport, this);
        _webViewHost.AddHostObject("api", apiBridge);
        
        // Inject the WebUI API JavaScript
        await InjectWebUIApiAsync();
    }

    /// <summary>
    /// Inject the WebUI API JavaScript from the bundled file
    /// </summary>
    private async Task InjectWebUIApiAsync()
    {
        try
        {
            // Get the path to the webui-api.js file from the build output
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var apiPath = Path.Combine(baseDirectory, "webui-api", "webui-api.js");
            
            if (!File.Exists(apiPath))
            {
                Console.WriteLine($"WebUI API file not found at: {apiPath}");
                Console.WriteLine($"Make sure the webui-api build completed successfully.");
                return;
            }
            
            // Read the JavaScript content
            var jsContent = await File.ReadAllTextAsync(apiPath);
            
            // Execute the script to make webui globally available
            var initScript = $@"
                {jsContent}
                
                // Ensure webui is globally available (the bundled code exports it)
                if (typeof WebUIApi !== 'undefined' && WebUIApi.webui) {{
                    window.webui = WebUIApi.webui;
                }}
            ";
            
            await _webViewHost.WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(initScript);
            Console.WriteLine("WebUI API JavaScript injected successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to inject WebUI API: {ex.Message}");
        }
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
        
        // Set up virtual host mapping for extensions (do this once during initialization)
        SetupExtensionHostMapping();
        
        // Open dev tools for debugging
        if (devTools)
        {
            _webViewHost.WebView.CoreWebView2.OpenDevToolsWindow();
            Console.WriteLine("Dev tools opened");
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