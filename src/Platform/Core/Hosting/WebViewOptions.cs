namespace WebUI.Core.Hosting;

/// <summary>
/// Configuration options for WebView instances
/// </summary>
public sealed class WebViewOptions
{
    /// <summary>
    /// User data folder path for the WebView
    /// </summary>
    public string? UserDataFolder { get; init; }

    /// <summary>
    /// Additional browser arguments
    /// </summary>
    public string[]? AdditionalBrowserArguments { get; init; }

    /// <summary>
    /// Whether to enable developer tools
    /// </summary>
    public bool EnableDeveloperTools { get; init; } = true;

    /// <summary>
    /// Whether to enable script debugging
    /// </summary>
    public bool EnableScriptDebugging { get; init; } = true;

    /// <summary>
    /// Whether to enable web security
    /// </summary>
    public bool EnableWebSecurity { get; init; } = true;

    /// <summary>
    /// Whether to allow insecure content
    /// </summary>
    public bool AllowInsecureContent { get; init; } = false;

    /// <summary>
    /// Custom scheme registrations
    /// </summary>
    public Dictionary<string, string>? CustomSchemes { get; init; }

    /// <summary>
    /// Whether to enable high DPI support
    /// </summary>
    public bool EnableHighDpiSupport { get; init; } = true;

    /// <summary>
    /// Initial zoom factor (1.0 = 100%)
    /// </summary>
    public double ZoomFactor { get; init; } = 1.0;
} 