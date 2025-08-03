using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WebUI;

/// <summary>
/// A reusable control that provides resize functionality for frameless windows.
/// Simply add this to any frameless form to enable resizing.
/// </summary>
public class FramelessResizeBorder : Control
{
    #region Win32 Constants and Imports
    
    // Hit test constants
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
    
    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();
    
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    
    #endregion
    
    private readonly Form _parentForm;
    private readonly int _edgeSize;
    private readonly int _cornerSize;
    
    /// <summary>
    /// Creates a new resize border for a frameless form
    /// </summary>
    /// <param name="parentForm">The form to add resize functionality to</param>
    /// <param name="edgeSize">Size of edge resize handles (default 2)</param>
    /// <param name="cornerSize">Size of corner resize handles (default 12)</param>
    public FramelessResizeBorder(Form parentForm, int edgeSize = 2, int cornerSize = 12)
    {
        _parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
        _edgeSize = edgeSize;
        _cornerSize = cornerSize;
        
        // Make this control transparent and non-interactive
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        SetStyle(ControlStyles.Opaque, false);
        SetStyle(ControlStyles.ResizeRedraw, false);
        BackColor = Color.Transparent;
        TabStop = false;
        
        // Dock to fill the entire form
        Dock = DockStyle.Fill;
        
        // Subscribe to parent form events
        _parentForm.Resize += (s, e) => UpdateRegion();
        
        // Set initial region
        UpdateRegion();
    }
    
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Do nothing - completely transparent
    }
    
    protected override void OnPaint(PaintEventArgs e)
    {
        // Do nothing - completely transparent
    }
    
    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateRegion();
    }
    
    private void UpdateRegion()
    {
        // Create a region that covers only the edges, cutting out the center
        using (var path = new System.Drawing.Drawing2D.GraphicsPath())
        {
            // Add outer rectangle
            path.AddRectangle(new Rectangle(0, 0, Width, Height));
            
            // Cut out inner rectangle, leaving only edge borders
            var innerRect = new Rectangle(
                _edgeSize, 
                _edgeSize, 
                Width - (_edgeSize * 2), 
                Height - (_edgeSize * 2)
            );
            
            // Only cut if there's actually space to cut
            if (innerRect.Width > 0 && innerRect.Height > 0)
            {
                path.AddRectangle(innerRect);
            }
            
            // Create region with the hole
            Region = new Region(path);
        }
    }
    
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        
        if (_parentForm.WindowState == FormWindowState.Maximized)
            return;
        
        // Determine which cursor to show based on position
        var hitTest = GetHitTest(e.Location);
        Cursor = GetCursorForHitTest(hitTest);
    }
    
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        
        if (e.Button == MouseButtons.Left && _parentForm.WindowState != FormWindowState.Maximized)
        {
            var hitTest = GetHitTest(e.Location);
            if (hitTest != 0)
            {
                ReleaseCapture();
                SendMessage(_parentForm.Handle, WM_NCLBUTTONDOWN, (IntPtr)hitTest, IntPtr.Zero);
            }
        }
    }
    
    private int GetHitTest(Point point)
    {
        var x = point.X;
        var y = point.Y;
        var w = Width;
        var h = Height;
        
        // Check corners first (larger hit area)
        if (x < _cornerSize && y < _cornerSize) return HTTOPLEFT;
        if (x >= w - _cornerSize && y < _cornerSize) return HTTOPRIGHT;
        if (x < _cornerSize && y >= h - _cornerSize) return HTBOTTOMLEFT;
        if (x >= w - _cornerSize && y >= h - _cornerSize) return HTBOTTOMRIGHT;
        
        // Check edges (thinner hit area)
        if (x < _edgeSize) return HTLEFT;
        if (x >= w - _edgeSize) return HTRIGHT;
        if (y < _edgeSize) return HTTOP;
        if (y >= h - _edgeSize) return HTBOTTOM;
        
        return 0; // Not on a resize border
    }
    
    private Cursor GetCursorForHitTest(int hitTest)
    {
        return hitTest switch
        {
            HTLEFT or HTRIGHT => Cursors.SizeWE,
            HTTOP or HTBOTTOM => Cursors.SizeNS,
            HTTOPLEFT or HTBOTTOMRIGHT => Cursors.SizeNWSE,
            HTTOPRIGHT or HTBOTTOMLEFT => Cursors.SizeNESW,
            _ => Cursors.Default
        };
    }
    
    /// <summary>
    /// Static helper method to easily add resize functionality to any frameless form
    /// </summary>
    public static FramelessResizeBorder AddTo(Form form, int edgeSize = 2, int cornerSize = 12)
    {
        var resizeBorder = new FramelessResizeBorder(form, edgeSize, cornerSize);
        form.Controls.Add(resizeBorder);
        resizeBorder.BringToFront();
        return resizeBorder;
    }
}