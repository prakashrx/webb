using WebUI.Core.Hosting;

namespace WebUI.Core.Screens;

/// <summary>
/// Represents a screen that combines a browser window with WebUI API integration
/// </summary>
public interface IScreen : IDisposable
{
    /// <summary>
    /// Unique identifier for this screen
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// The underlying browser window
    /// </summary>
    BrowserWindow Window { get; }
    
    /// <summary>
    /// Screen configuration options
    /// </summary>
    ScreenOptions Options { get; }
    
    /// <summary>
    /// Whether the screen has been initialized
    /// </summary>
    bool IsInitialized { get; }
    
    /// <summary>
    /// Initialize the screen asynchronously
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// Show the screen
    /// </summary>
    void Show();
    
    /// <summary>
    /// Hide the screen
    /// </summary>
    void Hide();
    
    /// <summary>
    /// Close the screen
    /// </summary>
    void Close();
    
    /// <summary>
    /// Send a message to the screen's JavaScript context
    /// </summary>
    Task SendMessageAsync(string type, object? data = null);
    
    /// <summary>
    /// Event raised when the screen is closed
    /// </summary>
    event EventHandler? Closed;
    
    /// <summary>
    /// Event raised when a message is received from JavaScript
    /// </summary>
    event EventHandler<ScreenMessage>? MessageReceived;
}

/// <summary>
/// Message received from a screen's JavaScript context
/// </summary>
public class ScreenMessage
{
    public required string Type { get; set; }
    public object? Data { get; set; }
    public string ScreenId { get; set; } = string.Empty;
}