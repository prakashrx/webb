using System.Collections.Concurrent;
using System.Text.Json;

namespace WebUI.Core.Communication;

/// <summary>
/// High-level message router built on top of IpcTransport.
/// Provides intuitive message-based communication between processes.
/// </summary>
public class IpcRouter : IDisposable
{
    private readonly IpcTransport _transport;
    private readonly ConcurrentDictionary<string, List<MessageHandler>> _handlers = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingResponses = new();
    private readonly string _routerId;
    private bool _isDisposed;

    public IpcRouter(IpcTransport transport, string? routerId = null)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _routerId = routerId ?? Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Send a message without expecting a response
    /// </summary>
    public Task SendAsync(string messageType, object? data = null)
    {
        // Just send directly through transport without extra wrapping
        _transport.Send(messageType, data);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Send a message and wait for a response
    /// </summary>
    public async Task<T?> SendAsync<T>(string messageType, object? data = null, TimeSpan? timeout = null)
    {
        var messageId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<string>();
        _pendingResponses[messageId] = tcs;

        var message = new RouterMessage
        {
            Id = messageId,
            Type = messageType,
            From = _routerId,
            Payload = JsonSerializer.Serialize(data),
            ExpectsResponse = true
        };

        var json = JsonSerializer.Serialize(message);
        
        // Set up timeout if specified
        using var cts = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : null;
        if (cts != null)
        {
            cts.Token.Register(() =>
            {
                _pendingResponses.TryRemove(messageId, out _);
                tcs.TrySetCanceled();
            });
        }

        // Send the message
        _transport.Send("ipc.router", json);

        try
        {
            var response = await tcs.Task;
            return JsonSerializer.Deserialize<T>(response);
        }
        finally
        {
            _pendingResponses.TryRemove(messageId, out _);
        }
    }

    /// <summary>
    /// Subscribe to a message type
    /// </summary>
    public void On(string messageType, Func<dynamic, Task> handler)
    {
        var wrapper = new MessageHandler(async (payload) =>
        {
            var data = string.IsNullOrEmpty(payload) 
                ? null 
                : JsonSerializer.Deserialize<dynamic>(payload);
            await handler(data);
            return null;
        });

        AddHandler(messageType, wrapper);
    }

    /// <summary>
    /// Subscribe to a message type with typed data
    /// </summary>
    public void On<T>(string messageType, Func<T, Task> handler)
    {
        var wrapper = new MessageHandler(async (payload) =>
        {
            var data = string.IsNullOrEmpty(payload) 
                ? default(T) 
                : JsonSerializer.Deserialize<T>(payload);
            await handler(data!);
            return null;
        });

        AddHandler(messageType, wrapper);
    }

    /// <summary>
    /// Subscribe to a message type and return a response
    /// </summary>
    public void On<TRequest, TResponse>(string messageType, Func<TRequest, Task<TResponse>> handler)
    {
        var wrapper = new MessageHandler(async (payload) =>
        {
            var data = string.IsNullOrEmpty(payload) 
                ? default(TRequest) 
                : JsonSerializer.Deserialize<TRequest>(payload);
            var response = await handler(data!);
            return JsonSerializer.Serialize(response);
        });

        AddHandler(messageType, wrapper);
    }

    /// <summary>
    /// Unsubscribe from a message type
    /// </summary>
    public void Off(string messageType)
    {
        _handlers.TryRemove(messageType, out _);
    }

    /// <summary>
    /// Broadcast a message to all connected routers
    /// </summary>
    public Task BroadcastAsync(string messageType, object? data = null)
    {
        var message = new RouterMessage
        {
            Id = Guid.NewGuid().ToString(),
            Type = messageType,
            From = _routerId,
            To = "broadcast",
            Payload = JsonSerializer.Serialize(data),
            ExpectsResponse = false
        };

        var json = JsonSerializer.Serialize(message);
        _transport.Send("ipc.router.broadcast", json);
        
        return Task.CompletedTask;
    }


    private void AddHandler(string messageType, MessageHandler handler)
    {
        _handlers.AddOrUpdate(messageType,
            new List<MessageHandler> { handler },
            (_, list) =>
            {
                list.Add(handler);
                return list;
            });
            
        // Register with transport for this specific message type
        _transport.RegisterHandler(messageType, $"{_routerId}:{messageType}", async (payload) => 
        {
            Console.WriteLine($"[IpcRouter] Received message type: {messageType}, payload: {payload}");
            
            if (_handlers.TryGetValue(messageType, out var handlers))
            {
                Console.WriteLine($"[IpcRouter] Found {handlers.Count} handlers for message type: {messageType}");
                foreach (var h in handlers)
                {
                    try
                    {
                        await h.HandleAsync(payload);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[IpcRouter] Error in handler: {ex.Message}");
                    }
                }
            }
        });
    }
    

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Cancel all pending responses
        foreach (var pending in _pendingResponses.Values)
        {
            pending.TrySetCanceled();
        }

        _pendingResponses.Clear();
        _handlers.Clear();
    }

    private class MessageHandler
    {
        private readonly Func<string, Task<string?>> _handler;

        public MessageHandler(Func<string, Task<string?>> handler)
        {
            _handler = handler;
        }

        public Task<string?> HandleAsync(string payload) => _handler(payload);
    }

    private class RouterMessage
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? To { get; set; }
        public string From { get; set; } = string.Empty;
        public string? Payload { get; set; }
        public bool ExpectsResponse { get; set; }
        public bool IsResponse { get; set; }
        public string? ResponseTo { get; set; }
    }
}