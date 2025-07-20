using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;
using WebUI.Bridge;
using WebUI.Commands;

namespace WebUI;

// Command argument classes
public class EchoArgs
{
    public string Message { get; set; } = "";
}

public class AddNumbersArgs
{
    public int A { get; set; }
    public int B { get; set; }
}

public static class WebUI
{
    public static void Run(string mainPanelName)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        var form = new Form
        {
            Text = mainPanelName,
            Width = 800,
            Height = 600
        };
        
        var webView = new WebView2
        {
            Dock = DockStyle.Fill
        };
        
        form.Controls.Add(webView);
        
        form.Load += async (s, e) =>
        {
            await webView.EnsureCoreWebView2Async();
            
            // Set up COM bridge
            var bridge = new HostApiBridge(webView);
            webView.CoreWebView2.AddHostObjectToScript("api", bridge);
            
            // Enable DevTools in debug mode
            #if DEBUG
            webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            #endif
            
            // Register test commands
            RegisterCommands();
            
            // Set up virtual host for serving files
            var panelsPath = Path.Combine(AppContext.BaseDirectory, "panels");
            if (Directory.Exists(panelsPath))
            {
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "webui.local",
                    panelsPath,
                    Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);
            }
            
            webView.NavigateToString($@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>{mainPanelName}</title>
                    <meta charset=""utf-8"">
                    <style>
                        body {{
                            font-family: system-ui, -apple-system, sans-serif;
                            margin: 0;
                            padding: 0;
                        }}
                    </style>
                </head>
                <body>
                    <div id=""app""></div>
                    <script type=""module"">
                        import Panel from 'http://webui.local/{mainPanelName}.js';
                        
                        // Mount the Svelte component
                        new Panel({{
                            target: document.getElementById('app')
                        }});
                    </script>
                </body>
                </html>
            ");
            
            // Set up hot reload in development
            if (Directory.Exists(panelsPath))
            {
                SetupHotReload(webView, panelsPath, mainPanelName);
            }
        };
        
        Application.Run(form);
    }
    
    private static void RegisterCommands()
    {
        // Echo command - returns the message sent
        CommandRegistry.Register<EchoArgs, string>("echo", async (args) =>
        {
            await Task.Delay(100); // Simulate some work
            return $"Echo: {args.Message}";
        });
        
        // Get time command - returns current time
        CommandRegistry.Register<string>("get-time", async () =>
        {
            await Task.Delay(50);
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        });
        
        // Add numbers command
        CommandRegistry.Register<AddNumbersArgs, int>("add-numbers", async (args) =>
        {
            await Task.Delay(50);
            return args.A + args.B;
        });
    }
    
    private static void SetupHotReload(WebView2 webView, string panelsPath, string panelName)
    {
        var watcher = new FileSystemWatcher(panelsPath)
        {
            Filter = $"{panelName}.js",
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };
        
        System.Threading.Timer? debounceTimer = null;
        
        watcher.Changed += (sender, e) =>
        {
            // Debounce rapid changes
            debounceTimer?.Dispose();
            debounceTimer = new System.Threading.Timer(_ =>
            {
                // Reload on UI thread
                webView.BeginInvoke(() =>
                {
                    Console.WriteLine($"🔄 Reloading {panelName}...");
                    webView.Reload();
                });
            }, null, 100, Timeout.Infinite);
        };
        
        watcher.EnableRaisingEvents = true;
        Console.WriteLine($"🔥 Hot reload enabled for {panelName}");
        Console.WriteLine($"💡 Run 'dotnet watch msbuild /t:watch' in another terminal to compile Svelte changes");
    }
    
    public static WebUIApp Create(string[] args)
    {
        return new WebUIApp();
    }
}

public class WebUIApp
{
    public void Run(string mainPanelName)
    {
        WebUI.Run(mainPanelName);
    }
}