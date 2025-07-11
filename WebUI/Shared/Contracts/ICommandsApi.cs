namespace WebUI.Shared.Contracts;

/// <summary>
/// Commands API for registering and executing commands
/// </summary>
public interface ICommandsApi
{
    /// <summary>
    /// Register a command with the platform
    /// </summary>
    /// <param name="id">Command identifier</param>
    /// <param name="handler">Command handler function</param>
    void RegisterCommand(string id, Func<object?, Task> handler);

    /// <summary>
    /// Register a command with the platform
    /// </summary>
    /// <param name="id">Command identifier</param>
    /// <param name="handler">Command handler action</param>
    void RegisterCommand(string id, Action<object?> handler);

    /// <summary>
    /// Execute a command
    /// </summary>
    /// <param name="id">Command identifier</param>
    /// <param name="args">Command arguments</param>
    Task ExecuteCommandAsync(string id, object? args = null);

    /// <summary>
    /// Unregister a command
    /// </summary>
    /// <param name="id">Command identifier</param>
    void UnregisterCommand(string id);

    /// <summary>
    /// Get all registered commands
    /// </summary>
    IEnumerable<string> GetRegisteredCommands();
} 