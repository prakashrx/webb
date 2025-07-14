using System.Text.Json;
using WebUI.Core.Api;
using WebUI.Core.Communication;
using WebUI.Core.Hosting;

namespace WebUI.Core.Screens;

/// <summary>
/// Default implementation of IScreen that combines BrowserWindow with WebUI API
/// </summary>
public class Screen : IScreen
{
    private readonly IpcTransport _ipcTransport;
    private readonly HostApiBridge _hostApi;
    private bool _isDisposed;

    public string Id { get; }
    public BrowserWindow Window { get; }
    public ScreenOptions Options { get; }
    public bool IsInitialized { get; private set; }

    public event EventHandler? Closed;
    public event EventHandler<ScreenMessage>? MessageReceived;

    public Screen(ScreenOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Id = options.Id;
        
        // Create browser window with specified options
        Window = new BrowserWindow(
            title: options.Title, 
            width: options.Width, 
            height: options.Height, 
            resizable: options.IsResizable,
            devTools: true,
            frameless: options.IsFrameless
        );
        
        // Set up IPC and API bridge
        _ipcTransport = new IpcTransport(Id);
        _hostApi = new HostApiBridge(Id, _ipcTransport, Window);
        
        // Wire up events
        Window.Closed += OnWindowClosed;
        SetupIpcHandlers();
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized)
            return;
            
        // Wait for WebView2 to be ready
        await Window.WaitForInitializationAsync();
        
        // Add the API bridge to the window
        await Window.AddHostObjectAsync("api", _hostApi);
        
        // Inject WebUI API JavaScript
        await InjectWebUIApiAsync();
        
        // Generate and load HTML
        var html = GenerateHtml();
        await Window.LoadHtmlAsync(html);
        
        IsInitialized = true;
    }
    
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
                
                console.log('WebUI API injected and available');
            ";
            
            await Window.WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(initScript);
            Console.WriteLine("WebUI API JavaScript injected successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to inject WebUI API: {ex.Message}");
        }
    }

    public void Show()
    {
        if (!IsInitialized)
            throw new InvalidOperationException("Screen must be initialized before showing");
            
        Window.Show();
    }

    public void Hide()
    {
        Window.Hide();
    }

    public void Close()
    {
        Window.Close();
    }

    public async Task SendMessageAsync(string type, object? data = null)
    {
        var message = new
        {
            type,
            data,
            screenId = Id
        };
        
        var json = JsonSerializer.Serialize(message);
        var script = $"window.postMessage({json}, '*');";
        
        await Window.EvaluateAsync(script);
    }

    protected virtual string GenerateHtml()
    {
        // Use custom template if provided
        if (!string.IsNullOrEmpty(Options.HtmlTemplate))
            return Options.HtmlTemplate;
            
        // Default template for loading UI modules
        var baseUrl = $"http://webui.local/{Options.UiModule}";
        var panelId = Options.PanelId ?? "default";
        
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>{{Options.Title}}</title>
                <link rel="stylesheet" href="{{baseUrl}}/dist/style.css">
                <style>
                    * { margin: 0; padding: 0; box-sizing: border-box; }
                    body { font-family: system-ui; height: 100vh; overflow: hidden; }
                    #app { width: 100%; height: 100%; }
                </style>
            </head>
            <body>
                <div id="app"></div>
                <script type="module">
                    // Set up message handling
                    window.addEventListener('message', (event) => {
                        if (event.data && event.data.type) {
                            // Forward to WebUI API if available
                            if (window.webui?.ipc?.send) {
                                window.webui.ipc.send('screen.message', event.data);
                            }
                        }
                    });
                    
                    // Load UI module
                    import('{{baseUrl}}/dist/activate.js')
                        .then(m => {
                            if (m.activate) {
                                m.activate();
                                // Mount the specific panel if specified
                                const panelId = '{{panelId}}';
                                if (panelId !== 'default' && window.webui?.panel?.mountPanel) {
                                    setTimeout(() => {
                                        window.webui.panel.mountPanel(panelId, 'app');
                                    }, 100);
                                }
                            }
                        })
                        .catch(err => {
                            console.error('Failed to load UI module:', err);
                            document.getElementById('app').innerHTML = 
                                `<div style="color: red; padding: 20px;">
                                    <h3>Failed to load UI</h3>
                                    <p>${err.message}</p>
                                </div>`;
                        });
                </script>
            </body>
            </html>
            """;
    }

    private void SetupIpcHandlers()
    {
        // Handle messages from JavaScript
        _ipcTransport.RegisterHandler("screen.message", "internal", (payload) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<ScreenMessage>(payload);
                if (message != null)
                {
                    message.ScreenId = Id;
                    MessageReceived?.Invoke(this, message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling screen message: {ex.Message}");
            }
        });
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;
            
        _isDisposed = true;
        
        Window.Closed -= OnWindowClosed;
        Window.Dispose();
        
        GC.SuppressFinalize(this);
    }
}