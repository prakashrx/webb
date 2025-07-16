using System.Collections.Concurrent;
using WebUI.Core.Communication;
using WebUI.Core.Hosting;
using WebUI.Core.Panels;

namespace WebUI.Core.Windows;

/// <summary>
/// Manager for all windows/panels in the application.
/// Future: Will support WebView2 pooling for better resource usage.
/// </summary>
public class WindowManager : IDisposable
{
    private readonly IMessageChannel _channel;
    private readonly string _processId = "main";
    private readonly ConcurrentDictionary<string, IPanel> _panels = new();
    private readonly ConcurrentDictionary<string, PanelOptions> _registeredPanels = new();
    private string? _defaultContentPath;
    private bool _isInitialized;
    private bool _isDisposed;
    
    // TODO: Future WebView2 pool
    // private readonly Queue<BrowserWindow> _availableWindows = new();
    // private readonly int _maxPoolSize = 3;
    
    
    /// <summary>
    /// Event raised when a panel is created
    /// </summary>
    public event EventHandler<PanelEventArgs>? PanelCreated;
    
    /// <summary>
    /// Event raised when a panel is closed
    /// </summary>
    public event EventHandler<PanelEventArgs>? PanelClosed;
    
    /// <summary>
    /// Event raised when a message is received from any panel
    /// </summary>
    public event EventHandler<PanelMessage>? MessageReceived;
    
    /// <summary>
    /// The message channel for communication
    /// </summary>
    public IMessageChannel Channel => _channel;
    
    public WindowManager()
    {
        // Create in-process channel for now
        _channel = new InProcessChannel();
        
        // Future: Check if extension host is needed
        // _channel = IsExtensionProcess() 
        //     ? new NamedPipeChannel() 
        //     : new InProcessChannel();
    }
    
    /// <summary>
    /// Initialize the WindowManager
    /// </summary>
    public Task InitializeAsync(string? defaultContentPath = null)
    {
        if (_isInitialized)
            return Task.CompletedTask;
            
        _defaultContentPath = defaultContentPath;
        
        // Create a message bus for window manager operations
        var managerBus = new MessageBus(_channel, _processId, "window-manager");
        
        // Subscribe to panel operations
        managerBus.On<PanelOpenRequest>("panel.open", async (request) =>
        {
            if (request != null)
            {
                Console.WriteLine($"[WindowManager] Received panel.open request for: {request.PanelId}");
                await OpenAsync(request.PanelId);
            }
        });
        
        managerBus.On<PanelCloseRequest>("panel.close", async (request) =>
        {
            if (request != null)
            {
                Close(request.PanelId);
            }
            await Task.CompletedTask;
        });
        
        _isInitialized = true;
        
        // TODO: Pre-create WebView2 pool
        // await InitializeWindowPoolAsync();
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Register a panel that can be opened later
    /// </summary>
    public void RegisterPanel(PanelOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(options.Id))
            throw new ArgumentException("Panel ID cannot be empty", nameof(options));
            
        _registeredPanels[options.Id] = options;
    }
    
    /// <summary>
    /// Open a panel by ID (called from JavaScript via webui.panel.open())
    /// </summary>
    public async Task<IPanel> OpenAsync(string panelId)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("WindowManager must be initialized before opening panels");
            
        // Check if panel is already open
        if (_panels.TryGetValue(panelId, out var existingPanel))
        {
            existingPanel.Show();
            return existingPanel;
        }
        
        // Get registered panel options
        if (!_registeredPanels.TryGetValue(panelId, out var options))
        {
            throw new InvalidOperationException($"Panel '{panelId}' is not registered. Available panels: {string.Join(", ", _registeredPanels.Keys)}");
        }
        
        // Create the panel
        Console.WriteLine($"[WindowManager] Creating panel: {panelId}");
        var panel = await CreatePanelAsync(options);
        
        // Store and show
        _panels[panelId] = panel;
        Console.WriteLine($"[WindowManager] Showing panel: {panelId}");
        panel.Show();
        
        // Raise event
        PanelCreated?.Invoke(this, new PanelEventArgs { PanelId = panelId, Panel = panel });
        
