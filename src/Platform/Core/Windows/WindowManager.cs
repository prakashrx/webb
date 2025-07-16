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
    private readonly IpcRouter _ipcRouter;
    private readonly IpcTransport _ipcTransport;
    private readonly ConcurrentDictionary<string, IPanel> _panels = new();
    private readonly ConcurrentDictionary<string, PanelDefinition> _panelDefinitions = new();
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
    /// The IPC router for external communication
    /// </summary>
    public IpcRouter IpcRouter => _ipcRouter;
    
    public WindowManager()
    {
        // Create our own IPC infrastructure
        _ipcTransport = new IpcTransport("workbench");
        _ipcRouter = new IpcRouter(_ipcTransport);
    }
    
    /// <summary>
    /// Initialize the WindowManager
    /// </summary>
    public Task InitializeAsync(string? defaultContentPath = null)
    {
        if (_isInitialized)
            return Task.CompletedTask;
            
        _defaultContentPath = defaultContentPath;
        
        // Subscribe to panel operations via IPC
        _ipcRouter.On<PanelOpenRequest>("panel.open", async (request) =>
        {
            Console.WriteLine($"[WindowManager] Received panel.open request for: {request.PanelId}");
            await OpenAsync(request.PanelId);
        });
        
        _ipcRouter.On<PanelCloseRequest>("panel.close", async (request) =>
        {
            Close(request.PanelId);
            await Task.CompletedTask;
        });
        
        _isInitialized = true;
        
        // TODO: Pre-create WebView2 pool
        // await InitializeWindowPoolAsync();
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Register a panel definition that can be opened later
    /// </summary>
    public void RegisterPanel(string panelId, PanelDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(panelId))
            throw new ArgumentException("Panel ID cannot be empty", nameof(panelId));
            
        _panelDefinitions[panelId] = definition ?? throw new ArgumentNullException(nameof(definition));
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
        
        // Get panel definition
        if (!_panelDefinitions.TryGetValue(panelId, out var definition))
        {
            throw new InvalidOperationException($"Panel '{panelId}' is not registered. Available panels: {string.Join(", ", _panelDefinitions.Keys)}");
        }
        
        // Create the panel
        var panel = await CreatePanelAsync(panelId, definition);
        
        // Store and show
        _panels[panelId] = panel;
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
        var panel = await CreatePanelCoreAsync(options);
        
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
    
    private async Task<IPanel> CreatePanelAsync(string panelId, PanelDefinition definition)
    {
        var options = new PanelOptions
        {
            Id = panelId,
            Title = definition.Title,
            Width = definition.Width,
            Height = definition.Height,
            IsFrameless = definition.IsFrameless,
            IsResizable = definition.IsResizable,
            CanMaximize = definition.CanMaximize,
            CanMinimize = definition.CanMinimize,
            UiModule = definition.UiModule,
            PanelId = definition.PanelId,
            HtmlTemplate = definition.HtmlTemplate,
            ContentPath = definition.ContentPath ?? _defaultContentPath
        };
        
        return await CreatePanelCoreAsync(options);
    }
    
    private async Task<IPanel> CreatePanelCoreAsync(PanelOptions options)
    {
        // TODO: Future - try to get window from pool
        // var browserWindow = GetPooledWindow() ?? CreateNewWindow(options);
        
        // Apply default content path if not specified
        if (string.IsNullOrEmpty(options.ContentPath))
        {
            options.ContentPath = _defaultContentPath;
        }
        
        // For now, create new panel each time
        var panel = new WebUI.Core.Panels.Panel(options, _ipcRouter);
        
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
        _panelDefinitions.Clear();
        
        // TODO: Dispose window pool
        
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Definition for a panel that can be created
/// </summary>
public class PanelDefinition
{
    public required string Title { get; init; }
    public int Width { get; init; } = 800;
    public int Height { get; init; } = 600;
    public bool IsFrameless { get; init; }
    public bool IsResizable { get; init; } = true;
    public bool CanMaximize { get; init; } = true;
    public bool CanMinimize { get; init; } = true;
    public required string UiModule { get; init; }
    public string? PanelId { get; init; }
    public string? HtmlTemplate { get; init; }
    public string? ContentPath { get; init; }
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