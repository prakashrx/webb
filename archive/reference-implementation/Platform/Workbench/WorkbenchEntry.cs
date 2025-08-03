using System.Windows.Forms;
using WebUI.Core.Panels;
using WebUI.Core.Windows;

namespace WebUI.Workbench;

public partial class WorkbenchEntry : Form
{
    private IPanel? _mainToolbar;
    private WindowManager? _windowManager;

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
            
            // Create WindowManager instance
            _windowManager = new WindowManager();
            
            // Initialize WindowManager
            // UI files are copied to "ui" subdirectory by MSBuild
            var contentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui");
            await _windowManager.InitializeAsync(contentPath);
            
            // Register panel definitions
            RegisterPanelDefinitions();
            
            // Create main toolbar panel
            _mainToolbar = await _windowManager.CreateAsync(new PanelOptions
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
                ComponentName = "main-toolbar"
            });
            
            // Handle main toolbar closing - exit the app
            _mainToolbar.Closed += (sender, e) =>
            {
                Application.Exit();
            };
            
            // Messages are now handled by WindowManager via IPC
            
            // Show the main toolbar
            _mainToolbar.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }
    
    private void RegisterPanelDefinitions()
    {
        // Register all available panels
        _windowManager.RegisterPanel(new PanelOptions
        {
            Id = "settings",
            Title = "Settings",
            Width = 800,
            Height = 600,
            UiModule = "workbench",
            ComponentName = "settings"
        });
        
        _windowManager.RegisterPanel(new PanelOptions
        {
            Id = "workspace",
            Title = "Workspace",
            Width = 1400,
            Height = 900,
            UiModule = "workbench",
            ComponentName = "workspace"
        });
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
            _windowManager?.Dispose();
        }
        base.Dispose(disposing);
    }
} 