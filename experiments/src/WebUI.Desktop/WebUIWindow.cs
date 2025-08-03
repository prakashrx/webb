using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace WebUI;

/// <summary>
/// Base window class for WebUI applications with modern features like rounded corners and frameless resize
/// </summary>
public class WebUIWindow : Form
{
    // Hit test constants for resize
    private const int HTLEFT = 10;
    private const int HTRIGHT = 11;
    private const int HTTOP = 12;
    private const int HTTOPLEFT = 13;
    private const int HTTOPRIGHT = 14;
    private const int HTBOTTOM = 15;
    private const int HTBOTTOMLEFT = 16;
    private const int HTBOTTOMRIGHT = 17;
    
    // Message constants
    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int WM_EXITSIZEMOVE = 0x0232;
    
    // DWM APIs for Windows 11 rounded corners
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWCP_ROUND = 2;
    private const int DWMWCP_ROUNDSMALL = 3;
    
    private readonly WebView2 _webView;
    
    // Resize panels
    private Panel top, left, right, bottom, tl, tr, bl, br;
    private const int ResizeHandleSize = 2;
    private const int CornerHandleSize = 16;
    
    public WebView2 WebView => _webView;
    
    public WebUIWindow()
    {
        SetStyle(ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        
        // Create and add WebView2
        _webView = new WebView2();
        _webView.Dock = DockStyle.Fill;
        _webView.Margin = Padding.Empty;
        _webView.Padding = Padding.Empty;
        
        Controls.Add(_webView);
        
        // Initialize WebView2
        InitializeAsync();
    }
    
    private async void InitializeAsync()
    {
        await _webView.EnsureCoreWebView2Async();
        
        // Enable non-client region support for draggable regions
        _webView.CoreWebView2.Settings.IsNonClientRegionSupportEnabled = true;
    }
    
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        
        // Enable rounded corners on Windows 11
        if (Environment.OSVersion.Version.Build >= 22000)
        {
            var preference = DWMWCP_ROUND;
            DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
        }
        
        // Set up resize panels for frameless windows
        if (FormBorderStyle == FormBorderStyle.None)
        {
            if (top == null) // Only set up once
            {
                SetupResizePanels();
            }
        }
    }
    
    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle &= ~0x02000000; // Remove WS_EX_COMPOSITED
            cp.ExStyle &= ~0x00020000; // Remove WS_EX_CLIENTEDGE
            return cp;
        }
    }
    
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_EXITSIZEMOVE)
        {
            // Force full refresh after resize completes
            Refresh();
            _webView?.Refresh();
        }
        
        base.WndProc(ref m);
    }
    
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (FormBorderStyle == FormBorderStyle.None && top != null)
        {
            PositionResizePanels();
        }
    }
    
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (FormBorderStyle == FormBorderStyle.None && top != null)
        {
            PositionResizePanels();
            // Ensure panels are on top
            foreach (var panel in new[] { top, left, right, bottom, tl, tr, bl, br })
            {
                panel.BringToFront();
            }
        }
    }
    
    
    // Win32 API imports for resize
    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();
    
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    
    // Custom invisible panel that truly has no visual presence
    private class InvisibleHitPanel : Panel
    {
        public InvisibleHitPanel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, false);
            SetStyle(ControlStyles.ResizeRedraw, false);
            BackColor = Color.Transparent;
            TabStop = false;
        }
        
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Do nothing - no painting at all
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            // Do nothing - no painting at all
        }
    }
    
    private void SetupResizePanels()
    {
        // Create edge panels
        top = MakeResizePanel(Cursors.SizeNS, HTTOP);
        left = MakeResizePanel(Cursors.SizeWE, HTLEFT);
        right = MakeResizePanel(Cursors.SizeWE, HTRIGHT);
        bottom = MakeResizePanel(Cursors.SizeNS, HTBOTTOM);
        
        // Create corner panels
        tl = MakeResizePanel(Cursors.SizeNWSE, HTTOPLEFT);
        tr = MakeResizePanel(Cursors.SizeNESW, HTTOPRIGHT);
        bl = MakeResizePanel(Cursors.SizeNESW, HTBOTTOMLEFT);
        br = MakeResizePanel(Cursors.SizeNWSE, HTBOTTOMRIGHT);
        
        // Add to form (on top of WebView)
        Controls.AddRange(new Control[] { top, left, right, bottom, tl, tr, bl, br });
        
        // Position panels
        PositionResizePanels();
    }
    
    private Panel MakeResizePanel(Cursor cursor, int hitTest)
    {
        var panel = new InvisibleHitPanel
        {
            Cursor = cursor,
            Tag = hitTest
        };
        
        panel.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)hitTest, IntPtr.Zero);
            }
        };
        
        return panel;
    }
    
    private void PositionResizePanels()
    {
        if (top == null) return;
        
        var w = ClientSize.Width;
        var h = ClientSize.Height;
        
        top.SetBounds(ResizeHandleSize, 0, w - ResizeHandleSize * 2, ResizeHandleSize);
        bottom.SetBounds(ResizeHandleSize, h - ResizeHandleSize, w - ResizeHandleSize * 2, ResizeHandleSize);
        left.SetBounds(0, ResizeHandleSize, ResizeHandleSize, h - ResizeHandleSize * 2);
        right.SetBounds(w - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize, h - ResizeHandleSize * 2);
        
        tl.SetBounds(0, 0, ResizeHandleSize, ResizeHandleSize);
        tr.SetBounds(w - ResizeHandleSize, 0, ResizeHandleSize, ResizeHandleSize);
        bl.SetBounds(0, h - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);
        br.SetBounds(w - ResizeHandleSize, h - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);
        
        // Ensure all panels are on top of WebView
        foreach (var panel in new[] { top, left, right, bottom, tl, tr, bl, br })
        {
            panel.BringToFront();
        }
    }
}