using System.Runtime.InteropServices;
using WebUI.Core.Communication;

namespace WebUI.Core.Api;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class IpcApi
{
    private readonly string _extensionId;
    private readonly IpcTransport _ipc;

    public IpcApi(string extensionId, IpcTransport ipc)
    {
        _extensionId = extensionId;
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

        _ipc.Send(type, parsedPayload);
    }

    public string On(string type, string handlerName)
    {
        var handlerId = Guid.NewGuid().ToString();
        _ipc.RegisterHandler(type, handlerId, payload =>
        {
            // Call JavaScript handler by name
            InvokeJavaScriptHandler(handlerName, payload);
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

        _ipc.Send($"broadcast.{type}", parsedPayload);
    }

    private void InvokeJavaScriptHandler(string handlerName, string payload)
    {
        // This will be called via WebView2's ExecuteScriptAsync
        // For now, we'll store this for the WebView2 integration
        Console.WriteLine($"Would invoke JS handler: {handlerName} with {payload}");
    }
}