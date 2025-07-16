using System.Collections.Concurrent;

namespace WebUI.Core.Communication;

public class IpcTransport
{
    private readonly ConcurrentDictionary<string, Action<string>> _handlers = new();
    private readonly string _panelId;

    public IpcTransport(string panelId)
    {
        _panelId = panelId;
    }

    public void Send(string type, object? payload)
    {
        Console.WriteLine($"[IpcTransport] Send called - type: {type}, handlers registered: {_handlers.Count}");
        
        // For local routing, send payload directly without wrapping
        // Future: When cross-process, wrap with metadata
        var payloadJson = payload != null ? System.Text.Json.JsonSerializer.Serialize(payload) : "{}";
        RouteMessage(type, payloadJson);
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
        Console.WriteLine($"[IpcTransport] RouteMessage - looking for handlers starting with: {type}:");
        
        // Route to all handlers for this message type
        foreach (var kvp in _handlers)
        {
            Console.WriteLine($"[IpcTransport] Handler registered: {kvp.Key}");
            if (kvp.Key.StartsWith($"{type}:"))
            {
                Console.WriteLine($"[IpcTransport] Found matching handler: {kvp.Key}");
                try
                {
                    kvp.Value(payload);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[IpcTransport] Handler error: {ex.Message}");
                }
            }
        }
    }
}