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
    private readonly MessageBus _messageBus;
    private readonly HostApiBridge _hostApi;
    private bool _isDisposed;

    public string Id { get; }
    public BrowserWindow Window { get; }
    public PanelOptions Options { get; }
    public bool IsInitialized { get; private set; }
    public MessageBus MessageBus => _messageBus;

    public event EventHandler? Closed;
    public event EventHandler<PanelMessage>? MessageReceived;

    public Panel(PanelOptions options, MessageBus messageBus)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
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
        _hostApi = new HostApiBridge(this, _messageBus);
        
        // Wire up events
        Window.Closed += OnWindowClosed;
        // Don't set up IPC handlers until after initialization
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized)
            return;
            
        Console.WriteLine($"[Panel] Initializing panel: {Id}");
        
        // Wait for WebView2 to be ready
        Console.WriteLine($"[Panel] Waiting for WebView2 initialization...");
        await Window.WaitForInitializationAsync();
        Console.WriteLine($"[Panel] WebView2 initialized");
        
        // Add the API bridge to the window
        Console.WriteLine($"[Panel] Adding host API bridge...");
        await Window.AddHostObjectAsync("api", _hostApi);
        Console.WriteLine($"[Panel] Host API bridge added");
        
        // Inject WebUI API JavaScript
        await InjectWebUIApiAsync();
        
        // Set up virtual host mapping if content path is provided
        if (!string.IsNullOrEmpty(Options.ContentPath))
        {
            try
            {
                Console.WriteLine($"[Panel] Setting up virtual host mapping: webui.local -> {Options.ContentPath}");
                await Window.SetVirtualHostMappingAsync("webui.local", Options.ContentPath);
                Console.WriteLine($"[Panel] Virtual host mapping set");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not set up virtual host mapping: {ex.Message}");
                // Continue without virtual host mapping - the app can still work
            }
        }
        
        // Generate and load HTML
        Console.WriteLine($"[Panel] Loading HTML content...");
        var html = GenerateHtml();
        await Window.LoadHtmlAsync(html);
        Console.WriteLine($"[Panel] HTML content loaded");
        
        // Set up message handlers
        SetupMessageHandlers();
        
        IsInitialized = true;
        Console.WriteLine($"[Panel] Panel {Id} initialization complete");
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
                
                // The script already sets window.webui, so just verify it's available
                if (typeof window.webui !== 'undefined') {{
                    console.log('WebUI API injected and available, version:', window.webui.version);
                }} else {{
                    console.error('WebUI API failed to initialize');
                }}
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
        // Use WebView2's PostMessage API for C# → JS communication
        if (Window?.WebView?.CoreWebView2 != null)
        {
            var message = new
            {
                type,
                data,
                panelId = Id
            };
            
            var json = JsonSerializer.Serialize(message);
            Window.WebView.CoreWebView2.PostWebMessageAsString(json);
        }
    }
    
    public async Task ExecuteScriptAsync(string script)
    {
        if (Window?.WebView?.CoreWebView2 != null)
        {
            await Window.WebView.CoreWebView2.ExecuteScriptAsync(script);
        }
    }

    protected virtual string GenerateHtml()
    {
        // Use custom template if provided
        if (!string.IsNullOrEmpty(Options.HtmlTemplate))
            return Options.HtmlTemplate;
            
        // Default template for loading UI modules
        var baseUrl = $"http://webui.local/{Options.UiModule}";
        var componentName = Options.ComponentName ?? Options.Id;
        
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
                    // Set up message handling from WebView2
                    window.chrome.webview.addEventListener('message', (event) => {
                        if (event.data && event.data.type) {
                            // Dispatch to WebUI message system
                            if (window.webui?.message) {
                                window.webui.message._handleMessage(event.data);
                            }
                        }
                    });
                    
                    // Load and mount the panel directly
                    const panelId = '{{componentName}}';
                    
                    import('{{baseUrl}}/dist/{{componentName}}.js')
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

    private void SetupMessageHandlers()
    {
        // Subscribe to "*" to forward ALL message types that arrive at this panel to JavaScript
        // The "*" here means "all message types", not "all panels"
        _messageBus.On<object>("*", async (data) =>
        {
            // Forward all messages to JavaScript via PostMessage
            await SendMessageAsync("message", data);
        });
        
        // Handle specific panel messages in C# (for raising events)
        _messageBus.On<PanelMessage>("panel.message", async (message) =>
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