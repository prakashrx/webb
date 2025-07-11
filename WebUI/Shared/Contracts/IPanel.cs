namespace WebUI.Shared.Contracts;

/// <summary>
/// Panel interface for individual panel operations
/// </summary>
public interface IPanel
{
    /// <summary>
    /// Panel identifier
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Panel title
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Panel content URL
    /// </summary>
    string Url { get; set; }

    /// <summary>
    /// Whether the panel is visible
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Navigate to a new URL
    /// </summary>
    /// <param name="url">URL to navigate to</param>
    Task NavigateAsync(string url);

    /// <summary>
    /// Execute JavaScript in the panel
    /// </summary>
    /// <param name="script">JavaScript code</param>
    /// <returns>Script result</returns>
    Task<object?> ExecuteScriptAsync(string script);

    /// <summary>
    /// Send a message to the panel
    /// </summary>
    /// <param name="message">Message to send</param>
    Task SendMessageAsync(object message);

    /// <summary>
    /// Event fired when the panel receives a message
    /// </summary>
    event Action<object>? MessageReceived;

    /// <summary>
    /// Event fired when the panel is closed
    /// </summary>
    event Action? Closed;
} 