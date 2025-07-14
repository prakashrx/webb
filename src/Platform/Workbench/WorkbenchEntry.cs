using System.Windows.Forms;
using WebUI.Core.Screens;

namespace WebUI.Workbench;

public partial class WorkbenchEntry : Form
{
    private ScreenManager? _screenManager;
    private IScreen? _mainToolbar;

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
            
            // Create screen manager
            _screenManager = new ScreenManager();
            await _screenManager.InitializeAsync();
            
            // Create main toolbar screen
            _mainToolbar = await _screenManager.CreateScreenAsync(new ScreenOptions
            {
                Id = "main-toolbar",
                Title = "WebUI Platform",
                Width = 1200,
                Height = 40,
                IsFrameless = true,
                IsResizable = false,
                CanMaximize = false,
                CanMinimize = false,
                UiModule = "workbench",
                PanelId = "main-toolbar"
            });
            
            // Handle screen closed event
            _screenManager.ScreenClosed += (sender, e) =>
            {
                if (e.ScreenId == "main-toolbar")
                {
                    // Close the application when the main toolbar closes
                    Application.Exit();
                }
            };
            
            // Handle messages from screens
            _screenManager.MessageReceived += (sender, message) =>
            {
                Console.WriteLine($"Message from {message.ScreenId}: {message.Type}");
                
                // Handle specific message types
                switch (message.Type)
                {
                    case "open-settings":
                        OpenSettingsScreen();
                        break;
                    case "open-workspace":
                        OpenWorkspaceScreen();
                        break;
                }
            };
            
            // Show the main toolbar
            _mainToolbar.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }
    
    private async void OpenSettingsScreen()
    {
        try
        {
            // Check if settings screen already exists
            var settings = _screenManager?.GetScreen("settings");
            if (settings != null)
            {
                settings.Show();
                return;
            }
            
            // Create new settings screen
            settings = await _screenManager!.CreateScreenAsync(new ScreenOptions
            {
                Id = "settings",
                Title = "Settings",
                Width = 800,
                Height = 600,
                UiModule = "workbench",
                PanelId = "settings"
            });
            
            settings.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void OpenWorkspaceScreen()
    {
        try
        {
            // Check if workspace screen already exists
            var workspace = _screenManager?.GetScreen("workspace");
            if (workspace != null)
            {
                workspace.Show();
                return;
            }
            
            // Create new workspace screen
            workspace = await _screenManager!.CreateScreenAsync(new ScreenOptions
            {
                Id = "workspace",
                Title = "Workspace",
                Width = 1400,
                Height = 900,
                UiModule = "workbench",
                PanelId = "workspace"
            });
            
            workspace.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening workspace: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            _screenManager?.Dispose();
        }
        base.Dispose(disposing);
    }
} 