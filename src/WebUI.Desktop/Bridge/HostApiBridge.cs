using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.WinForms;

namespace WebUI.Bridge;

/// <summary>
/// Main COM-visible bridge exposed to JavaScript
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public class HostApiBridge
{
    public CoreApi Core { get; }
    
    public HostApiBridge(WebView2 webView)
    {
        Core = new CoreApi(webView);
    }
}