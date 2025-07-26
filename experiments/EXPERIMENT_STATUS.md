# WebUI Desktop SDK Experiment - Status Report

## Overview
We've successfully created an experimental desktop framework that provides WinForms-like simplicity for building applications with Svelte and modern web technologies. The framework is distributed as a single NuGet package containing both runtime and build tools.

## Current Implementation

### Core Features Implemented
1. **Single NuGet Package Distribution** (`WebUI.Desktop`)
   - Combined runtime library and build tools
   - MSBuild props/targets flow automatically
   - WebView2 included as transitive dependency
   - WebUI.Api source included and built on-demand

2. **Runtime Library**
   - Simple API: `WebUI.Run("MainWindow")`
   - WebView2-based panel hosting
   - Virtual host mapping (http://webui.local/)
   - Built-in hot reload support
   - Dynamic command discovery

3. **Build Pipeline**
   - Automatic WebUI.Api compilation when needed
   - Svelte → ES module compilation
   - Integrated Tailwind CSS with PostCSS
   - Zero-configuration development experience

4. **Hot Reload System**
   - Uses `dotnet watch` for Svelte file monitoring
   - FileSystemWatcher for JS change detection
   - Browser refresh without app restart
   - Smooth development experience

### Project Structure
```
experiments/
├── src/
│   ├── WebUI.Desktop/          # Runtime library
│   │   └── WebUI.cs           # Main API
│   └── WebUI.Desktop.Sdk/      # MSBuild SDK
│       ├── Sdk/
│       │   ├── Sdk.props      # Default project settings
│       │   └── Sdk.targets    # Build pipeline
│       └── tools/build/       # Node.js build tools
│           ├── build-panel.js # Svelte compilation
│           ├── base.css       # Tailwind base
│           └── package.json   # Build dependencies
└── samples/
    └── HelloWorld/            # Sample app
        ├── HelloWorld.csproj
        ├── MainWindow.svelte
        └── Program.cs
```

### How It Works

#### Build Process
1. Developer writes Svelte files and marks them as `<Panel>`
2. MSBuild triggers Node.js build script
3. Rollup compiles Svelte → ES modules
4. Tailwind CSS is processed and injected
5. Output goes to `bin/Debug/panels/*.js`

#### Runtime Process
1. `WebUI.Run("MainWindow")` creates a Form with WebView2
2. Virtual host maps `panels/` directory to `http://webui.local/`
3. WebView2 loads HTML that imports the ES module
4. Svelte component mounts and runs

#### Hot Reload Flow
1. Terminal 1: `dotnet watch msbuild /t:watch`
   - Watches `*.svelte` files
   - Recompiles on change (no app rebuild)
2. Terminal 2: `dotnet run`
   - FileSystemWatcher monitors `panels/*.js`
   - Reloads WebView2 when JS updates
3. Result: Instant updates without app restart

## Design Decisions Made

### 1. **File System vs Embedded Resources**
- **Chose**: File system approach
- **Why**: Simpler, better for debugging, enables hot reload
- **Alternative considered**: Embedded resources for single-file deployment

### 2. **ES Modules vs IIFE**
- **Chose**: ES modules
- **Why**: Modern, better tree-shaking, cleaner imports
- **Implementation**: `format: 'es'` in Rollup config

### 3. **Panel Discovery**
- **Chose**: Explicit `<Panel Include="*.svelte" />` declaration
- **Why**: Intentional, no magic, clear project structure
- **Alternative rejected**: Auto-discovery of all Svelte files

### 4. **Hot Reload Approach**
- **Chose**: dotnet watch + FileSystemWatcher
- **Why**: Uses standard .NET tooling, no custom watchers
- **Alternatives considered**:
  - WebSocket-based HMR (too complex)
  - Full app restart with dotnet watch (poor UX)
  - Custom Node.js watcher (not .NET-first)

### 5. **Tailwind Integration**
- **Chose**: Automatic injection via base.css wrapper
- **Why**: Zero configuration, works transparently
- **Implementation**: Build script creates temporary wrapper

## Technical Challenges Solved

1. **Double SDK Import Error**
   - Fixed by not re-importing Microsoft.NET.Sdk in our targets

2. **STAThread Requirement**
   - Added `[STAThread]` to generated Main method

3. **ES Module Loading**
   - Changed from `Panel.default` to `Panel` for proper ES module imports

4. **Tailwind CSS Not Applying**
   - Created wrapper system to import base CSS
   - Configured content paths dynamically

5. **Timer Ambiguity**
   - Fully qualified `System.Threading.Timer`

## What's Not Implemented Yet

### 1. **WebUI JavaScript API**
- Panel management (`webui.panel.open()`, etc.)
- Window controls (minimize, maximize, close)
- Panel lifecycle events

### 2. **IPC Messaging System**
- Inter-panel communication
- Panel ↔ Host messaging
- Event subscription system

### 3. **Advanced Features**
- Multi-window support
- Panel persistence/state
- Custom window chrome
- Native menu integration
- System tray support

### 4. **Developer Experience**
- Project templates
- VS/VS Code integration
- Better error messages
- Dev tools integration

## Lessons Learned

1. **MSBuild SDK Development**
   - Use Directory.Build.props/targets for local development
   - Props run before project, Targets run after
   - Watch items enable dotnet watch integration

2. **WebView2 Integration**
   - Virtual host mapping is powerful for local serving
   - NavigateToString works well for dynamic content
   - Reload() is sufficient for hot reload

3. **Svelte + Rollup**
   - emitCss: true needed for style extraction
   - PostCSS integration requires careful plugin ordering
   - ES modules work great with WebView2

4. **Developer Experience**
   - Hot reload is crucial for productivity
   - Standard tooling (dotnet watch) is better than custom
   - Clear error messages save debugging time

## Next Steps Priority

1. **High Priority**
   - Implement WebUI JavaScript API
   - Add IPC messaging for panel communication
   - Create project templates

2. **Medium Priority**
   - Add window management features
   - Implement panel persistence
   - Better error handling

3. **Future Enhancements**
   - VS Code extension
   - Hot Module Replacement (HMR)
   - Production optimizations
   - Embedded resource mode

## Key Code References

### SDK Usage (HelloWorld.csproj)
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Panel Include="MainWindow.svelte" />
  </ItemGroup>
</Project>
```

### Simple API (Program.cs)
```csharp
using WebUI;

WebUI.WebUI.Run("MainWindow");
```

### Hot Reload Commands
```bash
# Terminal 1: Watch and compile Svelte
dotnet watch msbuild /t:watch

# Terminal 2: Run the app
dotnet run
```

## Summary
We've created a working prototype that successfully demonstrates the vision of a .NET-first desktop framework with modern web UI. The SDK provides a clean, simple API while hiding the complexity of Svelte compilation, module bundling, and hot reload. The development experience is smooth, using familiar .NET tooling while leveraging the power of modern web technologies.