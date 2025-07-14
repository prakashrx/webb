using System.Collections.Concurrent;

namespace WebUI.Core.Communication;

public class IpcTransport
{
    private readonly ConcurrentDictionary<string, Action<string>> _handlers = new();
    private readonly string _extensionId;

    public IpcTransport(string extensionId)
    {
        _extensionId = extensionId;
    }

    public void Send(string type, object? payload)
    {
        var message = new
        {
            Type = type,
            Payload = payload,
            From = _extensionId,
            Timestamp = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        // For now, route locally - will be enhanced for cross-process in future
        RouteMessage(type, System.Text.Json.JsonSerializer.Serialize(message));
    }

    public void RegisterHandler(string type, string handlerId, Action<string> handler)
    {
        _handlers[$"{type}:{handlerId}"] = handler;
    }

    public void UnregisterHandler(string type, string handlerId)
    {
        _handlers.TryRemove($"{type}:{handlerId}", out _);
    }

    private void RouteMessage(string type, string payload)
    {
        // Route to all handlers for this message type
        foreach (var kvp in _handlers)
        {
            if (kvp.Key.StartsWith($"{type}:"))
            {
                try
                {
                    kvp.Value(payload);
                }
                catch (Exception ex)
                {
                    // Log handler errors in future logging system
                }
            }
        }
    }
}