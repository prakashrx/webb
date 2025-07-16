# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WebUI Platform is a VS Code-style extensible desktop shell for building real-time trading applications. It provides a minimal C# runtime with WebView2-based panels and a clean JavaScript API for panels.

**Current Status**: Panel system fully implemented with working IPC communication between JavaScript and C#. Main toolbar and settings panels functional with simplified architecture.

## Build Commands

### .NET Solution
```bash
# Build entire solution
cd WebUI
dotnet build WebUI.Platform.sln

# Run main application
cd WebUI/Workbench
dotnet run

# Build specific project
dotnet build WebUI.Core/WebUI.Core.csproj
```

### UI Components (Svelte)
```bash
# Install dependencies
cd src/UI/workbench
npm install

# Build all panels
npm run build

# Clean build artifacts
npm run clean
```

## Architecture Overview

### Core Components
- **Workbench** (`src/Workbench`): Main application entry point and window lifecycle management
- **Core** (`src/Platform/Core`): Platform libraries for WebView2, window management, and panel APIs
- **Panels** (`src/UI/workbench/panels`): Built-in Svelte panels (main-toolbar, settings, workspace)
- **WebUI API** (`src/UI/api`): TypeScript API library providing `webui.panel` and `webui.ipc` namespaces

### Key Classes  
- **WorkbenchEntry** (`src/Workbench/WorkbenchEntry.cs:53`): Creates frameless main toolbar window and initializes panels
- **WindowManager** (`src/Platform/Core/Windows/WindowManager.cs`): Manages all panels with IPC routing
- **Panel** (`src/Platform/Core/Panels/Panel.cs`): WebView2-based panel implementation
- **IpcRouter** (`src/Platform/Core/Communication/IpcRouter.cs`): Simplified message routing without double-wrapping
- **HostApiBridge** (`src/Platform/Core/Api/HostApiBridge.cs`): COM bridge exposing Panel and IPC APIs to JavaScript

### Panel-Based Architecture
The platform uses a clean panel-based architecture:
- **Panels**: Self-contained UI modules built with Svelte and bundled with Rollup
- **IPC Communication**: Panels communicate via `webui.ipc` for inter-panel messaging
- **Panel Management**: WindowManager handles panel lifecycle and registration
- **WebUI API**: Global JavaScript API injected into all panels for platform access
- **Virtual Host**: Panels served from `http://webui.local/` with filesystem mapping

## Current Implementation Status

### ✅ Completed
- **WebView2 Infrastructure**: Clean BrowserWindow wrapper with proper lifecycle management
- **Panel System**: Complete panel abstraction with IPanel interface and Panel implementation
- **Window Manager**: Central manager for all panels with IPC routing integration
- **IPC Communication**: Simplified IpcRouter and IpcTransport without double-wrapping
- **COM API Bridge**: HostApiBridge exposing PanelApi and IpcApi to JavaScript
- **WebUI JavaScript API**: TypeScript API with `webui.panel` and `webui.ipc` namespaces
- **Panel Build System**: Rollup configuration for self-contained panel bundles with CSS injection
- **Working Panels**: Main toolbar, settings, and workspace panels all functional

### 🔄 Current Architecture
- **Core Framework** (`src/Platform/Core`):
  - Panel abstraction for creating/managing WebView2 windows
  - WindowManager for centralized panel lifecycle and IPC routing
  - WebUI API injection via WebView2 initialization
  - Virtual host mapping at `http://webui.local/`
  - Simplified IPC with direct message routing (no double-wrapping)
- **Workbench Application** (`src/Workbench`):
  - Creates main toolbar as frameless window
  - Registers all panels with WindowManager
  - Handles panel open/close via IPC messages
- **UI Structure**:
  - `src/UI/workbench/panels/` - Svelte panel components
  - `src/UI/workbench/dist/` - Built panel bundles
  - `src/UI/api/` - WebUI JavaScript API library
  - Self-contained panels with embedded CSS

## Development Guidelines

### C# Conventions
- Use modern C# features (file-scoped namespaces, global usings, nullable reference types)
- Follow KISS principle - avoid over-engineering
- Prefer composition over inheritance
- Use existing .NET APIs rather than reinventing
- Target .NET 9.0 with Windows Forms and WebView2

