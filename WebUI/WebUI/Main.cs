using WebUI.Framework;

namespace WebUI;

public sealed partial class Main : Form
{
    private readonly WebViewHost _webViewHost = new();

    public Main()
    {
        try
        {
            InitializeComponent();
            
            // Configure as floating toolbar
            (FormBorderStyle, TopMost, ShowInTaskbar) = (FormBorderStyle.None, true, false);
            
            // Position at top center of screen
            var screen = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
            Location = new Point(
                (screen.Width - Width) / 2,
                0
            );
            
            // Setup WebView2 (already initialized in field declaration)
            
            // Add WebView to form
            Controls.Add(_webViewHost.WebView);
            _webViewHost.WebView.Dock = DockStyle.Fill;
            
            // Handle WebView messages
            _webViewHost.MessageReceived += (sender, message) =>
            {
                if (message == "close")
                {
                    Application.Exit();
                    return;
                }
                
                // Handle JSON messages for drag and menu actions
                try
                {
                    var jsonDoc = System.Text.Json.JsonDocument.Parse(message);
                    var root = jsonDoc.RootElement;
                    
                    if (root.TryGetProperty("type", out var typeElement))
                    {
                        var type = typeElement.GetString();
                        
                        switch (type)
                        {
                            case "menu":
                                if (root.TryGetProperty("action", out var actionElement))
                                {
                                    var action = actionElement.GetString();
                                    MessageBox.Show($"Menu action: {action}\n(Not implemented yet)", "WebUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                break;
                                
                            case "ready":
                                // Toolbar is ready
                                break;
                        }
                    }
                }
                catch
                {
                    // If it's not JSON, treat as simple string command
                    // (for backwards compatibility)
                }
            };
            
            // Handle form load to setup Win32 features after handle is created
            this.Load += (sender, e) =>
            {
                // Now that handle is created, setup Win32 features
                WindowHelper.MakeToolWindow(this);
                EnableDragToMove();
            };
            
            // Load test HTML
            LoadTestPage();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing Main form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

    private async void LoadTestPage()
    {
        try
        {
            await _webViewHost.InitializeAsync(new WebViewOptions
            {
                EnableDeveloperTools = true,
                AdditionalBrowserArguments = ["--disable-web-security", "--allow-running-insecure-content"],
                EnableWebSecurity = false,
                AllowInsecureContent = true
            });
        
        var testHtml = """
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8">
            <title>WebUI Control App</title>
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body { 
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                    background: linear-gradient(135deg, #2d3748 0%, #4a5568 100%);
                    color: white;
                    height: 48px;
                    overflow: hidden;
                    user-select: none;
                }
                .toolbar {
                    display: flex;
                    align-items: center;
                    height: 48px;
                    padding: 0 16px;
                    gap: 16px;
                }
                .logo {
                    display: flex;
                    align-items: center;
                    gap: 8px;
                    font-weight: 600;
                    font-size: 14px;
                }
                .logo-icon {
                    width: 20px;
                    height: 20px;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    border-radius: 4px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    font-size: 12px;
                    font-weight: bold;
                }
                .menu-items {
                    display: flex;
                    gap: 8px;
                    margin-left: auto;
                }
                .menu-item {
                    padding: 6px 12px;
                    border-radius: 4px;
                    font-size: 12px;
                    background: rgba(255,255,255,0.1);
                    border: 1px solid rgba(255,255,255,0.2);
                    cursor: pointer;
                    transition: all 0.2s;
                }
                .menu-item:hover {
                    background: rgba(255,255,255,0.2);
                    border-color: rgba(255,255,255,0.3);
                }
                .close-btn {
                    width: 28px;
                    height: 28px;
                    background: rgba(255,255,255,0.1);
                    border: 1px solid rgba(255,255,255,0.2);
                    border-radius: 4px;
                    color: white;
                    cursor: pointer;
                    font-size: 14px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    transition: all 0.2s;
                }
                .close-btn:hover {
                    background: rgba(255,75,75,0.8);
                    border-color: rgba(255,75,75,1);
                }
                .status-indicator {
                    width: 8px;
                    height: 8px;
                    background: #48bb78;
                    border-radius: 50%;
                    margin-left: 4px;
                }
            </style>
        </head>
        <body>
            <div class="toolbar">
                <div class="logo">
                    <div class="logo-icon">W</div>
                    <span>WebUI</span>
                    <div class="status-indicator"></div>
                </div>
                <div class="menu-items">
                    <div class="menu-item" onclick="showExtensions()">Extensions</div>
                    <div class="menu-item" onclick="showSettings()">Settings</div>
                    <div class="menu-item" onclick="showWorkspace()">Workspace</div>
                </div>
                <button class="close-btn" onclick="window.chrome.webview.postMessage('close');">×</button>
            </div>
            <script>
                // Menu functions
                function showExtensions() {
                    window.chrome.webview.postMessage({ type: 'menu', action: 'extensions' });
                }
                
                function showSettings() {
                    window.chrome.webview.postMessage({ type: 'menu', action: 'settings' });
                }
                
                function showWorkspace() {
                    window.chrome.webview.postMessage({ type: 'menu', action: 'workspace' });
                }
                
                // Test communication
                window.chrome.webview.addEventListener('message', event => {
                    console.log('Received from host:', event.data);
                });
                
                // Send ready message
                setTimeout(() => {
                    window.chrome.webview.postMessage({
                        type: 'ready',
                        message: 'WebUI Toolbar Ready'
                    });
                }, 500);
            </script>
        </body>
        </html>
        """;
        
            await _webViewHost.NavigateAsync(testHtml, isHtml: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing WebView: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

    private void EnableDragToMove()
    {
        // Enable drag-to-move for the WebView2 control
        // Wait for WebView to be ready before attaching event
        _webViewHost.WebViewReady += (sender, e) =>
        {
            _webViewHost.WebView.MouseDown += (s, args) =>
            {
                if (args.Button == MouseButtons.Left)
                {
                    // Use Windows API to enable window dragging
                    WindowHelper.StartDragMove(this);
                }
            };
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            _webViewHost?.Dispose();
        }
        base.Dispose(disposing);
    }
} 