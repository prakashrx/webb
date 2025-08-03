namespace WebUI.Core.Communication;

/// <summary>
/// Abstraction for message transport mechanisms.
/// Implementations can provide in-process, named pipe, TCP, or other transport methods.
/// </summary>
public interface IMessageChannel : IDisposable
{
    /// <summary>
    /// Unique identifier for this channel instance
    /// </summary>
    string ChannelId { get; }
    
    /// <summary>
    /// Send a message through the channel
    /// </summary>
    Task SendAsync(ChannelMessage message);
    
    /// <summary>
    /// Subscribe to messages for a specific address
    /// </summary>
    /// <param name="address">The address to receive messages for (e.g., "main.settings")</param>
    /// <param name="handler">Handler to invoke when message is received</param>
    /// <returns>Disposable subscription</returns>
    IDisposable Subscribe(string address, Func<ChannelMessage, Task> handler);
    
    /// <summary>
    /// Raised when a message is received
    /// </summary>
    event EventHandler<ChannelMessage>? MessageReceived;
    
    /// <summary>
    /// Raised when an error occurs
    /// </summary>
    event EventHandler<Exception>? ErrorOccurred;
}

/// <summary>
/// Message structure for channel communication
/// </summary>
public record ChannelMessage(
    string Id,
    string Source,      // "process.panel" format
    string? Target,     // null for broadcast, "process.panel" for specific target, "*" for all
    string Type,
    byte[] Payload,
    Dictionary<string, string>? Headers = null
)
{
    /// <summary>
    /// Creates a new message with a unique ID
    /// </summary>
    public static ChannelMessage Create(string source, string? target, string type, byte[] payload, Dictionary<string, string>? headers = null)
    {
        return new ChannelMessage(
            Guid.NewGuid().ToString(),
            source,
            target,
            type,
            payload,
            headers
        );
    }
}