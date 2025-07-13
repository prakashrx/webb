using System.Runtime.InteropServices;
using WebUI.Core.Windows;

namespace WebUI.Core.Api;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class HostApiBridge
{
    private readonly string _extensionId;
    private readonly IpcTransport _ipcTransport;

    public PanelApi Panel { get; }
    public IpcApi Ipc { get; }

    public HostApiBridge(string extensionId, IpcTransport ipcTransport, BrowserWindow? browserWindow = null)
    {
        _extensionId = extensionId;
        _ipcTransport = ipcTransport;
        Panel = new PanelApi(extensionId, ipcTransport, browserWindow);
        Ipc = new IpcApi(extensionId, ipcTransport);
    }

    public string GetExtensionId() => _extensionId;
}