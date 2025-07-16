using System.Runtime.InteropServices;
using WebUI.Core.Hosting;
using WebUI.Core.Communication;

namespace WebUI.Core.Api;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class HostApiBridge
{
    private readonly string _panelId;
    private readonly IpcRouter _ipcRouter;
    
    private readonly PanelApi _panel;
    private readonly IpcApi _ipc;

    public PanelApi Panel => _panel;
    public IpcApi Ipc => _ipc;

    public HostApiBridge(string panelId, IpcRouter ipcRouter, BrowserWindow? browserWindow = null)
    {
        _panelId = panelId;
        _ipcRouter = ipcRouter;
        _panel = new PanelApi(ipcRouter, browserWindow);
        _ipc = new IpcApi(panelId, ipcRouter);
    }

    public string GetPanelId() => _panelId;
}