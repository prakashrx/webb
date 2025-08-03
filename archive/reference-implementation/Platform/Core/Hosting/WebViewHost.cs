using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace WebUI.Core.Hosting;

/// <summary>
/// Implementation of IWebViewHost for managing WebView instances
/// </summary>
public sealed class WebViewHost : IWebViewHost, IDisposable
{
    private readonly WebView2 _webView = new();
    private readonly Dictionary<string, object> _hostObjects = [];
    private TaskCompletionSource<bool>? _coreWebView2Ready;
    private bool _isInitialized;
    private bool _isDisposed;

    public WebView2 WebView => _webView;

    public event EventHandler? WebViewReady;
    public event EventHandler<string>? MessageReceived;

    public WebViewHost()
    {
        _coreWebView2Ready = new TaskCompletionSource<bool>();
        SetupWebViewEvents();
    }

    public async Task InitializeAsync(WebViewOptions options)
    {
        if (_isInitialized) return;

        try
        {
            var environmentOptions = new CoreWebView2EnvironmentOptions(
                additionalBrowserArguments: options.AdditionalBrowserArguments is not null ? 
                    string.Join(" ", options.AdditionalBrowserArguments) : null,
                language: null,
                targetCompatibleBrowserVersion: null,
                allowSingleSignOnUsingOSPrimaryAccount: false
            );

            var environment = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: options.UserDataFolder,
                options: environmentOptions);

            await _webView.EnsureCoreWebView2Async(environment);
            ConfigureWebViewSettings(options);

            if (options.ZoomFactor != 1.0)
                _webView.ZoomFactor = options.ZoomFactor;

            _isInitialized = true;
            WebViewReady?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize WebView: {ex.Message}", ex);
        }
    }

    public async Task NavigateAsync(string urlOrHtml, bool isHtml = false)
    {
        EnsureInitialized();

        if (isHtml)
            _webView.NavigateToString(urlOrHtml);
        else
            _webView.CoreWebView2.Navigate(urlOrHtml);

        await Task.Delay(100); // Basic delay, could be improved with proper navigation events
    }

    public async Task<string> ExecuteScriptAsync(string script)
    {
        EnsureInitialized();
        return await _webView.CoreWebView2.ExecuteScriptAsync(script);
    }

    public void AddHostObject(string name, object hostObject)
    {
        EnsureInitialized();
        
        if (_hostObjects.ContainsKey(name))
            RemoveHostObject(name);

        _hostObjects[name] = hostObject;
        _webView.CoreWebView2.AddHostObjectToScript(name, hostObject);
    }

    public void RemoveHostObject(string name)
    {
        EnsureInitialized();
        
        if (_hostObjects.Remove(name))
            _webView.CoreWebView2.RemoveHostObjectFromScript(name);
    }

    private void SetupWebViewEvents()
    {
        _webView.CoreWebView2InitializationCompleted += OnCoreWebView2InitializationCompleted;
        _webView.NavigationCompleted += OnNavigationCompleted;
    }

    private void OnCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (!e.IsSuccess)
        {
            _coreWebView2Ready?.TrySetException(new InvalidOperationException("CoreWebView2 initialization failed"));
            return;
        }

        _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        _webView.CoreWebView2.WindowCloseRequested += OnWindowCloseRequested;
        _webView.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
        
        // Signal that CoreWebView2 is fully ready
        _coreWebView2Ready?.TrySetResult(true);
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        // Navigation completed - could fire events here if needed
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e) =>
        MessageReceived?.Invoke(this, e.TryGetWebMessageAsString());

    private void OnWindowCloseRequested(object? sender, object e)
    {
        // Handle window close requests
    }

    private void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e) =>
        e.Handled = true; // Prevent new windows by default

    private void ConfigureWebViewSettings(WebViewOptions options)
    {
        var settings = _webView.CoreWebView2.Settings;
        
        // Set basic settings
        (settings.IsScriptEnabled, 
         settings.IsWebMessageEnabled, 
         settings.AreDevToolsEnabled) = (true, true, options.EnableDeveloperTools);

        // Disable unwanted features
        (settings.IsGeneralAutofillEnabled,
         settings.IsPasswordAutosaveEnabled,
         settings.AreBrowserAcceleratorKeysEnabled,
         settings.AreDefaultScriptDialogsEnabled,
         settings.AreDefaultContextMenusEnabled) = (false, false, false, false, false);

        // Security and performance settings
        (settings.AreHostObjectsAllowed,
         settings.IsStatusBarEnabled,
         settings.IsSwipeNavigationEnabled,
         settings.IsZoomControlEnabled,
         settings.IsNonClientRegionSupportEnabled,
         settings.IsReputationCheckingRequired) = (true, false, false, false, true, false);
        
        // Try to set version-dependent properties using reflection (graceful fallback)
        TrySetProperty(settings, "IsScriptDebuggingEnabled", options.EnableScriptDebugging);
        TrySetProperty(settings, "IsSmartScreenEnabled", false);
        TrySetProperty(settings, "IsInsecureContentAllowed", options.AllowInsecureContent);
    }

    private static void TrySetProperty(object target, string propertyName, object value)
    {
        try
        {
            target.GetType().GetProperty(propertyName)?.SetValue(target, value);
        }
        catch
        {
            // Ignore if property doesn't exist in this version
        }
    }

    private void EnsureInitialized()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("WebView has not been initialized. Call InitializeAsync first.");
    }
    
    /// <summary>
    /// Set up virtual host mapping for serving local files
    /// </summary>
    public async Task SetVirtualHostMappingAsync(string hostname, string folderPath)
    {
        EnsureInitialized();
        
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"Folder path not found: {folderPath}");
        }
        
        // Wait for CoreWebView2 to be fully initialized
        if (_coreWebView2Ready != null)
        {
            await _coreWebView2Ready.Task;
        }
        
        try
        {
            _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                hostname,
                folderPath,
                CoreWebView2HostResourceAccessKind.Allow);
                
            Console.WriteLine($"Virtual host mapping set: {hostname} -> {folderPath}");
        }
        catch (InvalidCastException ex)
        {
            Console.WriteLine($"Warning: Virtual host mapping not supported by current WebView2 runtime: {ex.Message}");
            Console.WriteLine("Please update WebView2 Runtime or use file:// URLs instead.");
            throw;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        _webView?.Dispose();
        _isDisposed = true;
    }
} 