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
- **Workbench**: Main application process and extension coordinator (WebUI.Workbench)
- **Core**: Platform libraries for WebView2 and window management (WebUI.Core) 
- **Shared**: API contracts and models for extension system (WebUI.Shared)
- **UIComponents**: Svelte component library for web-based UI (planned)

### Key Classes
- **BrowserWindow** (`WebUI.Core/Windows/BrowserWindow.cs`): Tauri-inspired WebView2 wrapper
- **WebViewHost** (`WebUI.Core/Windows/WebViewHost.cs`): WebView2 hosting infrastructure
- **WorkbenchEntry** (`WebUI.Workbench/WorkbenchEntry.cs`): Main application form and lifecycle
- **WindowControls** (`WebUI.Core/Windows/WindowControls.cs`): COM object for JS window controls

### Extension System (Planned)
The platform follows an "everything is an extension" architecture:
- **Core Extensions**: Platform UI (toolbar, panel chrome) implemented as extensions
- **User Extensions**: Loaded from HTTP with live reload for development
- **IPC Communication**: Named pipe routing between processes and extensions  
- **JavaScript API**: Clean `webui.panel` and `webui.ipc` namespaces

## Current Implementation Status

### ✅ Completed
- WebView2 hosting with BrowserWindow API
- Window management and controls
- COM interop patterns (WindowControls)
- Basic project structure and build system
- API contracts defined in Shared project

### 🔄 Next Phase (POC Implementation)
- HostApiBridge COM interface for extension API
- Extension loading system (core + HTTP-based)
- Core extensions (panel-container, main-toolbar)
- IPC transport with named pipes
- Test extension with Svelte + Vite

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
Launches WebView2 window with custom title bar demonstrating JavaScript ↔ C# communication.

### POC Validation (Planned)
- Load core extensions (panel-container, main-toolbar)
- Load test extension from `http://localhost:3001`
- Verify iframe isolation and IPC communication
- Test extension lifecycle events and error handling

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