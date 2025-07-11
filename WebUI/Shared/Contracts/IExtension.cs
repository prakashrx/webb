namespace WebUI.Shared.Contracts;

/// <summary>
/// Base interface for all WebUI extensions
/// </summary>
public interface IExtension
{
    /// <summary>
    /// Extension unique identifier
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Extension display name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Extension version
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Activate the extension with the provided context
    /// </summary>
    /// <param name="context">Extension context providing APIs</param>
    Task ActivateAsync(IExtensionContext context);

    /// <summary>
    /// Deactivate the extension and clean up resources
    /// </summary>
    Task DeactivateAsync();
} 