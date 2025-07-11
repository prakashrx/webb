namespace WebUI.Shared.Contracts;

/// <summary>
/// Services API for accessing platform services
/// </summary>
public interface IServicesApi
{
    /// <summary>
    /// Get a platform service by type
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <returns>Service instance or null if not found</returns>
    T? GetService<T>() where T : class;

    /// <summary>
    /// Get a platform service by name
    /// </summary>
    /// <param name="serviceName">Service name</param>
    /// <returns>Service instance or null if not found</returns>
    object? GetService(string serviceName);

    /// <summary>
    /// Register a service with the platform
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <param name="service">Service instance</param>
    void RegisterService<T>(T service) where T : class;

    /// <summary>
    /// Register a service with the platform
    /// </summary>
    /// <param name="serviceName">Service name</param>
    /// <param name="service">Service instance</param>
    void RegisterService(string serviceName, object service);

    /// <summary>
    /// Unregister a service
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    void UnregisterService<T>() where T : class;

    /// <summary>
    /// Unregister a service
    /// </summary>
    /// <param name="serviceName">Service name</param>
    void UnregisterService(string serviceName);

    /// <summary>
    /// Get all registered service names
    /// </summary>
    IEnumerable<string> GetRegisteredServices();
} 