        return panel;
    }
    
    /// <summary>
    /// Create a panel with custom options (not from definition)
    /// </summary>
    public async Task<IPanel> CreateAsync(PanelOptions options)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("WindowManager must be initialized before creating panels");
            
        var panelId = options.Id;
        
        // Check if panel already exists
        if (_panels.ContainsKey(panelId))
        {
            throw new InvalidOperationException($"Panel '{panelId}' already exists");
        }
        
        // Create the panel
        var panel = await CreatePanelAsync(options);
        
        // Store
        _panels[panelId] = panel;
        
        // Raise event
        PanelCreated?.Invoke(this, new PanelEventArgs { PanelId = panelId, Panel = panel });
        
        return panel;
    }
    
    /// <summary>
    /// Close a panel by ID
    /// </summary>
    public void Close(string panelId)
    {
        if (_panels.TryGetValue(panelId, out var panel))
        {
            panel.Close();
            // Removal happens in OnPanelClosed
        }
    }
    
    /// <summary>
    /// Get a panel by ID
    /// </summary>
    public IPanel? GetPanel(string panelId)
    {
        return _panels.TryGetValue(panelId, out var panel) ? panel : null;
    }
    
    /// <summary>
    /// Get all open panels
    /// </summary>
    public IEnumerable<IPanel> GetAllPanels()
    {
        return _panels.Values;
    }
    
    /// <summary>
    /// Send a message to a specific panel
    /// </summary>
    public async Task SendMessageAsync(string panelId, string type, object? data = null)
    {
        if (_panels.TryGetValue(panelId, out var panel))
        {
            await panel.SendMessageAsync(type, data);
        }
    }
    
    /// <summary>
    /// Broadcast a message to all panels
    /// </summary>
    public async Task BroadcastAsync(string type, object? data = null)
    {
        var tasks = _panels.Values.Select(panel => panel.SendMessageAsync(type, data));
        await Task.WhenAll(tasks);
    }
    
    private async Task<IPanel> CreatePanelAsync(PanelOptions options)
    {
        // TODO: Future - try to get window from pool
        // var browserWindow = GetPooledWindow() ?? CreateNewWindow(options);
        
        // Apply default content path if not specified
        if (string.IsNullOrEmpty(options.ContentPath))
        {
            options.ContentPath = _defaultContentPath;
        }
        
        // Create message bus for this panel
        var messageBus = new MessageBus(_channel, _processId, options.Id);
        
        // Create new panel
        var panel = new WebUI.Core.Panels.Panel(options, messageBus);
        
        // Wire up events
        panel.Closed += OnPanelClosed;
        panel.MessageReceived += OnPanelMessageReceived;
        
        // Initialize
        await panel.InitializeAsync();
        
        return panel;
    }
    
    private void OnPanelClosed(object? sender, EventArgs e)
    {
        if (sender is IPanel panel)
        {
            var panelId = panel.Id;
            
            // Remove from collection
            _panels.TryRemove(panelId, out _);
            
            // TODO: Future - return window to pool instead of disposing
            // ReturnToPool(panel.Window);
            
            // Raise event
            PanelClosed?.Invoke(this, new PanelEventArgs { PanelId = panelId, Panel = panel });
        }
    }
    
    private void OnPanelMessageReceived(object? sender, PanelMessage e)
    {
        // Forward to subscribers
        MessageReceived?.Invoke(sender, e);
    }
    
    public void Dispose()
    {
        if (_isDisposed)
            return;
            
        _isDisposed = true;
        
        // Close all panels
        foreach (var panel in _panels.Values)
        {
            panel.Dispose();
        }
        
        _panels.Clear();
        _registeredPanels.Clear();
        
        // TODO: Dispose window pool
        
        GC.SuppressFinalize(this);
    }
}


/// <summary>
/// Event args for panel events
/// </summary>
public class PanelEventArgs : EventArgs
{
    public required string PanelId { get; init; }
    public required IPanel Panel { get; init; }
}

/// <summary>
/// Request to open a panel
/// </summary>
public class PanelOpenRequest
{
    public required string PanelId { get; init; }
}

/// <summary>
/// Request to close a panel
/// </summary>
public class PanelCloseRequest
{
    public required string PanelId { get; init; }
}