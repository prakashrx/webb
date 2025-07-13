# WebUI Platform - Project Structure

## Overview

**WebUI Platform** is a VS Code-style extensible desktop shell for building real-time trading applications. It provides a minimal C# runtime with WebView2-based panels and a clean JavaScript API for extensions.

**Current Status**: Early implementation phase with basic WebView2 hosting and window management in place.

---

## Architecture Philosophy

### Core Principles
- **Extension-First**: Everything (toolbars, panels, forms) is implemented as an extension
- **HTTP-Served Extensions**: Extensions are served via HTTP with live reload for development
- **Multi-WebView**: Each extension can run in isolated WebView2 processes with Golden Layout
- **IPC-Based Communication**: Unified messaging system for local and cross-process communication
- **Modern Development**: Vite + Svelte + TypeScript for extension development

### Key Components
1. **Workbench**: Central coordinator (formerly Shell)
2. **Host Processes**: WebView2 containers for extensions
3. **Extension System**: Manifest-based loading with lifecycle management
4. **IPC System**: Message routing between processes and extensions

---

## Directory Structure

```
WebUI/
├── WebUI.Platform.sln              # Solution file
├── PROJECT_STRUCTURE.md            # This document
│
├── Workbench/                      # Main application process
│   ├── WorkbenchEntry.cs           # Main form and app lifecycle
│   ├── Program.cs                  # Application entry point
│   └── WebUI.Workbench.csproj     # Project file
│
├── Core/                           # Core platform libraries
│   ├── Windows/                    # Window and WebView2 management
│   │   ├── BrowserWindow.cs        # ✅ Main WebView2 window wrapper
│   │   ├── WebViewHost.cs          # ✅ WebView2 hosting infrastructure
│   │   ├── WebViewOptions.cs       # ✅ WebView2 configuration
│   │   ├── WindowControls.cs       # ✅ COM object for JS window controls
│   │   ├── WindowHelper.cs         # ✅ Window utility functions
│   │   └── IWebViewHost.cs         # ✅ WebView hosting interface
│   │
│   ├── IPC/                        # ⏳ Inter-process communication (planned)
│   │   └── README.md               # Placeholder
│   │
│   ├── Extensions/                 # ⏳ Extension system (planned)
│   │   └── README.md               # Extension infrastructure plans
│   │
│   ├── GlobalUsings.cs             # ✅ Global using statements
│   └── WebUI.Core.csproj           # ✅ Core project file
│
├── Shared/                         # Shared contracts and models
│   ├── Contracts/                  # ✅ API interface definitions
│   │   ├── IPanel.cs               # ✅ Panel interface
│   │   ├── IWindowApi.cs           # ✅ Window API interface
│   │   ├── ICommandsApi.cs         # ✅ Commands API interface  
│   │   ├── IEventsApi.cs           # ✅ Events API interface
│   │   ├── IServicesApi.cs         # ✅ Services API interface
│   │   ├── IExtension.cs           # ✅ Extension interface
│   │   └── IExtensionContext.cs    # ✅ Extension context interface
│   │
│   ├── Models/                     # ⏳ Data models (planned)
│   ├── GlobalUsings.cs             # ✅ Global using statements
│   └── WebUI.Shared.csproj         # ✅ Shared project file
│
├── Host/                           # ⏳ Extension host processes (planned)
├── Extensions/                     # ⏳ Built-in extensions (planned)  
├── UIComponents/                   # ⏳ Svelte component library (planned)
└── Tools/                          # ⏳ CLI tools and scaffolding (planned)
```

**Legend**: ✅ Implemented | ⏳ Planned | 🔄 In Progress

---

## Current Implementation Status

### ✅ **Completed Components**

#### **Workbench (Main Application)**
- **WorkbenchEntry.cs** (317 lines): Main application form and lifecycle management
  - Creates frameless WebView2 window with custom title bar
  - Demo HTML with window controls integration
  - Basic window management (minimize, maximize, close)
  - Application initialization and cleanup
- **Program.cs** (21 lines): Application entry point with proper threading

#### **Core/Windows (WebView2 Infrastructure)**  
- **BrowserWindow.cs** (217 lines): Complete WebView2 window wrapper
  - Tauri-inspired API: `new BrowserWindow(title, width, height, options)`
  - HTML/URL loading, JavaScript execution, host object injection
  - Window operations: show/hide, position, size, minimize/maximize
  - Event handling for close and message events
- **WebViewHost.cs** (182 lines): WebView2 hosting infrastructure
  - WebView2 initialization and lifecycle management
  - Message passing between JS and C#
  - Host object registration for COM interop
  - Navigation and script execution
- **WebViewOptions.cs** (52 lines): WebView2 configuration options
- **WindowControls.cs** (136 lines): COM object for JavaScript window controls
  - Exposes minimize, maximize, restore, close to JavaScript
  - Used in demo for custom title bar functionality
- **WindowHelper.cs** (101 lines): Window utility functions
- **IWebViewHost.cs** (50 lines): WebView hosting interface

#### **Shared Contracts**
- **Complete API Interface Definitions**: All major APIs defined
  - `IPanel`, `IWindowApi`, `ICommandsApi`, `IEventsApi`, `IServicesApi`
  - `IExtension`, `IExtensionContext` for extension system
  - Ready for implementation in Phase 3

