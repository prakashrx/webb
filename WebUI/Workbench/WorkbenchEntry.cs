using System.Windows.Forms;
using WebUI.Core.Extensions;

namespace WebUI.Workbench;

public partial class WorkbenchEntry : Form
{
    private ExtensionHost? _extensionHost;

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
            
            // Create an extension host with a frameless window for the main toolbar
            _extensionHost = ExtensionHostFactory.CreateFrameless("WebUI Platform", 1200, 40);
            
            // Load the core main toolbar extension
            await _extensionHost.LoadExtensionAsync("extension://core/main-toolbar");
            
            // Handle messages from JavaScript
            _extensionHost.Window.MessageReceived += (sender, message) =>
            {
                Console.WriteLine($"Received message: {message}");
            };
            
            // Handle browser window closing
            _extensionHost.Window.Closed += (sender, e) =>
            {
                // Close the application when the browser window closes
                Application.Exit();
            };
            
            // Show the browser window
            _extensionHost.Window.Show();
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
            _extensionHost?.Dispose();
        }
        base.Dispose(disposing);
    }
} 