using WebUI.Core.Hosting;

namespace WebUI.Core.Panels;

/// <summary>
/// Represents a panel that combines a browser window with WebUI API integration
/// </summary>
public interface IPanel : IDisposable
{
    /// <summary>
    /// Unique identifier for this panel
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// The underlying browser window
    /// </summary>
    BrowserWindow Window { get; }
    
    /// <summary>
    /// Panel configuration options
    /// </summary>
    PanelOptions Options { get; }
    
    /// <summary>
    /// Whether the panel has been initialized
    /// </summary>
    bool IsInitialized { get; }
    
    /// <summary>
    /// Initialize the panel asynchronously
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// Show the panel
    /// </summary>
    void Show();
    
    /// <summary>
    /// Hide the panel
    /// </summary>
    void Hide();
    
    /// <summary>
    /// Close the panel
    /// </summary>
    void Close();
    
    /// <summary>
    /// Send a message to the panel's JavaScript context
    /// </summary>
    Task SendMessageAsync(string type, object? data = null);
    
    /// <summary>
    /// Event raised when the panel is closed
    /// </summary>
    event EventHandler? Closed;
    
    /// <summary>
    /// Event raised when a message is received from JavaScript
    /// </summary>
    event EventHandler<PanelMessage>? MessageReceived;
}

/// <summary>
/// Message received from a panel's JavaScript context
/// </summary>
public class PanelMessage
{
    public required string Type { get; set; }
    public object? Data { get; set; }
    public string PanelId { get; set; } = string.Empty;
}