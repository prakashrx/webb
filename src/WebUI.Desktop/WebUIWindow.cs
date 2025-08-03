using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace WebUI;

/// <summary>
/// WebUI window that hosts a WebView2 control with modern styling
/// </summary>
public class WebUIWindow : RoundedForm
{
    private readonly WebView2 _webView;
    
    public WebView2 WebView => _webView;
    
    public WebUIWindow()
    {
        // Create and add WebView2
        _webView = new WebView2();
        _webView.Dock = DockStyle.Fill;
        _webView.Margin = Padding.Empty;
        _webView.Padding = Padding.Empty;
        
        Controls.Add(_webView);
        
        // Initialize WebView2
        InitializeAsync();
    }
    
    private async void InitializeAsync()
    {
        await _webView.EnsureCoreWebView2Async();
        
        // Enable non-client region support for draggable regions
        _webView.CoreWebView2.Settings.IsNonClientRegionSupportEnabled = true;
    }
    
    protected override void OnResizeEnd()
    {
        base.OnResizeEnd();
        // Also refresh WebView2 after resize
        _webView?.Refresh();
    }
    
}