using System.Collections.Concurrent;
using Microsoft.Web.WebView2.Core;
using WebUI.Core.Hosting;

namespace WebUI.Core.Screens;

/// <summary>
/// Manages the lifecycle of multiple screens and their relationships
/// </summary>
public class ScreenManager : IDisposable
{
    private readonly ConcurrentDictionary<string, IScreen> _screens = new();
    private readonly string _virtualHostPath;
    private bool _isInitialized;
    private bool _isDisposed;

    /// <summary>
    /// Event raised when any screen is closed
    /// </summary>
    public event EventHandler<ScreenClosedEventArgs>? ScreenClosed;

    /// <summary>
    /// Event raised when a message is received from any screen
    /// </summary>
    public event EventHandler<ScreenMessage>? MessageReceived;

    public ScreenManager()
    {
        // Default to UI directory in the application's base directory
        _virtualHostPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui");
    }

    public ScreenManager(string virtualHostPath)
    {
        _virtualHostPath = virtualHostPath ?? throw new ArgumentNullException(nameof(virtualHostPath));
    }

    /// <summary>
    /// Initialize the screen manager
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;
            
        // Ensure virtual host path exists
        if (!Directory.Exists(_virtualHostPath))
        {
            throw new DirectoryNotFoundException($"Virtual host path not found: {_virtualHostPath}");
        }
        
        _isInitialized = true;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Create and initialize a new screen
    /// </summary>
    public async Task<IScreen> CreateScreenAsync(ScreenOptions options)
    {
        if (!_isInitialized)
            await InitializeAsync();
            
        if (_screens.ContainsKey(options.Id))
            throw new InvalidOperationException($"Screen with ID '{options.Id}' already exists");
            
        // Create the screen
        var screen = CreateScreen(options);
        
        // Set up virtual host mapping for the screen's window
        SetupVirtualHostMapping(screen.Window);
        
        // Wire up events
        screen.Closed += OnScreenClosed;
        screen.MessageReceived += OnScreenMessageReceived;
        
        // Initialize the screen
        await screen.InitializeAsync();
        
        // Track the screen
        _screens[options.Id] = screen;
        
        return screen;
    }

    /// <summary>
    /// Get a screen by ID
    /// </summary>
    public IScreen? GetScreen(string screenId)
    {
        _screens.TryGetValue(screenId, out var screen);
        return screen;
    }

    /// <summary>
    /// Get all active screens
    /// </summary>
    public IEnumerable<IScreen> GetAllScreens()
    {
        return _screens.Values;
    }

    /// <summary>
    /// Close a screen by ID
    /// </summary>
    public void CloseScreen(string screenId)
    {
        if (_screens.TryGetValue(screenId, out var screen))
        {
            screen.Close();
        }
    }

    /// <summary>
    /// Close all screens
    /// </summary>
    public void CloseAllScreens()
    {
        foreach (var screen in _screens.Values)
        {
            screen.Close();
        }
    }

    /// <summary>
    /// Send a message to a specific screen
    /// </summary>
    public async Task SendMessageToScreenAsync(string screenId, string type, object? data = null)
    {
        if (_screens.TryGetValue(screenId, out var screen))
        {
            await screen.SendMessageAsync(type, data);
        }
    }

    /// <summary>
    /// Broadcast a message to all screens
    /// </summary>
    public async Task BroadcastMessageAsync(string type, object? data = null)
    {
        var tasks = _screens.Values.Select(screen => screen.SendMessageAsync(type, data));
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Create a screen instance (can be overridden for custom screen types)
    /// </summary>
    protected virtual IScreen CreateScreen(ScreenOptions options)
    {
        return new Screen(options);
    }

    private void SetupVirtualHostMapping(BrowserWindow window)
    {
        // Set up virtual host mapping when WebView2 is ready
        window.WebView.CoreWebView2InitializationCompleted += (sender, args) =>
        {
            if (args.IsSuccess && window.WebView.CoreWebView2 != null)
            {
                try
                {
                    window.WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                        "webui.local",
                        _virtualHostPath,
                        CoreWebView2HostResourceAccessKind.Allow);
                        
                    Console.WriteLine($"Virtual host mapping configured for screen: {_virtualHostPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting up virtual host mapping: {ex.Message}");
                }
            }
        };
    }

    private void OnScreenClosed(object? sender, EventArgs e)
    {
        if (sender is IScreen screen)
        {
            // Remove from tracking
            _screens.TryRemove(screen.Id, out _);
            
            // Unwire events
            screen.Closed -= OnScreenClosed;
            screen.MessageReceived -= OnScreenMessageReceived;
            
            // Raise event
            ScreenClosed?.Invoke(this, new ScreenClosedEventArgs { ScreenId = screen.Id });
            
            // Dispose the screen
            screen.Dispose();
        }
    }

    private void OnScreenMessageReceived(object? sender, ScreenMessage e)
    {
        MessageReceived?.Invoke(this, e);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;
            
        _isDisposed = true;
        
        // Close and dispose all screens
        foreach (var screen in _screens.Values)
        {
            screen.Closed -= OnScreenClosed;
            screen.MessageReceived -= OnScreenMessageReceived;
            screen.Dispose();
        }
        
        _screens.Clear();
        
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Event args for screen closed event
/// </summary>
public class ScreenClosedEventArgs : EventArgs
{
    public required string ScreenId { get; set; }
}