using System.Runtime.InteropServices;

namespace WebUI.Framework;

/// <summary>
/// Simple helper for window operations that Windows Forms doesn't provide well
/// </summary>
public static class WindowHelper
{
    #region Win32 API (minimal)

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APPWINDOW = 0x00040000;
    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HT_CAPTION = 0x2;

    #endregion

    /// <summary>
    /// Make window a tool window (doesn't appear in taskbar)
    /// Use this for floating panels, popups, etc.
    /// </summary>
    public static void MakeToolWindow(Form form)
    {
        var exStyle = GetWindowLong(form.Handle, GWL_EXSTYLE);
        exStyle |= WS_EX_TOOLWINDOW;
        exStyle &= ~WS_EX_APPWINDOW;
        SetWindowLong(form.Handle, GWL_EXSTYLE, exStyle);
    }

    /// <summary>
    /// Enable drag to move window by clicking anywhere on it
    /// Perfect for borderless windows
    /// </summary>
    public static void EnableDragMove(Form form)
    {
        form.MouseDown += OnMouseDown;
        
        // Also enable for child controls
        foreach (Control control in form.Controls)
            control.MouseDown += OnMouseDown;

        static void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            
            ReleaseCapture();
            var form = sender as Form ?? ((Control)sender!).FindForm();
            if (form is not null)
                SendMessage(form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
    }

    /// <summary>
    /// Position window at top of desktop (for toolbar-style windows)
    /// </summary>
    public static void PositionAtTopOfDesktop(Form form, int height = 60)
    {
        var screen = Screen.PrimaryScreen.Bounds;
        form.Bounds = new Rectangle(0, 0, screen.Width, height);
    }

    /// <summary>
    /// Setup a form for floating toolbar style
    /// - Borderless, Tool window (no taskbar), Draggable, Always on top
    /// </summary>
    public static void SetupFloatingToolbar(Form form)
    {
        (form.FormBorderStyle, form.TopMost, form.ShowInTaskbar) = 
            (FormBorderStyle.None, true, false);
        
        MakeToolWindow(form);
        EnableDragMove(form);
    }

    /// <summary>
    /// Start dragging the window (use this when WebView2 or other controls capture mouse)
    /// Call this from a MouseDown event handler
    /// </summary>
    public static void StartDragMove(Form form)
    {
        ReleaseCapture();
        SendMessage(form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
    }
} 