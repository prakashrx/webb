using System.IO;
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
            
            // Handle JSON messages for component events
            try
            {
                var jsonDoc = System.Text.Json.JsonDocument.Parse(message);
                var root = jsonDoc.RootElement;
                
                if (root.TryGetProperty("type", out var typeElement))
                {
                    var type = typeElement.GetString();
                    
                    switch (type)
                    {
                        case "webui-menu":
                            if (root.TryGetProperty("action", out var actionElement))
                            {
                                var action = actionElement.GetString();
                                HandleMenuAction(action);
                            }
                            break;
                            
                        case "webui-close":
                            Application.Exit();
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
            
            // Load main toolbar
            LoadMainToolbar();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing Main form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

        private async void LoadMainToolbar()
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
            
            // Get the path to the main toolbar HTML file
            var componentsPath = Path.GetFullPath("../WebUI.Components/public");
            var mainToolbarHtmlPath = Path.Combine(componentsPath, "main-toolbar.html");
            
            if (!File.Exists(mainToolbarHtmlPath))
            {
                throw new FileNotFoundException($"Main toolbar HTML file not found at: {mainToolbarHtmlPath}");
            }
            
            // Navigate to the HTML file
            await _webViewHost.NavigateAsync($"file:///{mainToolbarHtmlPath.Replace("\\", "/")}");
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
    
    private void HandleMenuAction(string action)
    {
        switch (action?.ToLower())
        {
            case "workspace":
                MessageBox.Show("Workspace functionality coming soon!\n\nThis will allow you to:\n• Save/load trading layouts\n• Manage multiple workspace configurations\n• Switch between different setups", 
                    "Workspace", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
                
            case "extensions":
                MessageBox.Show("Extensions functionality coming soon!\n\nThis will allow you to:\n• Browse available trading plugins\n• Install/uninstall extensions\n• Manage extension settings", 
                    "Extensions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
                
            case "settings":
                MessageBox.Show("Settings functionality coming soon!\n\nThis will allow you to:\n• Configure trading preferences\n• Set up API connections\n• Customize UI themes", 
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
                
            case "help":
                MessageBox.Show("WebUI Trading Platform v1.0.0\n\nA modular desktop framework for trading applications.\n\nBuilt with:\n• C# .NET 9.0\n• WebView2\n• Svelte Components", 
                    "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
                
            default:
                MessageBox.Show($"Menu action '{action}' not implemented yet.", "WebUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
        }
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