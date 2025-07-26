# Technical Implementation Notes

## Build Pipeline Details

### MSBuild Targets Execution Order
1. `CheckNodeJS` - Verifies Node.js is installed
2. `RestoreWebUITools` - Runs npm install if needed
3. `CompilePanels` - Builds all Panel items
4. `CompilePanelsOnly` - Used by dotnet watch (no project build)

### Rollup Build Process
1. Creates temporary wrapper importing base.css and Svelte component
2. Runs Rollup with:
   - Svelte preprocessor (PostCSS support)
   - PostCSS with Tailwind (scans component for classes)
   - CommonJS and resolve plugins
   - Terser for production builds
3. Outputs ES module to panels directory
4. Cleans up temporary wrapper

### Tailwind CSS Integration
- `base.css` contains @tailwind directives
- Build dynamically configures content paths
- CSS is injected into JS bundle (no separate CSS files)
- Only used classes are included (tree-shaking)

## Runtime Architecture

### WebView2 Setup
```csharp
// Virtual host mapping
webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
    "webui.local",
    panelsPath,
    CoreWebView2HostResourceAccessKind.Allow);

// Load panel
webView.NavigateToString($@"
    <script type=""module"">
        import Panel from 'http://webui.local/{panelName}.js';
        new Panel({{ target: document.getElementById('app') }});
    </script>
");
```

### Hot Reload Implementation
```csharp
// FileSystemWatcher with debouncing
var watcher = new FileSystemWatcher(panelsPath)
{
    Filter = $"{panelName}.js",
    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
};

// Debounced reload
System.Threading.Timer? debounceTimer = null;
watcher.Changed += (sender, e) =>
{
    debounceTimer?.Dispose();
    debounceTimer = new System.Threading.Timer(_ =>
    {
        webView.BeginInvoke(() => webView.Reload());
    }, null, 100, Timeout.Infinite);
};
```

## File Paths and Locations

### Build Files (Included in NuGet Package)
- Props: `src/WebUI.Desktop/build/WebUI.Desktop.props`
- Targets: `src/WebUI.Desktop/build/WebUI.Desktop.targets`
- Build script: `src/WebUI.Desktop/tools/build/build-panel.js`
- Base CSS: `src/WebUI.Desktop/tools/build/base.css`

### Output Structure
```
bin/Debug/net9.0-windows/
├── HelloWorld.exe
├── WebUI.Desktop.dll
├── panels/
│   └── MainWindow.js    # Compiled Svelte component
└── [other runtime files]
```

## Known Issues and Workarounds

### 1. Tailwind Content Warning
- **Issue**: "Content option is missing" warning
- **Cause**: Dynamic content configuration
- **Impact**: Cosmetic only, Tailwind still works

### 2. FileSystemWatcher "Too many changes"
- **Issue**: Error when many files change at once
- **Cause**: Rebuild triggers multiple file updates
- **Impact**: Recovers automatically

### 3. Initial Load Timing
- **Issue**: Rare race condition on first load
- **Solution**: WebView2 initialization is async

## Performance Considerations

1. **Build Performance**
   - Node.js process startup adds ~1s overhead
   - Rollup compilation typically <2s per component
   - Tailwind processing adds minimal time

2. **Runtime Performance**
   - ES modules load efficiently
   - No framework overhead (pure Svelte)
   - WebView2 shares Chrome's V8 performance

3. **Hot Reload Performance**
   - FileSystemWatcher has minimal overhead
   - Debouncing prevents excessive reloads
   - WebView2.Reload() is fast (~100ms)

## Security Considerations

1. **Virtual Host Isolation**
   - Content served from webui.local
   - No access to local file system
   - Standard web security model

2. **Script Injection**
   - Currently using NavigateToString
   - Consider CSP headers for production

3. **IPC Security** (Future)
   - Need to validate message sources
   - Consider permission model

## Debugging Tips

1. **Build Issues**
   - Check `dotnet build` output for Node.js errors
   - Verify panels/ directory has JS files
   - Look for Rollup error messages

2. **Runtime Issues**
   - Open WebView2 DevTools (F12)
   - Check console for module load errors
   - Verify virtual host mapping

3. **Hot Reload Issues**
   - Ensure both terminals are running
   - Check FileSystemWatcher is enabled
   - Verify JS file timestamp changes

## Alternative Approaches Considered

### 1. Embedded Resources
```csharp
// Could serve from assembly
var js = Assembly.GetExecutingAssembly()
    .GetManifestResourceStream("WebUI.Panels.MainWindow.js");
```
**Rejected**: Complicates hot reload

### 2. WebSocket HMR
```javascript
// Could implement Vite-style HMR
const ws = new WebSocket('ws://localhost:3000');
ws.onmessage = (e) => {
    if (e.data.type === 'update') {
        import(/* @vite-ignore */ module);
    }
};
```
**Rejected**: Too complex for prototype

### 3. Source Generators
```csharp
// Could generate panel registration
[Generator]
public class PanelGenerator : ISourceGenerator
{
    // Generate WebUI.Panels class
}
```
**Rejected**: Overkill for current needs

## Future Architecture Considerations

### 1. Plugin System
- Panels as NuGet packages
- Dynamic panel loading
- Panel marketplace?

### 2. State Management
- Consider stores pattern
- Panel state persistence
- Cross-panel state sync

### 3. Native Integration
- P/Invoke for system APIs
- Native window controls
- Platform-specific features

### 4. Testing Strategy
- Unit tests for build pipeline
- Integration tests for runtime
- E2E tests with Playwright?