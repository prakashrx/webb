# WebUI Platform - Project Structure & API Reference

## Project Architecture

```
WebUI.Platform/
├── WebUI.Platform.sln          # Solution file
├── Shell/                      # Main application (Shell project)
│   ├── Program.cs             # Application bootstrap
│   ├── MainForm.cs            # Windows Forms lifecycle manager
│   └── WebUI.Shell.csproj     # Project configuration
├── Core/                       # Core framework library
│   ├── BrowserWindow.cs       # Main API - Tauri-like window creation
│   ├── WebViewHost.cs         # WebView2 wrapper/host implementation
│   ├── IWebViewHost.cs        # WebView host interface
│   ├── WebViewOptions.cs      # Configuration options class
│   ├── WindowControls.cs      # Window control operations for frameless windows
│   ├── WindowHelper.cs        # Window utility functions
│   ├── GlobalUsings.cs        # Global namespace imports
│   └── WebUI.Core.csproj      # Framework project configuration
├── UIComponents/               # Svelte component library
│   ├── src/                   # Svelte source files
│   ├── public/                # Static assets
│   ├── package.json           # NPM dependencies
│   └── rollup.config.js       # Build configuration
├── Host/                       # Placeholder for future host processes
├── Extensions/                 # Placeholder for future extensions
└── Tools/                      # Placeholder for future CLI tools
```

## Core Classes & Functionality

### BrowserWindow (WebUI.Core.Windows)
**Purpose**: Main API class providing Tauri-like instantiable browser windows
**Dependencies**: WebViewHost, WindowControls, System.Windows.Forms

#### Constructor
```csharp
BrowserWindow(string title, int width, int height, bool resizable, bool devTools, bool frameless)
```
- Creates Form with specified dimensions/properties
- Sets FormBorderStyle.None for frameless windows
- Initializes WebViewHost with WebView2 control
- Calls InitializeAsync() for WebView2 setup

#### Core Methods
- `NavigateAsync(string url)` - Navigate to URL, awaits initialization
- `LoadHtmlAsync(string html)` - Load HTML content, awaits initialization  
- `EvaluateAsync(string script)` - Execute JavaScript, returns result
- `AddHostObjectAsync(string name, object hostObject)` - Add .NET object to JS context
- `Show()` - Display window
- `Hide()` - Hide window
- `Close()` - Close window
- `SetPosition(int x, int y)` - Set window position
- `SetSize(int width, int height)` - Set window size
- `SetTitle(string title)` - Set window title
- `SetAlwaysOnTop(bool alwaysOnTop)` - Toggle always on top
- `Minimize()` - Minimize window
- `Maximize()` - Maximize window
- `Restore()` - Restore window

#### Events
- `Closed` - Fired when window closes
- `MessageReceived` - Fired when JS sends message via postMessage

#### Internal Methods
- `InitializeAsync(bool devTools, bool frameless)` - Initialize WebView2 with options, add WindowControls for frameless windows
- `Dispose()` - Clean up WebViewHost and Form resources

### WebViewHost (WebUI.Core.Windows)
**Purpose**: WebView2 wrapper providing initialization, navigation, script execution, host object management
**Dependencies**: Microsoft.Web.WebView2, WebViewOptions

#### Properties
- `WebView2 WebView` - Underlying WebView2 control
- `bool _isInitialized` - Initialization state
- `Dictionary<string, object> _hostObjects` - Host object registry

#### Core Methods
- `InitializeAsync(WebViewOptions options)` - Create WebView2 environment, configure settings, set initialization flag
- `NavigateAsync(string urlOrHtml, bool isHtml)` - Navigate to URL or load HTML string
- `ExecuteScriptAsync(string script)` - Execute JavaScript, return result
- `AddHostObject(string name, object hostObject)` - Register .NET object for JS interop
- `RemoveHostObject(string name)` - Remove host object
- `EnsureInitialized()` - Throws if not initialized

#### Internal Methods
- `SetupWebViewEvents()` - Wire CoreWebView2InitializationCompleted, NavigationCompleted events
- `OnCoreWebView2InitializationCompleted()` - Setup WebMessageReceived, WindowCloseRequested, NewWindowRequested events
- `ConfigureWebViewSettings(WebViewOptions options)` - Configure WebView2 settings (security, dev tools, etc.)
- `TrySetProperty(object target, string propertyName, object value)` - Reflection-based property setting with graceful fallback

### WindowControls (WebUI.Core.Windows)
**Purpose**: Window control operations for frameless windows, exposed to JavaScript
**Dependencies**: System.Windows.Forms

#### Constructor
```csharp
WindowControls(Form form)
```
- Stores form reference for window operations

