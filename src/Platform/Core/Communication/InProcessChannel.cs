using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace WebUI.Core.Communication;

/// <summary>
/// In-process implementation of IMessageChannel for local message routing.
/// This is used when all panels run in the same process.
/// </summary>
public class InProcessChannel : IMessageChannel
{
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
        
        // Get all matching handlers
        var handlers = GetMatchingHandlers(message);
        
        Console.WriteLine($"[InProcessChannel] Found {handlers.Count} matching handlers");
        
        // Execute handlers
        var tasks = handlers.Select(async handler =>
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
    
    public IDisposable Subscribe(string pattern, Func<ChannelMessage, Task> handler)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(InProcessChannel));
        
        Console.WriteLine($"[InProcessChannel] Subscribing to pattern: {pattern}");
        
        lock (_handlersLock)
        {
            if (!_handlers.TryGetValue(pattern, out var handlers))
            {
                handlers = new List<Func<ChannelMessage, Task>>();
                _handlers[pattern] = handlers;
            }
            handlers.Add(handler);
        }
        
        // Return subscription that removes handler when disposed
        return new Subscription(() =>
        {
            lock (_handlersLock)
            {
                if (_handlers.TryGetValue(pattern, out var handlers))
                {
                    handlers.Remove(handler);
                    if (handlers.Count == 0)
                    {
                        _handlers.TryRemove(pattern, out _);
                    }
                }
            }
        });
    }
    
    private List<Func<ChannelMessage, Task>> GetMatchingHandlers(ChannelMessage message)
    {
        var matchingHandlers = new List<Func<ChannelMessage, Task>>();
        
        lock (_handlersLock)
        {
            foreach (var (pattern, handlers) in _handlers)
            {
                if (MatchesPattern(pattern, message))
                {
                    matchingHandlers.AddRange(handlers);
                }
            }
        }
        
        return matchingHandlers;
    }
    
    private bool MatchesPattern(string pattern, ChannelMessage message)
    {
        // Handle broadcast messages
        if (message.Target == "*" || message.Target == null)
        {
            // Broadcast patterns like "*.broadcast.*" or specific type patterns
            if (pattern.Contains("broadcast") || pattern == "*" || pattern == message.Type)
            {
                return true;
            }
        }
        
        // Handle targeted messages
        if (!string.IsNullOrEmpty(message.Target))
        {
            // Check if pattern matches target
            if (MatchesWildcard(pattern, message.Target))
            {
                return true;
            }
            
            // Also match on message type
            if (pattern == message.Type)
            {
                return true;
            }
        }
        
        // Match type patterns
        if (MatchesWildcard(pattern, message.Type))
        {
            return true;
        }
        
        return false;
    }
    
    private bool MatchesWildcard(string pattern, string text)
    {
        if (pattern == "*") return true;
        if (pattern == text) return true;
        
        // Convert wildcard pattern to regex
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        return Regex.IsMatch(text, regexPattern);
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