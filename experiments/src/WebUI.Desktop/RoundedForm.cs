using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel;

namespace WebUI;

/// <summary>
/// A modern form base class that provides rounded corners, frameless window support,
/// and clean rendering without edge artifacts.
/// </summary>
public class RoundedForm : Form
{
    #region Win32 APIs
    
    // DWM APIs for Windows 11 rounded corners
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWCP_DEFAULT = 0;
    private const int DWMWCP_DONOTROUND = 1;
    private const int DWMWCP_ROUND = 2;
    private const int DWMWCP_ROUNDSMALL = 3;
    
    // Message constants
    private const int WM_EXITSIZEMOVE = 0x0232;
    
    #endregion
    
    private FramelessResizeBorder _resizeBorder;
    private bool _enableRoundedCorners = true;
    private bool _enableFramelessResize = true;

    /// <summary>
    /// Gets or sets whether rounded corners are enabled (Windows 11+)
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool EnableRoundedCorners
    {
        get => _enableRoundedCorners;
        set
        {
            _enableRoundedCorners = value;
            if (IsHandleCreated)
                ApplyRoundedCorners();
        }
    }

    /// <summary>
    /// Gets or sets whether frameless windows should have resize functionality
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool EnableFramelessResize
    {
        get => _enableFramelessResize;
        set
        {
            _enableFramelessResize = value;
            if (IsHandleCreated)
                UpdateResizeBorder();
        }
    }
    
    public RoundedForm()
    {
        SetStyle(ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
    }
    
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        
        // Apply rounded corners
        ApplyRoundedCorners();
        
        // Add resize functionality for frameless windows
        UpdateResizeBorder();
    }
    
    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            // Remove window edge styles to prevent artifacts
            cp.ExStyle &= ~0x02000000; // Remove WS_EX_COMPOSITED
            cp.ExStyle &= ~0x00020000; // Remove WS_EX_CLIENTEDGE
            return cp;
        }
    }
    
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_EXITSIZEMOVE)
        {
            // Force full refresh after resize completes to fix any artifacts
            OnResizeEnd();
        }
        
        base.WndProc(ref m);
    }
    
    /// <summary>
    /// Called when resize operation completes. Override to handle resize end.
    /// </summary>
    protected virtual void OnResizeEnd()
    {
        Refresh();
    }
    
    private void ApplyRoundedCorners()
    {
        if (!_enableRoundedCorners || !IsHandleCreated)
            return;
            
        // Enable rounded corners on Windows 11
        if (Environment.OSVersion.Version.Build >= 22000)
        {
            var preference = FormBorderStyle == FormBorderStyle.None ? DWMWCP_ROUND : DWMWCP_DEFAULT;
            DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
        }
    }
    
    private void UpdateResizeBorder()
    {
        if (!IsHandleCreated)
            return;
            
        // Remove existing resize border if any
        if (_resizeBorder != null)
        {
            Controls.Remove(_resizeBorder);
            _resizeBorder.Dispose();
            _resizeBorder = null;
        }
        
        // Add resize border for frameless windows
        if (FormBorderStyle == FormBorderStyle.None && _enableFramelessResize)
        {
            _resizeBorder = FramelessResizeBorder.AddTo(this);
        }
    }

    // Override FormBorderStyle property to detect changes
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new FormBorderStyle FormBorderStyle
    {
        get => base.FormBorderStyle;
        set
        {
            if (base.FormBorderStyle != value)
            {
                base.FormBorderStyle = value;
                
                // Update rounded corners and resize border when border style changes
                if (IsHandleCreated)
                {
                    ApplyRoundedCorners();
                    UpdateResizeBorder();
                }
            }
        }
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _resizeBorder?.Dispose();
        }
        base.Dispose(disposing);
    }
}