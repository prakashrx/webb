using System.Windows.Forms;
using WebUI.Core.Windows;

namespace WebUI.Workbench;

public partial class WorkbenchEntry : Form
{
    private BrowserWindow? _browserWindow;

    public WorkbenchEntry()
    {
        InitializeComponent();
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            // Hide the main form (we just use it to manage app lifecycle)
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
            
            // Create a simple browser window demo with frameless window
            _browserWindow = new BrowserWindow("WebUI Demo", 1000, 700, resizable: true, devTools: true, frameless: true);
            
            // Load a demo page with custom title bar
            await _browserWindow.LoadHtmlAsync("""
                <html>
                <head>
                    <title>WebUI Demo</title>
                    <style>
                        * {
                            margin: 0;
                            padding: 0;
                            box-sizing: border-box;
                        }
                        
                        body { 
                            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            color: white;
                            height: 100vh;
                            display: flex;
                            flex-direction: column;
                        }
                        
                        /* Custom Title Bar */
                        .titlebar {
                            background: rgba(0, 0, 0, 0.3);
                            height: 40px;
                            display: flex;
                            align-items: center;
                            justify-content: space-between;
                            padding: 0 15px;
                            backdrop-filter: blur(10px);
                            border-bottom: 1px solid rgba(255, 255, 255, 0.1);
                        }
                        
                        .titlebar-drag {
                            flex: 1;
                            height: 100%;
                            display: flex;
                            align-items: center;
                            -webkit-app-region: drag;
                        }
                        
                        .window-title {
                            font-size: 14px;
                            font-weight: 500;
                            color: rgba(255, 255, 255, 0.9);
                        }
                        
                        .window-controls {
                            display: flex;
                            align-items: center;
                            gap: 1px;
                        }
                        
                        .window-control {
                            width: 32px;
                            height: 32px;
                            background: transparent;
                            border: none;
                            color: rgba(255, 255, 255, 0.8);
                            cursor: pointer;
                            display: flex;
                            align-items: center;
                            justify-content: center;
                            transition: background-color 0.2s ease;
                            font-size: 14px;
                            -webkit-app-region: no-drag;
                        }
                        
                        .window-control:hover {
                            background: rgba(255, 255, 255, 0.1);
                        }
                        
                        .window-control.close:hover {
                            background: #e74c3c;
                        }
                        
                        /* Main Content */
                        .content {
                            flex: 1;
                            display: flex;
                            align-items: center;
                            justify-content: center;
                            padding: 40px;
                        }
                        
                        .container {
                            background: rgba(255, 255, 255, 0.1);
                            backdrop-filter: blur(10px);
                            border-radius: 20px;
                            padding: 40px;
                            max-width: 600px;
                            text-align: center;
                        }
                        
                        h1 { 
                            margin-bottom: 20px;
                            font-size: 2.5em;
                        }
                        
                        p { 
                            margin-bottom: 30px; 
                            opacity: 0.9;
                            font-size: 1.1em;
                        }
                        
                        .demo-button {
                            background: rgba(255, 255, 255, 0.2);
                            border: 2px solid rgba(255, 255, 255, 0.3);
                            color: white;
                            padding: 12px 24px;
                            margin: 10px;
                            border-radius: 10px;
                            cursor: pointer;
                            transition: all 0.3s ease;
                            font-size: 14px;
                        }
                        
                        .demo-button:hover {
                            background: rgba(255, 255, 255, 0.3);
                            transform: translateY(-2px);
                        }
                        
                        .status {
                            margin-top: 20px;
                            padding: 15px;
                            background: rgba(0, 0, 0, 0.2);
                            border-radius: 10px;
                            font-size: 14px;
                            opacity: 0.8;
                        }
                        
                        code {
                            background: rgba(0, 0, 0, 0.3);
                            padding: 2px 6px;
                            border-radius: 4px;
                            font-family: 'Consolas', 'Monaco', monospace;
                            font-size: 0.9em;
                        }
                    </style>
                </head>
                <body>
                    <!-- Custom Title Bar -->
                    <div class="titlebar">
                        <div class="titlebar-drag">
                            <div class="window-title">🚀 WebUI Platform - Extension Framework</div>
                        </div>
                        <div class="window-controls">
                            <button class="window-control minimize" onclick="minimizeWindow()">−</button>
                            <button class="window-control maximize" onclick="toggleMaximize()">□</button>
                            <button class="window-control close" onclick="closeWindow()">×</button>
                        </div>
                    </div>
                    
                    <!-- Main Content -->
                    <div class="content">
                        <div class="container">
                            <h1>🚀 WebUI Platform</h1>
                            <p>Your extension platform is ready!</p>
                            <p>This window uses the new <code>webui.api</code> interface for clean JavaScript-to-C# communication.</p>
                            
                            <div>
                                <button class="demo-button" onclick="testWindowControls()">Test Window Controls</button>
                                <button class="demo-button" onclick="showDevTools()">Open Dev Tools</button>
                                <button class="demo-button" onclick="testMessage()">Test Message</button>
                            </div>
                            
                            <div class="status" id="status">
                                Try the new webui.api interface! Clean JavaScript-to-C# communication.
                            </div>
                        </div>
                    </div>
                    
                    <script>
                        // Create WebUI Platform API
                        window.webui = {
                            api: {
                                window: {
                                    minimize: () => window.chrome.webview.hostObjects.api.Minimize(),
                                    maximize: () => window.chrome.webview.hostObjects.api.Maximize(),
                                    restore: () => window.chrome.webview.hostObjects.api.Restore(),
                                    close: () => window.chrome.webview.hostObjects.api.Close(),
                                    isMaximized: () => window.chrome.webview.hostObjects.api.IsMaximized(),
                                    toggleMaximize: () => {
                                        if (webui.api.window.isMaximized()) {
                                            webui.api.window.restore();
                                        } else {
                                            webui.api.window.maximize();
                                        }
                                    }
                                }
                            }
                        };
                        
                        function minimizeWindow() {
                            webui.api.window.minimize();
                            updateStatus('Window minimized');
                        }
                        
                        function toggleMaximize() {
                            webui.api.window.toggleMaximize();
                            updateStatus(webui.api.window.isMaximized() ? 'Window maximized' : 'Window restored');
                        }
                        
                        function closeWindow() {
                            webui.api.window.close();
                        }
                        
                        function testWindowControls() {
                            updateStatus('Testing window controls...');
                            setTimeout(() => {
                                webui.api.window.minimize();
                                setTimeout(() => {
                                    webui.api.window.restore();
                                    updateStatus('Window controls test complete!');
                                }, 1000);
                            }, 500);
                        }
                        
                        function showDevTools() {
                            updateStatus('Press F12 or right-click -> Inspect to open dev tools');
                        }
                        
                        function testMessage() {
                            window.chrome.webview.postMessage('Hello from JavaScript!');
                            updateStatus('Message sent to host application');
                        }
                        
                        function updateStatus(message) {
                            const status = document.getElementById('status');
                            status.textContent = message;
                            setTimeout(() => {
                                status.textContent = 'Try the new webui.api interface! Clean JavaScript-to-C# communication.';
                            }, 3000);
                        }
                        
                        // Initialize
                        document.addEventListener('DOMContentLoaded', () => {
                            updateStatus('Extension platform ready! Use webui.api for clean JavaScript-to-C# communication.');
                        });
                    </script>
                </body>
                </html>
                """);
            
            // Handle messages from JavaScript
            _browserWindow.MessageReceived += (sender, message) =>
            {
                Console.WriteLine($"Received message: {message}");
            };
            
            // Handle browser window closing
            _browserWindow.Closed += (sender, e) =>
            {
                // Close the application when the browser window closes
                Application.Exit();
            };
            
            // Show the browser window
            _browserWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        // 
        // WorkbenchEntry
        // 
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(284, 261);
        this.Name = "WorkbenchEntry";
        this.Text = "WebUI Workbench";
        this.ResumeLayout(false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _browserWindow?.Dispose();
        }
        base.Dispose(disposing);
    }
} 