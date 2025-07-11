namespace WebUI.Shared.Contracts;

/// <summary>
/// Extension context providing access to WebUI Platform APIs
/// </summary>
public interface IExtensionContext
{
    /// <summary>
    /// Extension information
    /// </summary>
    IExtension Extension { get; }

    /// <summary>
    /// Commands API for registering and executing commands
    /// </summary>
    ICommandsApi Commands { get; }

    /// <summary>
    /// Events API for subscribing to and emitting events
    /// </summary>
    IEventsApi Events { get; }

    /// <summary>
    /// Window API for creating and managing panels
    /// </summary>
    IWindowApi Window { get; }

    /// <summary>
    /// Services API for accessing platform services
    /// </summary>
    IServicesApi Services { get; }
} 