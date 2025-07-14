using System.Windows.Forms;
using WebUI.Core.Windows;

namespace WebUI.Workbench;

public partial class WorkbenchEntry : Form
{
    private BrowserWindow? _browserWindow;

    public WorkbenchEntry()
    {
        InitializeComponent();
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            // Hide the main form (we just use it to manage app lifecycle)
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
            
            // Create a simple browser window demo with frameless window
            _browserWindow = new BrowserWindow("WebUI Platform", 1200, 40, resizable: true, devTools: true, frameless: true);
            
            // Add the new WebUI API bridge
            await _browserWindow.AddWebUIApiAsync("workbench");
            
            // Load the core main toolbar extension
            await _browserWindow.LoadHtmlAsync("extension://core/main-toolbar");
            
            // Handle messages from JavaScript
            _browserWindow.MessageReceived += (sender, message) =>
            {
                Console.WriteLine($"Received message: {message}");
            };
            
            // Handle browser window closing
            _browserWindow.Closed += (sender, e) =>
            {
                // Close the application when the browser window closes
                Application.Exit();
            };
            
            // Show the browser window
            _browserWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        // 
        // WorkbenchEntry
        // 
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(284, 261);
        this.Name = "WorkbenchEntry";
        this.Text = "WebUI Workbench";
        this.ResumeLayout(false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _browserWindow?.Dispose();
        }
        base.Dispose(disposing);
    }
} 