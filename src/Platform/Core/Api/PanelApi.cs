using System.Runtime.InteropServices;
using WebUI.Core.Communication;
using WebUI.Core.Panels;

namespace WebUI.Core.Api;

/// <summary>
/// COM-visible API for panel operations from JavaScript
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class PanelApi
{
    private readonly IPanel _panel;

    public PanelApi(IPanel panel)
    {
        _panel = panel ?? throw new ArgumentNullException(nameof(panel));
    }

    /// <summary>
    /// Open another panel by ID
    /// </summary>
    public void Open(string panelId)
    {
        Console.WriteLine($"[PanelApi] Open called with panelId: {panelId}");
        
        // Send message to open a panel - will be handled by WindowManager
        // Fire and forget, but stay on current thread to avoid COM issues
        _ = _panel.MessageBus.SendAsync("panel.open", new { PanelId = panelId });
    }

    /// <summary>
    /// Minimize the current panel window
    /// </summary>
    public void Minimize()
    {
        if (_panel.Window?.Form != null)
        {
            _panel.Window.Form.WindowState = System.Windows.Forms.FormWindowState.Minimized;
        }
    }

    /// <summary>
    /// Maximize the current panel window
    /// </summary>
    public void Maximize()
    {
        if (_panel.Window?.Form != null)
        {
            _panel.Window.Form.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }
    }

    /// <summary>
    /// Restore the current panel window to normal state
    /// </summary>
    public void Restore()
    {
        if (_panel.Window?.Form != null)
        {
            _panel.Window.Form.WindowState = System.Windows.Forms.FormWindowState.Normal;
        }
    }

    /// <summary>
    /// Close the current panel
    /// </summary>
    public void Close()
    {
        _panel.Close();
    }
    
    /// <summary>
    /// Close a specific panel by ID
    /// </summary>
    public void ClosePanel(string panelId)
    {
        if (string.IsNullOrEmpty(panelId))
        {
            // If no panel ID provided, close current window
            Close();
        }
        else
        {
            // Send message to close a specific panel
            // Fire and forget, but stay on current thread to avoid COM issues
            _ = _panel.MessageBus.SendAsync("panel.close", new { PanelId = panelId });
        }
    }

    /// <summary>
    /// Check if the window is maximized
    /// </summary>
    public bool IsMaximized()
    {
        return _panel.Window?.Form.WindowState == System.Windows.Forms.FormWindowState.Maximized;
    }

    /// <summary>
    /// Open developer tools for the current panel
    /// </summary>
    public void OpenDevTools()
    {
        if (_panel.Window?.WebView?.CoreWebView2 != null)
        {
            _panel.Window.WebView.CoreWebView2.OpenDevToolsWindow();
        }
    }
    
    /// <summary>
    /// Get the current panel's ID
    /// </summary>
    public string GetId()
    {
        return _panel.Id;
    }
    
    /// <summary>
    /// Get the current panel's title
    /// </summary>
    public string GetTitle()
    {
        return _panel.Options.Title;
    }
}