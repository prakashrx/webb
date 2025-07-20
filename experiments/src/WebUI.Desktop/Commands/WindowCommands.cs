using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebUI.Commands;

/// <summary>
/// Window management commands
/// </summary>
public class WindowCommands
{
    private readonly Form _form;
    
    public WindowCommands()
    {
        // Get the main form - this is a simple approach for now
        _form = Application.OpenForms[0] ?? throw new InvalidOperationException("No forms are open");
    }
    
    public async Task Minimize()
    {
        await Task.Run(() =>
        {
            _form.BeginInvoke(() => _form.WindowState = FormWindowState.Minimized);
        });
    }
    
    public async Task Maximize()
    {
        await Task.Run(() =>
        {
            _form.BeginInvoke(() => _form.WindowState = FormWindowState.Maximized);
        });
    }
    
    public async Task Restore()
    {
        await Task.Run(() =>
        {
            _form.BeginInvoke(() => _form.WindowState = FormWindowState.Normal);
        });
    }
    
    public async Task<string> GetTitle()
    {
        return await Task.Run(() =>
        {
            string title = "";
            _form.Invoke(() => title = _form.Text);
            return title;
        });
    }
    
    public async Task SetTitle(SetTitleArgs args)
    {
        await Task.Run(() =>
        {
            _form.BeginInvoke(() => _form.Text = args.Title);
        });
    }
    
    public async Task<WindowState> GetState()
    {
        return await Task.Run(() =>
        {
            var state = FormWindowState.Normal;
            _form.Invoke(() => state = _form.WindowState);
            
            return state switch
            {
                FormWindowState.Minimized => new WindowState { IsMinimized = true },
                FormWindowState.Maximized => new WindowState { IsMaximized = true },
                _ => new WindowState { IsNormal = true }
            };
        });
    }
}

public class SetTitleArgs
{
    public string Title { get; set; } = "";
}

public class WindowState
{
    public bool IsMinimized { get; set; }
    public bool IsMaximized { get; set; }
    public bool IsNormal { get; set; }
}