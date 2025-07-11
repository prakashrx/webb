namespace WebUI.Shared.Contracts;

/// <summary>
/// Events API for subscribing to and emitting events
/// </summary>
public interface IEventsApi
{
    /// <summary>
    /// Subscribe to an event
    /// </summary>
    /// <param name="eventName">Event name</param>
    /// <param name="handler">Event handler</param>
    /// <returns>Subscription identifier for unsubscribing</returns>
    string Subscribe(string eventName, Action<object?> handler);

    /// <summary>
    /// Subscribe to an event with async handler
    /// </summary>
    /// <param name="eventName">Event name</param>
    /// <param name="handler">Async event handler</param>
    /// <returns>Subscription identifier for unsubscribing</returns>
    string Subscribe(string eventName, Func<object?, Task> handler);

    /// <summary>
    /// Unsubscribe from an event
    /// </summary>
    /// <param name="subscriptionId">Subscription identifier</param>
    void Unsubscribe(string subscriptionId);

    /// <summary>
    /// Emit an event
    /// </summary>
    /// <param name="eventName">Event name</param>
    /// <param name="data">Event data</param>
    Task EmitAsync(string eventName, object? data = null);

    /// <summary>
    /// Get all active event subscriptions
    /// </summary>
    IEnumerable<string> GetActiveSubscriptions();
} 