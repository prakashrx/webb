using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace WebUI.Framework;

/// <summary>
/// Provides window control functionality for frameless browser windows
/// </summary>
public class WindowControls
{
    private readonly Form _form;
    private Point _lastKnownPosition;

    public WindowControls(Form form)
    {
        _form = form;
        _lastKnownPosition = _form.Location;
    }

    /// <summary>
    /// Minimize the window
    /// </summary>
    public void Minimize()
    {
        _form.Invoke(() => _form.WindowState = FormWindowState.Minimized);
    }

    /// <summary>
    /// Maximize the window
    /// </summary>
    public void Maximize()
    {
        _form.Invoke(() => _form.WindowState = FormWindowState.Maximized);
    }

    /// <summary>
    /// Restore the window
    /// </summary>
    public void Restore()
    {
        _form.Invoke(() => _form.WindowState = FormWindowState.Normal);
    }

    /// <summary>
    /// Close the window
    /// </summary>
    public void Close()
    {
        _form.Invoke(() => _form.Close());
    }

    /// <summary>
    /// Check if the window is maximized
    /// </summary>
    public bool IsMaximized()
    {
        return _form.WindowState == FormWindowState.Maximized;
    }

    /// <summary>
    /// Move the window by the specified delta
    /// </summary>
    public void MoveWindow(int deltaX, int deltaY)
    {
        if (_form.WindowState == FormWindowState.Maximized)
            return; // Can't move maximized window

        _form.Invoke(() =>
        {
            var newLocation = new Point(_form.Location.X + deltaX, _form.Location.Y + deltaY);
            
            // Keep window on screen
            var workingArea = Screen.GetWorkingArea(_form);
            if (newLocation.X < workingArea.Left - (_form.Width - 100))
                newLocation.X = workingArea.Left - (_form.Width - 100);
            if (newLocation.Y < workingArea.Top)
                newLocation.Y = workingArea.Top;
            if (newLocation.X > workingArea.Right - 100)
                newLocation.X = workingArea.Right - 100;
            if (newLocation.Y > workingArea.Bottom - 50)
                newLocation.Y = workingArea.Bottom - 50;

            _form.Location = newLocation;
        });
    }

    /// <summary>
    /// Toggle between maximized and normal state
    /// </summary>
    public void ToggleMaximize()
    {
        if (IsMaximized())
            Restore();
        else
            Maximize();
    }

    /// <summary>
    /// Get the current window state
    /// </summary>
    public string GetWindowState()
    {
        return _form.WindowState switch
        {
            FormWindowState.Minimized => "minimized",
            FormWindowState.Maximized => "maximized",
            FormWindowState.Normal => "normal",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Set the window size
    /// </summary>
    public void SetSize(int width, int height)
    {
        _form.Invoke(() => _form.Size = new Size(width, height));
    }

    /// <summary>
    /// Get the window size
    /// </summary>
    public object GetSize()
    {
        return new { width = _form.Width, height = _form.Height };
    }

    /// <summary>
    /// Get the window position
    /// </summary>
    public object GetPosition()
    {
        return new { x = _form.Location.X, y = _form.Location.Y };
    }
} 