#### JavaScript-Exposed Methods
- `Minimize()` - Minimize window via Form.Invoke
- `Maximize()` - Maximize window via Form.Invoke
- `Restore()` - Restore window via Form.Invoke
- `Close()` - Close window via Form.Invoke
- `IsMaximized()` - Return maximized state
- `MoveWindow(int deltaX, int deltaY)` - Move window by delta, includes screen bounds checking
- `ToggleMaximize()` - Toggle between maximized/normal state
- `GetWindowState()` - Return "minimized"/"maximized"/"normal"/"unknown"
- `SetSize(int width, int height)` - Set window size
- `GetSize()` - Return anonymous object {width, height}
- `GetPosition()` - Return anonymous object {x, y}

### WebViewOptions (WebUI.Core.Windows)
**Purpose**: Configuration class for WebView2 initialization
**Properties**:
- `string? UserDataFolder` - WebView2 user data directory
- `string[]? AdditionalBrowserArguments` - Extra browser arguments
- `bool EnableDeveloperTools` - Enable F12 dev tools (default: true)
- `bool EnableScriptDebugging` - Enable script debugging (default: true)
- `bool EnableWebSecurity` - Enable web security (default: true)
- `bool AllowInsecureContent` - Allow insecure content (default: false)
- `Dictionary<string, string>? CustomSchemes` - Custom scheme registrations
- `bool EnableHighDpiSupport` - Enable high DPI support (default: true)
- `double ZoomFactor` - Initial zoom factor (default: 1.0)

### MainForm (WebUI.Shell)
**Purpose**: Windows Forms application lifecycle manager, creates BrowserWindow instance
**Dependencies**: BrowserWindow, WebUI.Core.Windows

#### Constructor
- Calls InitializeComponent() for form setup
- Calls InitializeAsync() for BrowserWindow creation

#### InitializeAsync Method
- Hides MainForm (used only for app lifecycle)
- Creates BrowserWindow with frameless=true
- Loads demo HTML with custom title bar using -webkit-app-region CSS
- Wires MessageReceived and Closed events
- Shows BrowserWindow

#### HTML Template Features
- Custom title bar with -webkit-app-region: drag
- Window control buttons with -webkit-app-region: no-drag
- JavaScript functions calling windowControls host object methods
- Modern CSS styling with backdrop-filter effects

### Program (WebUI.Shell)
**Purpose**: Application bootstrap and entry point
**Methods**:
- `Main()` - [STAThread] entry point, calls ApplicationConfiguration.Initialize(), creates/runs MainForm

## Key Design Patterns

### Frameless Window Implementation
1. **CSS-Based Dragging**: Uses -webkit-app-region: drag/no-drag instead of JavaScript mouse events
2. **Host Object Pattern**: WindowControls class exposed to JavaScript for window operations
3. **Event Wiring**: Form events wired to BrowserWindow events for proper lifecycle management

### Async Initialization Pattern
1. **Task Storage**: _initializationTask stored in constructor
2. **Await Before Use**: All WebView operations await initialization task
3. **Single Initialization**: InitializeAsync checks _isInitialized flag

### WebView2 Integration
1. **Environment Creation**: CoreWebView2Environment.CreateAsync with options
2. **Settings Configuration**: Programmatic WebView2 settings via ConfigureWebViewSettings
3. **Host Object Registration**: AddHostObjectToScript for .NET-JS interop
4. **Event Handling**: WebMessageReceived for JS-to-.NET communication

### Error Handling
1. **Graceful Fallbacks**: TrySetProperty uses reflection with exception handling
2. **Initialization Guards**: EnsureInitialized throws descriptive exceptions
3. **Resource Cleanup**: Dispose pattern for WebView2 and Form resources

## Framework Dependencies
- .NET 9.0 Windows target
- Microsoft.Web.WebView2 NuGet package
- System.Windows.Forms enabled
- ImplicitUsings enabled for common System namespaces

## Usage Pattern
```csharp
// Simple usage
var window = new BrowserWindow("Title", 800, 600);
await window.LoadHtmlAsync("<html>...</html>");
window.Show();

// Frameless window
var window = new BrowserWindow("Title", 800, 600, frameless: true);
// HTML uses -webkit-app-region: drag for title bar
// JavaScript calls window.chrome.webview.hostObjects.windowControls methods
```

## Build Configuration
- WebUI.Platform.sln contains WebUI.Shell and WebUI.Core projects
- WebUI.Core builds to class library
- WebUI.Shell builds to Windows executable with WebUI.Core reference
- WebUI.UIComponents contains Svelte build system (separate from main .NET build) 