# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WebUI Platform is a VS Code-style extensible desktop shell for building real-time trading applications. It provides a minimal C# runtime with WebView2-based panels and a clean JavaScript API for extensions.

**Current Status**: Foundation implementation complete with WebView2 hosting and window management. Ready for core API and extension system implementation.

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
cd WebUI/UIComponents
npm install

# Development server with hot reload
npm run dev

# Production build
npm run build
```

## Architecture Overview

### Core Components
- **Workbench** (`WebUI.Workbench`): Main application entry point and window lifecycle management
- **Core** (`WebUI.Core`): Platform libraries for WebView2, window management, and extension APIs
- **Core Extensions** (`WebUI/extensions/core`): Built-in UI components (main-toolbar, settings) as Svelte components
- **WebUI API** (`WebUI/webui-api`): TypeScript API library providing `webui.panel` and `webui.ipc` namespaces

### Key Classes  
- **WorkbenchEntry** (`WebUI.Workbench/WorkbenchEntry.cs:26`): Creates frameless main toolbar window and loads core extension
- **BrowserWindow** (`WebUI.Core/Windows/BrowserWindow.cs`): Tauri-inspired WebView2 wrapper with extension support
- **WebViewHost** (`WebUI.Core/Windows/WebViewHost.cs`): WebView2 hosting infrastructure with COM integration
- **HostApiBridge** (`WebUI.Core/Api/HostApiBridge.cs`): Main COM bridge exposing Panel and IPC APIs to JavaScript
- **PanelApi** (`WebUI.Core/Api/PanelApi.cs`): Panel registration, lifecycle, and window control methods

### Extension System (Current Implementation)
The platform follows an "everything is an extension" architecture:
- **Core Extensions**: Built-in Svelte components loaded via `extension://` URLs (main-toolbar, settings)
- **Extension Registration**: Components register via `webui.panel.registerPanel(id, SvelteComponent)`
- **Virtual Host Mapping**: Extensions served from filesystem at `http://webui.local/extensionId/`
- **HTML Generation**: Dynamic HTML creation with module imports and component mounting
- **JavaScript API**: Global `webui` object with `panel`, `ipc`, and `extension` namespaces

## Current Implementation Status

### ✅ Completed (MVP Foundation)
- **WebView2 Infrastructure**: BrowserWindow and WebViewHost with proper initialization, events, and lifecycle management
- **Extension Loading System**: Direct HTML generation with virtual host mapping for core extensions
- **COM API Bridge**: HostApiBridge, PanelApi, and IpcApi for JavaScript ↔ C# communication  
- **WebUI JavaScript API**: Complete TypeScript API (`webui.panel`, `webui.ipc`, `webui.extension`) with bundled distribution
- **Core Extension Framework**: Svelte-based core extension with MainToolbar and Settings panels
- **Working Demo**: Functional main-toolbar extension loading with panel registration and mounting

### 🔄 Current MVP State
- Workbench creates frameless browser window (1200x40) for main toolbar
- Loads `extension://core/main-toolbar` which generates HTML and mounts Svelte components
- WebUI API injected globally for extension communication
- Core extension registers panels via `webui.panel.registerPanel()`
- Virtual host mapping serves extension assets from local filesystem

## Development Guidelines

### C# Conventions
- Use modern C# features (file-scoped namespaces, global usings, nullable reference types)
- Follow KISS principle - avoid over-engineering
- Prefer composition over inheritance
- Use existing .NET APIs rather than reinventing
- Target .NET 9.0 with Windows Forms and WebView2

### Extension Development  
- Extensions are manifest-based with `activate(context)` lifecycle
- Core extensions load from disk, user extensions from HTTP
- Panel isolation via iframes with extension identity in query params
- Clean separation between platform APIs and extension code

## Testing and Validation

### Current Demo
```bash
cd WebUI/Workbench
dotnet run
```
Launches frameless browser window (1200x40) with main-toolbar extension. Demonstrates:
- Extension loading via `extension://core/main-toolbar` 
- Svelte component registration and mounting
- WebUI API injection and global availability
- Window controls (minimize, maximize, close) from JavaScript

### Next Steps for MVP Improvement
- Clean up HTML generation and component mounting logic
- Implement proper IPC message handling (currently stubs)
- Add error handling and debugging for extension loading
- Standardize extension manifest processing
- Add support for dynamic panel layouts

## Key Files and Locations

### Core Implementation
- `WebUI/Core/Windows/` - WebView2 hosting and window management
- `WebUI/Shared/Contracts/` - API interface definitions
- `WebUI/Workbench/WorkbenchEntry.cs` - Main application entry point

### Configuration
- `WebUI/WebUI.Platform.sln` - Main solution file
- `WebUI/UIComponents/package.json` - Svelte build configuration
- `.cursor/rules/` - Development rules and conventions

### Documentation
- `WebUI/PROJECT_STRUCTURE.md` - Detailed architecture documentation
- `.cursor/memory/poc-tasks.md` - POC implementation plan
- `.cursor/memory/implementation-tasks.md` - Detailed task breakdown

## Architecture Principles

1. **Extension-First**: Platform UI built from core extensions
2. **HTTP-Served Extensions**: User extensions served from development servers
3. **Iframe Isolation**: User panels run in isolated iframes  
4. **Central IPC Routing**: All communication flows through workbench
5. **Clean API Surface**: Simple `webui.panel` and `webui.ipc` namespaces
6. **Modern Development**: Vite + Svelte + TypeScript for extensions

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

### Extension Loading Pattern (Planned)
```javascript
// Extension activate function
export function activate(context) {
    context.panel.registerView("main", "http://localhost:3001/panel.html");
    context.ipc.on("refresh", handleRefresh);
}
```

This codebase represents a solid foundation ready for the core API and extension system implementation outlined in the POC tasks.