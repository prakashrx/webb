using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WebUI.Core.Api;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class PanelApi
{
    private readonly string _extensionId;
    private readonly IpcTransport _ipc;
    private readonly Dictionary<string, string> _registeredViews = new();
    private readonly Form? _form;

    public PanelApi(string extensionId, IpcTransport ipc, Form? form = null)
    {
        _extensionId = extensionId;
        _ipc = ipc;
        _form = form;
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
        if (_form != null)
        {
            _form.WindowState = FormWindowState.Minimized;
        }
    }

    public void Maximize()
    {
        if (_form != null)
        {
            _form.WindowState = FormWindowState.Maximized;
        }
    }

    public void Restore()
    {
        if (_form != null)
        {
            _form.WindowState = FormWindowState.Normal;
        }
    }

    public void Close()
    {
        if (_form != null)
        {
            _form.Close();
        }
    }

    public bool IsMaximized()
    {
        return _form?.WindowState == FormWindowState.Maximized;
    }

    private void InvokeJavaScriptHandler(string handlerName, string payload)
    {
        // This will be called via WebView2's ExecuteScriptAsync in future tasks
    }
}