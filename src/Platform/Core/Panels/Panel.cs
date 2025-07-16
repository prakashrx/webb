using System.Text.Json;
using WebUI.Core.Api;
using WebUI.Core.Communication;
using WebUI.Core.Hosting;

namespace WebUI.Core.Panels;

/// <summary>
/// Default implementation of IPanel that combines BrowserWindow with WebUI API
/// </summary>
public class Panel : IPanel
{
    private readonly IpcRouter _ipcRouter;
    private readonly HostApiBridge _hostApi;
    private bool _isDisposed;

    public string Id { get; }
    public BrowserWindow Window { get; }
    public PanelOptions Options { get; }
    public bool IsInitialized { get; private set; }

    public event EventHandler? Closed;
    public event EventHandler<PanelMessage>? MessageReceived;

    public Panel(PanelOptions options, IpcRouter ipcRouter)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _ipcRouter = ipcRouter ?? throw new ArgumentNullException(nameof(ipcRouter));
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
        
        // Set up API bridge
        _hostApi = new HostApiBridge(Id, _ipcRouter, Window);
        
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
        
        // Set up virtual host mapping if content path is provided
        if (!string.IsNullOrEmpty(Options.ContentPath))
        {
            try
            {
                await Window.SetVirtualHostMappingAsync("webui.local", Options.ContentPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not set up virtual host mapping: {ex.Message}");
                // Continue without virtual host mapping - the app can still work
            }
        }
        
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
            throw new InvalidOperationException("Panel must be initialized before showing");
            
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
            panelId = Id
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
                                window.webui.ipc.send('panel.message', event.data);
                            }
                        }
                    });
                    
                    // Load and mount the panel directly
                    const panelId = '{{panelId}}';
                    
                    import('{{baseUrl}}/dist/{{panelId}}.js')
                        .then(module => {
                            // Mount the Svelte component
                            if (module.default) {
                                new module.default({
                                    target: document.getElementById('app')
                                });
                                console.log(`Panel '${panelId}' mounted successfully`);
                            } else {
                                throw new Error('Panel module does not export a default component');
                            }
                        })
                        .catch(err => {
                            console.error('Failed to load panel:', err);
                            document.getElementById('app').innerHTML = 
                                `<div style="color: red; padding: 20px;">
                                    <h3>Failed to load panel</h3>
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
        _ipcRouter.On<PanelMessage>("panel.message", async (message) =>
        {
            if (message != null)
            {
                message.PanelId = Id;
                MessageReceived?.Invoke(this, message);
            }
            await Task.CompletedTask;
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