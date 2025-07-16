using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace WebUI;

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
        };
        
        Application.Run(form);
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