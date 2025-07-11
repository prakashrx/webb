namespace WebUI.Shared.Models;

/// <summary>
/// Extension contribution points defining what the extension provides
/// </summary>
public sealed class ExtensionContributions
{
    /// <summary>
    /// Commands contributed by the extension
    /// </summary>
    [JsonPropertyName("commands")]
    public List<CommandContribution> Commands { get; set; } = new();

    /// <summary>
    /// Panels contributed by the extension
    /// </summary>
    [JsonPropertyName("panels")]
    public List<PanelContribution> Panels { get; set; } = new();

    /// <summary>
    /// Events contributed by the extension
    /// </summary>
    [JsonPropertyName("events")]
    public List<EventContribution> Events { get; set; } = new();

    /// <summary>
    /// Services contributed by the extension
    /// </summary>
    [JsonPropertyName("services")]
    public List<ServiceContribution> Services { get; set; } = new();
}

/// <summary>
/// Command contribution definition
/// </summary>
public sealed class CommandContribution
{
    /// <summary>
    /// Command identifier
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Command title
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Command description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Command category
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// Panel contribution definition
/// </summary>
public sealed class PanelContribution
{
    /// <summary>
    /// Panel identifier
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Panel title
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Panel URL or HTML file
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Panel category
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// Event contribution definition
/// </summary>
public sealed class EventContribution
{
    /// <summary>
    /// Event name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Event description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Service contribution definition
/// </summary>
public sealed class ServiceContribution
{
    /// <summary>
    /// Service name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Service description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Service type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
} 