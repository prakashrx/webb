using System.Text.Json.Serialization;

namespace WebUI.Core.Extensions;

/// <summary>
/// Extension manifest model
/// </summary>
public class ExtensionManifest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("main")]
    public string Main { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("panels")]
    public List<PanelDefinition>? Panels { get; set; }
}

/// <summary>
/// Panel definition in manifest
/// </summary>
public class PanelDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("component")]
    public string? Component { get; set; }
}