### ⏳ **Planned Components (Not Yet Implemented)**

#### **IPC System**  
- Memory-mapped file transport for high-performance communication
- Named pipe transport for development and fallback
- Message routing between extensions and Workbench
- Unified `webui.ipc` API for local and cross-process messaging

#### **Extension System**
- Extension manifest loading and validation
- Extension lifecycle management (load, activate, deactivate, unload)
- Extension isolation with separate WebView2 processes
- Hot reload for development

#### **Host Processes**
- Separate processes for extension isolation
- Golden Layout integration for panel docking
- Extension runtime with injected `webui.api`

#### **UI Components**
- Svelte component library with Tailwind CSS
- Vite-based build system for fast development
- Component documentation and examples

#### **Tools & CLI**
- Extension scaffolding tools
- Development server for extensions
- Build and packaging utilities

---

## Key Classes and Components

### **WorkbenchEntry** (Main Application)
```csharp
public partial class WorkbenchEntry : Form
{
    private BrowserWindow? _browserWindow;
    
    // Application lifecycle, WebView2 demo window creation
    // Currently shows basic window with custom title bar
}
```

### **BrowserWindow** (WebView2 Wrapper)
```csharp
public sealed class BrowserWindow : IDisposable
{
    // Clean API for creating and managing WebView2 windows
    // Methods: Navigate, LoadHtml, Evaluate, AddHostObject
    // Events: Closed, MessageReceived
}
```

### **WebViewHost** (WebView2 Infrastructure)
```csharp
public sealed class WebViewHost : IWebViewHost
{
    // Low-level WebView2 management
    // Handles initialization, options, host objects
    // Message passing and script execution
}
```

### **WindowControls** (JavaScript Bridge)
```csharp
[ComVisible(true)]
public class WindowControls
{
    // COM object exposed to JavaScript
    // Provides window.windowControls.minimize(), etc.
    // Used in demo's custom title bar
}
```

---

## Technology Stack

### **Backend (.NET)**
- **.NET 9.0**: Modern C# with latest language features
- **Windows Forms**: Native Windows UI framework
- **WebView2**: Microsoft Edge WebView for web content
- **COM Interop**: JavaScript ↔ C# communication

### **Frontend (Planned)**
- **Svelte**: Reactive component framework
- **TypeScript**: Type-safe JavaScript development  
- **Tailwind CSS**: Utility-first CSS framework
- **Vite**: Fast build tool with hot reload

### **Development Workflow (Planned)**
- **HTTP-Served Extensions**: Extensions served from local dev server
- **Live Reload**: Automatic updates during development
- **Manifest-Based**: Declarative extension configuration
- **Golden Layout**: Professional docking and layout system

---

## API Architecture (Planned)

### **webui.api Surface**
```typescript
// Panel & Workspace Management
webui.panel.registerView(id, provider)      // Low-level panel registration
webui.panel.registerPanel(id, component)    // Svelte component wrapper
webui.panel.open(id, options)              // Open and dock panels
webui.workspace.save(name)                  // Workspace persistence

// Universal Messaging (Local + Cross-Process)
webui.ipc.on(type, handler)                 // Register message handlers
webui.ipc.send(type, payload)               // Send to platform
webui.ipc.send(target, type, payload)       // Send to specific extension
webui.ipc.broadcast(type, payload)          // Send to all extensions

// Platform Services
webui.ui.showQuickPick(items)              // Platform UI helpers
webui.services.marketData.subscribe(...)    // Domain-specific services
webui.keyboard.registerShortcut(...)        // Keyboard bindings
webui.clipboard.readText()                  // System integration
```

### **Extension Manifest**
```json
{
  "id": "my-extension",
  "displayName": "My Extension", 
  "version": "1.0.0",
  "main": "dist/activate.js",
  "panels": [
    { "id": "main", "title": "My Panel", "defaultDock": "right" }
  ]
}
```

### **Extension Activation**
```javascript
import MyPanel from './MyPanel.svelte';

export function activate(context) {
  webui.panel.registerPanel("main", MyPanel);
  webui.panel.open("main");
  
  webui.ipc.on("my.command", (data) => {
    // Handle command
  });
}
```

---

## Development Status

### **Current Phase**: Foundation Implementation
- Basic WebView2 hosting ✅
- Window management and controls ✅  
- API contracts defined ✅
- Demo application working ✅

### **Next Phase**: Core API Implementation
- IPC transport layer implementation
- HostApiBridge C# ↔ JavaScript API
- Basic extension loading system
- Workbench as extension coordinator

### **Future Phases**
- Extension isolation and multi-process architecture
- Golden Layout integration for docking
- Svelte component library and build system
- CLI tools and developer experience improvements
- Production-ready extension marketplace

---

## Getting Started

### **Current Demo**
```bash
# Build and run the current demo
cd WebUI/Workbench
dotnet run
```

This launches a frameless window with custom title bar demonstrating:
- WebView2 integration
- JavaScript ↔ C# communication via COM
- Custom window controls (minimize, maximize, close)
- Modern UI with backdrop blur effects

### **Development Setup (Planned)**
```bash
# Create new extension
webui create-extension my-extension

# Start development server  
cd my-extension
npm run dev

# Extension available at http://localhost:3001
# Load in WebUI for live development
```

---

*This document is maintained and updated after each development task to reflect the current state of the project.* 