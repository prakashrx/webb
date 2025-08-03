using System.Runtime.InteropServices;
using System.Text.Json;
using WebUI.Core.Communication;

namespace WebUI.Core.Api;

/// <summary>
/// COM-visible API for JavaScript to send messages to the C# platform.
/// This API only handles JS → C# communication.
/// C# → JS communication uses WebView2's PostMessage API.
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class MessageApi
{
    private readonly MessageBus _bus;
    
    public MessageApi(MessageBus bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    }
    
    /// <summary>
    /// Send a message to the platform or other panels
    /// </summary>
    public void Send(string type, string payload)
    {
        Console.WriteLine($"[MessageApi] Send - type: {type}, payload: {payload}");
        
        var data = ParsePayload(payload);
        // Fire and forget, but stay on current thread to preserve STA context
        _ = _bus.SendAsync(type, data);
    }
    
    /// <summary>
    /// Send a message to a specific target panel
    /// </summary>
    public void SendTo(string target, string type, string payload)
    {
        Console.WriteLine($"[MessageApi] SendTo - target: {target}, type: {type}, payload: {payload}");
        
        var data = ParsePayload(payload);
        // Fire and forget, but stay on current thread to preserve STA context
        _ = _bus.SendAsync(type, data, target);
    }
    
    /// <summary>
    /// Broadcast a message to all panels
    /// </summary>
    public void Broadcast(string type, string payload)
    {
        Console.WriteLine($"[MessageApi] Broadcast - type: {type}, payload: {payload}");
        
        var data = ParsePayload(payload);
        // Fire and forget, but stay on current thread to preserve STA context
        _ = _bus.BroadcastAsync(type, data);
    }
    
    private object? ParsePayload(string payload)
    {
        if (string.IsNullOrEmpty(payload)) 
            return null;
        
        try
        {
            // Try to parse as JSON
            using var doc = JsonDocument.Parse(payload);
            return doc.RootElement.Clone();
        }
        catch
        {
            // If not valid JSON, return as string
            return payload;
        }
    }
    
    // Note: No On() method - C# to JS communication uses PostMessage instead
}