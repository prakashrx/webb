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
            
            webView.NavigateToString($@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>{mainPanelName}</title>
                    <style>
                        body {{
                            font-family: system-ui;
                            display: flex;
                            align-items: center;
                            justify-content: center;
                            height: 100vh;
                            margin: 0;
                            background: #f3f4f6;
                        }}
                        .container {{
                            text-align: center;
                            padding: 2rem;
                            background: white;
                            border-radius: 8px;
                            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>🎉 WebUI is running!</h1>
                        <p>Panel: {mainPanelName}</p>
                        <p>Soon this will load your Svelte component!</p>
                    </div>
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