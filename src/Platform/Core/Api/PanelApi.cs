using System.Runtime.InteropServices;
using WebUI.Core.Hosting;
using WebUI.Core.Communication;

namespace WebUI.Core.Api;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class PanelApi
{
    private readonly IpcRouter _ipc;
    private readonly Dictionary<string, string> _registeredViews = new();
    private readonly BrowserWindow? _browserWindow;

    public PanelApi(IpcRouter ipc, BrowserWindow? browserWindow = null)
    {
        _ipc = ipc;
        _browserWindow = browserWindow;
    }

    public void RegisterView(string panelId, string url)
    {
        _registeredViews[panelId] = url;
        _ipc.SendAsync("panel.register", new
        {
            panelId,
            url
        });
    }

    public void Open(string panelId)
    {
        Console.WriteLine($"[PanelApi] Open called with panelId: {panelId}");
        
        // Send message to open a panel
        // This will be handled by WindowManager via IpcRouter
        _ipc.SendAsync("panel.open", new
        {
            PanelId = panelId
        });
        
        Console.WriteLine($"[PanelApi] Sent panel.open message for: {panelId}");
    }

    public string On(string eventType, string handlerName)
    {
        var handlerId = Guid.NewGuid().ToString();
        _ipc.On($"panel.{eventType}", async (dynamic payload) =>
        {
            // Call JavaScript handler by name
            await Task.Run(() => InvokeJavaScriptHandler(handlerName, payload?.ToString() ?? string.Empty));
        });
        return handlerId; // Return for cleanup
    }

    // Window control methods for panel management
    public void Minimize()
    {
        if (_browserWindow != null)
        {
            _browserWindow.WindowState = System.Windows.Forms.FormWindowState.Minimized;
        }
    }

    public void Maximize()
    {
        if (_browserWindow != null)
        {
            _browserWindow.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }
    }

    public void Restore()
    {
        if (_browserWindow != null)
        {
            _browserWindow.WindowState = System.Windows.Forms.FormWindowState.Normal;
        }
    }

    public void Close()
    {
        // Close current panel/window
        _browserWindow?.Close();
    }
    
    public void Close(string panelId)
    {
        if (string.IsNullOrEmpty(panelId))
        {
            // If no panel ID provided, close current window
            Close();
        }
        else
        {
            // Send IPC message to close a specific panel
            _ipc.SendAsync("panel.close", new
            {
                PanelId = panelId
            });
        }
    }

    public bool IsMaximized()
    {
        return _browserWindow?.Form.WindowState == System.Windows.Forms.FormWindowState.Maximized;
    }

    public void OpenDevTools()
    {
        if (_browserWindow?.WebView?.CoreWebView2 != null)
        {
            _browserWindow.WebView.CoreWebView2.OpenDevToolsWindow();
        }
    }

    private void InvokeJavaScriptHandler(string handlerName, string payload)
    {
        // This will be called via WebView2's ExecuteScriptAsync in future tasks
    }
}