using System.Collections.Concurrent;
using System.Text.Json;

namespace WebUI.Core.Communication;

/// <summary>
/// High-level message routing built on top of IMessageChannel.
/// Provides intuitive message-based communication for panels.
/// </summary>
public class MessageBus : IDisposable
{
    private readonly IMessageChannel _channel;
    private readonly string _processId;
    private readonly string _panelId;
    private readonly ConcurrentDictionary<string, List<Func<object?, Task>>> _handlers = new();
    private readonly List<IDisposable> _subscriptions = new();
    private readonly object _handlersLock = new();
    private bool _disposed;
    
    public string Address => $"{_processId}.{_panelId}";
    
    public MessageBus(IMessageChannel channel, string processId, string panelId)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _processId = processId ?? throw new ArgumentNullException(nameof(processId));
        _panelId = panelId ?? throw new ArgumentNullException(nameof(panelId));
        
        // Subscribe to messages for this panel
        var patterns = new[]
        {
            Address,                    // Direct messages to this panel
            $"{_processId}.{_panelId}.*", // Messages to this panel with sub-routing
            $"*.{_panelId}",           // Messages to this panel from any process
            "*"                        // Broadcast messages
        };
        
        foreach (var pattern in patterns)
        {
            var subscription = _channel.Subscribe(pattern, HandleChannelMessage);
            _subscriptions.Add(subscription);
        }
    }
    
    /// <summary>
    /// Send a message without expecting a response
    /// </summary>
    public async Task SendAsync(string type, object? data = null, string? target = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(MessageBus));
        
        var payload = data != null 
            ? JsonSerializer.SerializeToUtf8Bytes(data) 
            : Array.Empty<byte>();
            
        var message = ChannelMessage.Create(
            source: Address,
            target: target,
            type: type,
            payload: payload
        );
        
        await _channel.SendAsync(message);
    }
    
    /// <summary>
    /// Broadcast a message to all panels
    /// </summary>
    public Task BroadcastAsync(string type, object? data = null)
    {
        return SendAsync(type, data, "*");
    }
    
    /// <summary>
    /// Subscribe to a message type
    /// </summary>
    public void On<T>(string type, Func<T?, Task> handler)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(MessageBus));
        
        lock (_handlersLock)
        {
            if (!_handlers.TryGetValue(type, out var handlers))
            {
                handlers = new List<Func<object?, Task>>();
                _handlers[type] = handlers;
            }
            
            handlers.Add(async (data) =>
            {
                T? typedData = default;
                if (data is JsonElement element)
                {
                    typedData = JsonSerializer.Deserialize<T>(element.GetRawText());
                }
                else if (data != null)
                {
                    // Convert if needed
                    var json = JsonSerializer.Serialize(data);
                    typedData = JsonSerializer.Deserialize<T>(json);
                }
                
                await handler(typedData);
            });
        }
        
        Console.WriteLine($"[MessageBus] {Address} subscribed to '{type}'");
    }
    
    /// <summary>
    /// Subscribe to a message type with dynamic data
    /// </summary>
    public void On(string type, Func<dynamic?, Task> handler)
    {
        On<object>(type, async (data) => await handler(data));
    }
    
    /// <summary>
    /// Unsubscribe from a message type
    /// </summary>
    public void Off(string type)
    {
        lock (_handlersLock)
        {
            _handlers.TryRemove(type, out _);
        }
    }
    
    private async Task HandleChannelMessage(ChannelMessage message)
    {
        // Check if this message is for us
        if (!ShouldHandleMessage(message)) return;
        
        Console.WriteLine($"[MessageBus] {Address} handling message - Type: {message.Type}");
        
        List<Func<object?, Task>>? handlers = null;
        
        lock (_handlersLock)
        {
            // Look for handlers for this message type
            if (_handlers.TryGetValue(message.Type, out var typeHandlers))
            {
                handlers = typeHandlers.ToList();
            }
            
            // Also check for wildcard handlers
            if (_handlers.TryGetValue("*", out var wildcardHandlers))
            {
                handlers ??= new List<Func<object?, Task>>();
                handlers.AddRange(wildcardHandlers);
            }
        }
        
        if (handlers != null && handlers.Count > 0)
        {
            // Deserialize payload once
            object? data = null;
            if (message.Payload.Length > 0)
            {
                try
                {
                    data = JsonSerializer.Deserialize<object>(message.Payload);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MessageBus] Failed to deserialize payload: {ex.Message}");
                }
            }
            
            // Execute all handlers
            var tasks = handlers.Select(async handler =>
            {
                try
                {
                    await handler(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MessageBus] Handler error: {ex.Message}");
                }
            });
            
            await Task.WhenAll(tasks);
        }
    }
    
    private bool ShouldHandleMessage(ChannelMessage message)
    {
        // Handle broadcast messages
        if (message.Target == "*" || string.IsNullOrEmpty(message.Target))
        {
            return true;
        }
        
        // Handle messages targeted at us
        if (message.Target == Address || 
            message.Target == _panelId ||
            message.Target?.StartsWith($"{Address}.") == true)
        {
            return true;
        }
        
        // Don't handle messages for other targets
        return false;
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        // Unsubscribe from channel
        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }
        _subscriptions.Clear();
        
        // Clear handlers
        lock (_handlersLock)
        {
            _handlers.Clear();
        }
    }
}