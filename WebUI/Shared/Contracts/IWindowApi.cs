namespace WebUI.Shared.Contracts;

/// <summary>
/// Window API for creating and managing panels
/// </summary>
public interface IWindowApi
{
    /// <summary>
    /// Create a new panel
    /// </summary>
    /// <param name="id">Panel identifier</param>
    /// <param name="title">Panel title</param>
    /// <param name="url">Panel content URL</param>
    /// <returns>Panel instance</returns>
    Task<IPanel> CreatePanelAsync(string id, string title, string url);

    /// <summary>
    /// Get an existing panel
    /// </summary>
    /// <param name="id">Panel identifier</param>
    /// <returns>Panel instance or null if not found</returns>
    IPanel? GetPanel(string id);

    /// <summary>
    /// Close a panel
    /// </summary>
    /// <param name="id">Panel identifier</param>
    Task ClosePanelAsync(string id);

    /// <summary>
    /// Get all active panels
    /// </summary>
    IEnumerable<IPanel> GetActivePanels();

    /// <summary>
    /// Show a panel
    /// </summary>
    /// <param name="id">Panel identifier</param>
    Task ShowPanelAsync(string id);

    /// <summary>
    /// Hide a panel
    /// </summary>
    /// <param name="id">Panel identifier</param>
    Task HidePanelAsync(string id);
} 