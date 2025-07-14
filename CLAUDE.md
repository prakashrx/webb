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
- **Core** (`WebUI.Core`): Platform libraries for WebView2, window management, and native OS integration
- **Core UI** (`WebUI/ui/core`): Built-in Svelte/Tailwind UI components (toolbar, settings, panel containers)
- **Extensions** (`WebUI/extensions`): External plugins that run inside panel containers
- **WebUI API** (`WebUI/webui-api`): TypeScript API library providing native UI capabilities

### Key Classes  
- **WorkbenchEntry** (`WebUI.Workbench/WorkbenchEntry.cs`): Creates main application window and initializes core UI
- **BrowserWindow** (`WebUI.Core/Windows/BrowserWindow.cs`): Clean WebView2 wrapper (no extension knowledge)
- **WebViewHost** (`WebUI.Core/Windows/WebViewHost.cs`): WebView2 hosting infrastructure with COM integration
- **ExtensionHost** (`WebUI.Core/Extensions/ExtensionHost.cs`): Manages extension loading and lifecycle
- **HostApiBridge** (`WebUI.Core/Api/HostApiBridge.cs`): COM bridge exposing native APIs to JavaScript

### UI Architecture
The platform has a clear separation between core UI and extensions:
- **Core UI**: Built-in application shell (toolbar, settings, panel containers) using Svelte/Tailwind
- **Extensions**: External code that runs inside panel containers, isolated from core UI
- **Panel System**: Extensions render content inside panels with native chrome (title bar, tabs, etc.)
- **WebUI API**: Provides native UI patterns (context menus, dialogs, notifications) to both core and extensions
- **Extension Contributions**: Extensions can contribute commands, menus, settings via API

## Current Implementation Status

### ✅ Completed
- **WebView2 Infrastructure**: BrowserWindow and WebViewHost with proper initialization, events, and lifecycle management
- **Screen Abstraction**: IScreen, Screen, ScreenOptions, and ScreenManager for managing multiple WebView2 windows
- **COM API Bridge**: HostApiBridge, PanelApi, and IpcApi for JavaScript ↔ C# communication  
- **WebUI JavaScript API**: Complete TypeScript API (`webui.panel`, `webui.ipc`, `webui.extension`) with bundled distribution
- **Workbench UI**: Svelte-based UI components (MainToolbar, Settings) that run as built-in parts of the Workbench
- **Working Application**: Functional multi-screen application with toolbar, settings, and workspace screens

### 🔄 Current Architecture
- **Core Framework** (`WebUI.Core`):
  - Provides Screen abstraction for creating/managing WebView2 windows
  - Handles WebUI API injection via `AddScriptToExecuteOnDocumentCreatedAsync`
  - Virtual host mapping for serving UI assets
  - IPC infrastructure for communication
- **Workbench Application** (`WebUI.Workbench`):
  - Uses Core framework to create application screens
  - Workbench UI components are built-in (not extensions)
  - Manages screen lifecycle and inter-screen communication
- **UI Structure**:
  - `src/UI/workbench/` - Contains Workbench UI components
  - `src/UI/api/` - WebUI JavaScript API library
  - Clear separation between platform and application UI

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

## Project Architecture Considerations

### Framework Foundation Considerations
- Before thinking about extension we need a solid webUI foundation with ability to load multiple web UI screens. This foundation needs to be present in the core framework. The workbench uses the core framework to provide all our web UI screens.

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

### Architecture Refactor Plan

**New Vision:**
1. **Core UI is NOT Extensions** - Toolbar, settings, panel containers are built-in Svelte components
2. **Extensions Run IN Panels** - Extensions provide content, not chrome/containers
3. **Native UI Patterns** - WebUI API provides context menus, dialogs that feel native
4. **Clean Separation** - Core UI has full platform access, extensions are sandboxed

**Implementation Steps:**
1. **Restructure UI Code** - Move toolbar/settings from extensions/ to ui/core/
2. **Create Panel Container** - Core UI component that hosts extension content
3. **Enhance WebUI API** - Add native UI capabilities (context menus, file dialogs, etc.)
4. **Simplify Extension Loading** - Extensions just mount content, no chrome responsibility
5. **Implement IPC Router** - Message passing between extensions and core
6. **Add Dev Mode** - Hot reload for both core UI and extensions

**Goal**: VS Code-like architecture where core UI provides the shell and extensions add functionality within that shell.

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

1. **Clear UI Separation**: Core UI (toolbar, settings, panel chrome) is built-in, not extensions
2. **Extension Isolation**: Extensions run inside panel containers, can't modify core UI directly
3. **Native UI Patterns**: WebUI API enables native-feeling UIs (context menus, dialogs, etc.)
4. **HTTP-Served Extensions**: User extensions served from development servers for hot reload
5. **Central IPC Routing**: All extension communication flows through the workbench
6. **Modern Development**: Vite + Svelte + TypeScript for both core UI and extensions

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