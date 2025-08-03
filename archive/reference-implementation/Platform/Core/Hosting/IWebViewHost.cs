using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace WebUI.Core.Hosting;

/// <summary>
/// Interface for managing WebView instances within a host application
/// </summary>
public interface IWebViewHost
{
    /// <summary>
    /// The WebView2 control instance
    /// </summary>
    WebView2 WebView { get; }

    /// <summary>
    /// Initialize the WebView with specified options
    /// </summary>
    Task InitializeAsync(WebViewOptions options);

    /// <summary>
    /// Navigate to a URL or load HTML content
    /// </summary>
    Task NavigateAsync(string urlOrHtml, bool isHtml = false);

    /// <summary>
    /// Execute JavaScript and return the result
    /// </summary>
    Task<string> ExecuteScriptAsync(string script);

    /// <summary>
    /// Add a host object to the WebView for JavaScript interop
    /// </summary>
    void AddHostObject(string name, object hostObject);

    /// <summary>
    /// Remove a host object from the WebView
    /// </summary>
    void RemoveHostObject(string name);

    /// <summary>
    /// Event fired when the WebView is ready for interaction
    /// </summary>
    event EventHandler WebViewReady;

    /// <summary>
    /// Event fired when a message is received from JavaScript
    /// </summary>
    event EventHandler<string> MessageReceived;
} 