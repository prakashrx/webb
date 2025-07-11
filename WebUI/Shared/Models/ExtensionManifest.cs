namespace WebUI.Shared.Models;

/// <summary>
/// Extension manifest model defining extension metadata and contributions
/// </summary>
public sealed class ExtensionManifest
{
    /// <summary>
    /// Extension unique identifier
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Extension display name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Extension version
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Extension description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Extension author
    /// </summary>
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Extension entry point (main JavaScript file)
    /// </summary>
    [JsonPropertyName("main")]
    public string Main { get; set; } = string.Empty;

    /// <summary>
    /// Extension contribution points
    /// </summary>
    [JsonPropertyName("contributes")]
    public ExtensionContributions Contributes { get; set; } = new();

    /// <summary>
    /// Extension dependencies
    /// </summary>
    [JsonPropertyName("dependencies")]
    public Dictionary<string, string> Dependencies { get; set; } = new();

    /// <summary>
    /// Extension permissions
    /// </summary>
    [JsonPropertyName("permissions")]
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Minimum platform version required
    /// </summary>
    [JsonPropertyName("platformVersion")]
    public string PlatformVersion { get; set; } = string.Empty;
} 