using System.Runtime.InteropServices;
using WebUI.Core.Communication;
using WebUI.Core.Panels;

namespace WebUI.Core.Api;

/// <summary>
/// Bridge that aggregates all WebUI APIs for JavaScript access.
/// This is exposed via window.chrome.webview.hostObjects.api
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class HostApiBridge
{
    public PanelApi Panel { get; }
    public MessageApi Message { get; }
    
    public HostApiBridge(IPanel panel, MessageBus messageBus)
    {
        Panel = new PanelApi(panel);
        Message = new MessageApi(messageBus);
    }
}