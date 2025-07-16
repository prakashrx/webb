using System.Collections.Concurrent;

namespace WebUI.Core.Communication;

/// <summary>
/// In-process implementation of IMessageChannel for local message routing.
/// This is used when all panels run in the same process.
/// </summary>
public class InProcessChannel : IMessageChannel
{
    // Simple dictionary: target address -> handlers for that address
    private readonly ConcurrentDictionary<string, List<Func<ChannelMessage, Task>>> _handlers = new();
    private readonly object _handlersLock = new();
    private bool _disposed;
    
    public string ChannelId { get; } = Guid.NewGuid().ToString();
    
    public event EventHandler<ChannelMessage>? MessageReceived;
    public event EventHandler<Exception>? ErrorOccurred;
    
    public async Task SendAsync(ChannelMessage message)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(InProcessChannel));
        
        Console.WriteLine($"[InProcessChannel] Sending message - Type: {message.Type}, Source: {message.Source}, Target: {message.Target}");
        
        var handlersToExecute = new List<Func<ChannelMessage, Task>>();
        
        lock (_handlersLock)
        {
            // If it's a broadcast message, send to all handlers
            if (message.Target == "*" || string.IsNullOrEmpty(message.Target))
            {
                foreach (var handlerList in _handlers.Values)
                {
                    handlersToExecute.AddRange(handlerList);
                }
            }
            // Otherwise, send only to the specific target
            else if (_handlers.TryGetValue(message.Target, out var targetHandlers))
            {
                handlersToExecute.AddRange(targetHandlers);
            }
        }
        
        Console.WriteLine($"[InProcessChannel] Found {handlersToExecute.Count} matching handlers");
        
        // Execute handlers
        var tasks = handlersToExecute.Select(async handler =>
        {
            try
            {
                await handler(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InProcessChannel] Handler error: {ex.Message}");
                ErrorOccurred?.Invoke(this, ex);
            }
        });
        
        await Task.WhenAll(tasks);
        
        // Raise event
        MessageReceived?.Invoke(this, message);
    }
    
    public IDisposable Subscribe(string address, Func<ChannelMessage, Task> handler)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(InProcessChannel));
        
        Console.WriteLine($"[InProcessChannel] Subscribing to address: {address}");
        
        lock (_handlersLock)
        {
            if (!_handlers.TryGetValue(address, out var handlers))
            {
                handlers = new List<Func<ChannelMessage, Task>>();
                _handlers[address] = handlers;
            }
            handlers.Add(handler);
        }
        
        // Return subscription that removes handler when disposed
        return new Subscription(() =>
        {
            lock (_handlersLock)
            {
                if (_handlers.TryGetValue(address, out var handlers))
                {
                    handlers.Remove(handler);
                    if (handlers.Count == 0)
                    {
                        _handlers.TryRemove(address, out _);
                    }
                }
            }
        });
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        lock (_handlersLock)
        {
            _handlers.Clear();
        }
    }
    
    private class Subscription : IDisposable
    {
        private readonly Action _onDispose;
        private bool _disposed;
        
        public Subscription(Action onDispose)
        {
            _onDispose = onDispose;
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _onDispose();
        }
    }
}