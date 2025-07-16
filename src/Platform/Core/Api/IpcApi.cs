using System.Runtime.InteropServices;
using WebUI.Core.Communication;

namespace WebUI.Core.Api;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class IpcApi
{
    private readonly string _panelId;
    private readonly IpcRouter _ipc;

    public IpcApi(string panelId, IpcRouter ipc)
    {
        _panelId = panelId;
        _ipc = ipc;
    }

    public void Send(string type, string payload)
    {
        object? parsedPayload = null;
        if (!string.IsNullOrEmpty(payload))
        {
            try
            {
                parsedPayload = System.Text.Json.JsonSerializer.Deserialize<object>(payload);
            }
            catch
            {
                // If JSON parsing fails, send as string
                parsedPayload = payload;
            }
        }

        _ipc.SendAsync(type, parsedPayload);
    }

    public string On(string type, string handlerName)
    {
        var handlerId = Guid.NewGuid().ToString();
        _ipc.On(type, async (dynamic payload) =>
        {
            // Call JavaScript handler by name
            await Task.Run(() => InvokeJavaScriptHandler(handlerName, payload?.ToString() ?? string.Empty));
        });
        return handlerId; // Return for cleanup
    }

    public void Broadcast(string type, string payload)
    {
        object? parsedPayload = null;
        if (!string.IsNullOrEmpty(payload))
        {
            try
            {
                parsedPayload = System.Text.Json.JsonSerializer.Deserialize<object>(payload);
            }
            catch
            {
                parsedPayload = payload;
            }
        }

        _ipc.BroadcastAsync(type, parsedPayload);
    }

    private void InvokeJavaScriptHandler(string handlerName, string payload)
    {
        // This will be called via WebView2's ExecuteScriptAsync
        // For now, we'll store this for the WebView2 integration
        Console.WriteLine($"Would invoke JS handler: {handlerName} with {payload}");
    }
}