### Panel Development  
- Panels are self-contained Svelte components
- Built with Rollup for single-file output with embedded CSS
- Register via `webui.panel.registerPanel(id, component)`
- Communicate via `webui.ipc.send()` and `webui.ipc.on()`
- Access platform features through WebUI API

## Notes for Development

### Workflow Notes
- **Build Process**:
  - Dont try to run the app yourself, I will run and report back the result. But you should build and see if there is no error

## Testing and Validation

### Running the Application
```bash
cd src/Workbench
dotnet run
```
Launches frameless main toolbar (1200x40) with working panels:
- Click "Settings" to open settings panel
- Click "Workspace" to open workspace panel  
- All panels have minimize/maximize/close controls
- IPC communication working between panels
- WebUI API available globally in all panels

### Recent Accomplishments

**Completed Refactoring:**
1. ✅ **Removed Extension Model** - Cleaned up all "extension" and "screen" terminology
2. ✅ **Implemented Panel System** - Clean panel-based architecture with IPanel interface  
3. ✅ **Fixed IPC Communication** - Removed double-wrapping, messages flow correctly
4. ✅ **Rollup Build System** - Self-contained panel bundles with embedded CSS
5. ✅ **Working Multi-Panel App** - Main toolbar opens settings and workspace panels
6. ✅ **Simplified Architecture** - WindowManager handles all panels with single IpcRouter

**Key Improvements:**
- Changed from Vite to Rollup for proper self-contained builds
- Fixed case sensitivity issues in panel file names
- Removed unnecessary RouterMessage wrapper in IPC system
- All panels now successfully open and communicate via IPC

## Key Files and Locations

### Core Implementation
- `src/Platform/Core/Windows/` - WindowManager and BrowserWindow classes
- `src/Platform/Core/Panels/` - Panel abstraction and implementation
- `src/Platform/Core/Communication/` - IpcRouter and IpcTransport
- `src/Platform/Core/Api/` - HostApiBridge, PanelApi, and IpcApi
- `src/Workbench/WorkbenchEntry.cs` - Main application entry point

### UI Implementation
- `src/UI/workbench/panels/` - Svelte panel source files
- `src/UI/workbench/dist/` - Built panel bundles
- `src/UI/api/` - WebUI JavaScript API
- `src/UI/workbench/rollup.config.js` - Build configuration

### Configuration
- `src/WebUI.Platform.sln` - Main solution file
- `src/UI/workbench/package.json` - Panel build scripts

## Architecture Principles

1. **Panel-Based Design**: Everything is a panel with its own WebView2 window
2. **Simplified IPC**: Direct message routing without unnecessary wrapping
3. **Self-Contained Bundles**: Panels built as single JS files with embedded CSS
4. **Central Management**: WindowManager handles all panel lifecycle and routing
5. **Clean API Surface**: Simple `webui.panel` and `webui.ipc` namespaces
6. **Modern Stack**: Rollup + Svelte + TypeScript for panel development

## Common Patterns

### WebView2 Integration
```csharp
// Create and configure WebView2 window
var window = new BrowserWindow("Title", 800, 600, new WebViewOptions());
await window.LoadHtmlAsync(htmlContent);
window.AddHostObject("api", comObject);
```

### COM Bridge Registration
```csharp
[ComVisible(true)]
public class HostApiBridge
{
    // Exposed to JavaScript via window.chrome.webview.hostObjects.api
}
```

### Panel Registration Pattern
```javascript
// In panel's main.js
import PanelComponent from './MyPanel.svelte';

webui.panel.registerPanel('my-panel', PanelComponent, {
    title: 'My Panel'
});

// Open another panel
webui.panel.open('settings');

// Listen for IPC messages
webui.ipc.on('data-update', (data) => {
    console.log('Received:', data);
});
```

## Next Steps

With the panel system working, the next phase would include:
1. **Enhanced UI Components**: Status bar, side panels, docking support
2. **Advanced IPC**: Request/response patterns, event subscriptions
3. **Panel Persistence**: Save/restore panel layouts and states
4. **Developer Tools**: Hot reload support, better debugging
5. **Extended API**: File operations, notifications, context menus
6. **Performance**: WebView2 pooling, lazy loading, shared processes