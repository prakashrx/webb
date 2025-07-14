using System.Runtime.InteropServices;
using WebUI.Core.Windows;

namespace WebUI.Core.Api;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class PanelApi
{
    private readonly string _extensionId;
    private readonly IpcTransport _ipc;
    private readonly Dictionary<string, string> _registeredViews = new();
    private readonly BrowserWindow? _browserWindow;

    public PanelApi(string extensionId, IpcTransport ipc, BrowserWindow? browserWindow = null)
    {
        _extensionId = extensionId;
        _ipc = ipc;
        _browserWindow = browserWindow;
    }

    public void RegisterView(string panelId, string url)
    {
        _registeredViews[panelId] = url;
        _ipc.Send("panel.register", new
        {
            extensionId = _extensionId,
            panelId,
            url
        });
    }

    public void Open(string panelId)
    {
        if (!_registeredViews.ContainsKey(panelId))
        {
            throw new InvalidOperationException($"Panel '{panelId}' not registered");
        }

        _ipc.Send("panel.open", new
        {
            extensionId = _extensionId,
            panelId
        });
    }

    public void ClosePanel(string panelId)
    {
        _ipc.Send("panel.close", new
        {
            extensionId = _extensionId,
            panelId
        });
    }

    public string On(string eventType, string handlerName)
    {
        var handlerId = Guid.NewGuid().ToString();
        _ipc.RegisterHandler($"panel.{eventType}", handlerId, payload =>
        {
            // Call JavaScript handler by name
            InvokeJavaScriptHandler(handlerName, payload);
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
        _browserWindow?.Close();
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