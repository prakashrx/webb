namespace WebUI.Core.Panels;

/// <summary>
/// Configuration options for creating a panel
/// </summary>
public class PanelOptions
{
    /// <summary>
    /// Unique identifier for the panel
    /// </summary>
    public required string Id { get; set; }
    
    /// <summary>
    /// Window title
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Window width in pixels
    /// </summary>
    public int Width { get; set; } = 800;
    
    /// <summary>
    /// Window height in pixels
    /// </summary>
    public int Height { get; set; } = 600;
    
    /// <summary>
    /// Whether the window should be frameless
    /// </summary>
    public bool IsFrameless { get; set; }
    
    /// <summary>
    /// Whether the window can be resized
    /// </summary>
    public bool IsResizable { get; set; } = true;
    
    /// <summary>
    /// Whether the window can be maximized
    /// </summary>
    public bool CanMaximize { get; set; } = true;
    
    /// <summary>
    /// Whether the window can be minimized
    /// </summary>
    public bool CanMinimize { get; set; } = true;
    
    /// <summary>
    /// The UI module name (e.g., "workbench", "extension-name")
    /// </summary>
    public required string UiModule { get; set; }
    
    /// <summary>
    /// The specific component to mount from the UI module (optional)
    /// If not specified, defaults to "default"
    /// </summary>
    public string? ComponentName { get; set; }
    
    /// <summary>
    /// Custom HTML template (optional - uses default if not provided)
    /// </summary>
    public string? HtmlTemplate { get; set; }
    
    /// <summary>
    /// Parent panel ID for modal/child windows
    /// </summary>
    public string? ParentPanelId { get; set; }
    
    /// <summary>
    /// Whether this panel should be modal
    /// </summary>
    public bool IsModal { get; set; }
    
    /// <summary>
    /// Base path for static content (HTML, JS, CSS files)
    /// Used for virtual host mapping. If not specified, uses a default location.
    /// </summary>
    public string? ContentPath { get; set; }
}