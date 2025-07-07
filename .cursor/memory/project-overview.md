# WebUI Project Overview - Modular Desktop Framework for Trading

## Project Naming & Structure Decision
- **Framework Name**: WebUI 
- **Main Project**: WebUI (serves as the Control App)

## Project Type
- **Technology**: Multi-process C# application with WebView2 and Svelte
- **Target**: Windows Desktop Trading Framework
- **Architecture**: Modular, plugin-based system

## Project Vision
Designing the Ultimate Modular Desktop Framework for Trading - a simple, fast, and flexible system with clean separation of concerns.

## Key Use Case: High-Performance Grid UI

### Trading Data Visualization Challenge
- **Problem**: Display millions of rows of real-time trading data without performance degradation
- **Solution**: Virtual paging + reactive streaming pattern with C# backend + AG Grid in Svelte

### Architecture Pattern for Data-Heavy Panels
- **Backend (C#)**: Owns the data, stores millions of rows in memory-resident columnar format
- **Frontend (Svelte Panel)**: Acts as a "viewport" over the dataset, only loads visible rows
- **Communication**: Two-way but minimal:
  - **Pull**: `getRows(start, count)` - Frontend requests visible row slices
  - **Push**: `pushUpdate(rowIndex, newData)` - Backend pushes real-time updates for visible rows only

### Implementation in Our Framework
- **Svelte Panel**: High-performance grid component using AG Grid with server-side row model
- **window.host API**: Provides `getRows()` and `pushUpdate()` methods
- **Plugin System**: Easy to create different grid-based trading tools (price tables, order books, etc.)
- **Performance**: No JSON overhead, binary formats (Apache Arrow), batched updates

### Benefits for Trading Framework
- ✅ Millions of rows without lag or memory bloat
- ✅ Real-time updates in-place without redraws  
- ✅ Backend remains authoritative data source
- ✅ Native-like performance in WebView
- ✅ Modular - one component among many in workspace

## Modular Infrastructure Architecture

### Core Libraries (Building Blocks)

#### 1. WebView+Native Framework Library
- **Purpose**: Common lightweight framework for both Control App and Host processes
- **Features**: WebView management, native window handling, common utilities
- **Used by**: Control App, Host processes
- **Focus**: High performance, reusable components

#### 2. Communication Library 
- **Purpose**: Handle host↔control app↔host bidirectional communication
- **Features**: Message routing, process management, event handling
- **Communication Models**: Different models for different use cases
- **Performance**: Separate library for optimal performance

#### 3. C# ↔ WebView Bridge Library
- **Purpose**: Simple abstraction for C# to WebView communication
- **API**: Simple `send()` / `on()` pattern on both sides
- **Format**: JSON message passing (abstracted from developer)
- **Goal**: Make it easy to dev without dealing with WebView complexity

### Application Components

#### Control App (WebUI)
- **Base**: WebView+Native Framework Library
- **UI**: Top panel ribbon interface (Svelte-based)
- **Role**: Coordination, workspace management, host process launching

#### Host Processes (WebUI.Host)
- **Base**: WebView+Native Framework Library
- **UI Framework**: Golden Layout for dockable panels
- **Role**: Plugin containers, isolated processes
- **Communication**: Via Communication Library

#### Plugin System
- **Components**: Svelte components with manifests
- **Runtime**: Runs in Host processes
- **Communication**: Message passing through Bridge Library

### Implementation Strategy
- **Build Basic Blocks First**: Focus on core libraries and infrastructure
- **Pooling**: Keep in mind but implement later
- **Performance**: High performance is KEY throughout
- **Modularity**: Each component is a separate, reusable library

## Implementation Decisions

### Control App UI
- **Style**: Top panel at top of desktop (draggable, modern ribbon interface)
- **Size**: Dynamic based on menu/ribbon content
- **Container**: Form1 renamed to Main, hosts WebView2 control
- **Framework**: Uses WebView+Native Framework Library

### CRITICAL: On-Demand WebView Infrastructure
- **Current Focus**: Build basic blocks for modular infrastructure
- **Future**: Pooled WebView system for on-demand popup panels
- **Requirements**: Desktop-wide positioning, performance pooling, seamless communication

### WebView Panel Pool System
- **Pool Manager**: C# service that manages WebView instances
- **On-Demand Creation**: Create panels when needed (right-click, dropdown, etc.)
- **Desktop Positioning**: Panels can appear anywhere on screen
- **Communication**: Message passing between main WebView and popup WebViews
- **Performance**: Reuse instances to avoid creation overhead

### Project Structure  
- **WebUI** - Control App (main project)
- **WebUI.Host** - Separate project for plugin containers
- **WebUI.Web** - Web assets (Svelte components, Vite build)

### Web Development Stack
- **Package Manager**: pnpm
- **Build Tool**: Vite  
- **Frontend**: Svelte + Tailwind CSS
- **Container**: WebView2 in both Control App and Host processes

## Architecture Components

### 1. Control App (C# + WebView2 + Svelte) - WebUI Project
- **Purpose**: Coordination and management with minimal floating toolbar
- **UI**: Floating toolbar interface built with Svelte + Tailwind
- **Form**: Main.cs (renamed from Form1) with WebView2 control
- **Responsibilities**:
  - Loading and saving workspaces
  - Launching WebUI.Host processes
  - Managing layout and menus
  - Routing commands
  - Plugin discovery and integration
  - Workspace management (JSON-based)
- **UI Features**: Save/Load workspace, core menus, all web UI driven

### 2. Host Processes (C# + WebView2) - WebUI.Host Project  
- **Purpose**: Individual plugin container processes
- **Characteristics**:
  - Separate executable launched by Control App
  - Separate C# processes for individual plugins/panels
  - No window borders (borderless)
  - Embeds Golden Layout-based interface through WebView2
  - Renders multiple Svelte panels
  - "Dumb" hosts - just show panels, don't manage logic
  - Better performance, isolation, and flexibility

### 3. Panels (Svelte Components)
- **Purpose**: Individual UI components/tools
- **Development**: 
  - Built using Svelte components
  - Plugin creation requires only `.svelte` file + manifest
  - Fully web-based, lightweight, and reactive
  - Access to `window.host` APIs

### 4. Plugin System
- **Structure**:
  - Svelte component + small manifest file
  - Manifest declares: commands, menu items, default config, panel metadata
  - Automatic discovery and integration
  - No native code required for developers

### 5. Workspace Management
- **Format**: JSON-based
- **Content**: Panel arrangement, configuration, metadata
- **Features**: Portable, easy to load/save/share
- **Host-agnostic**: Workspaces don't care which host renders them

### 6. Development Environment
- **Hot Reload**: Vite dev server integration
- **Testing**: Real WebView environment with full API access
- **No Mocking**: Direct localhost loading into actual host app

## Current State
- Basic Windows Forms project structure (needs conversion)
- Form1 needs to be renamed to Main and converted to WebView2 container
- Need to create WebUI.Host project
- Need to create WebUI.Web project with pnpm + Vite + Svelte + Tailwind

## API System
- `window.host` provides: file system access, messaging, commands, networking
- Structured message communication between panels and control app
- APIs injected by host process

## Design Principles
- Clean separation of concerns
- Each component does one job well
- Stay out of the way
- Easy to extend
- Minimal and focused

## Project Structure
```
WebUI/
├── WebUI.sln (Solution file)
└── WebUI/
    ├── WebUI.csproj (Project file)
    ├── Program.cs (Application entry point)
    ├── Form1.cs (Main form code)
    └── Form1.Designer.cs (UI designer code)
```

## Next Steps Needed
- Define the purpose and functionality of this WebUI application
- Determine what controls and features to add
- Plan the user interface design and user experience

## Notes
- The borderless, controlbox-free design suggests this might be:
  - A custom overlay application
  - A kiosk-style application
  - A widget or dashboard
  - A game launcher or media interface 

## Implementation Roadmap

### Phase 1: WebView+Native Framework Library (B)
- **Priority**: First - foundation for windowing and WebView management
- **Purpose**: Common lightweight framework for both Control App and Host processes
- **Focus**: High performance WebView management, native window handling

### Phase 2: C# ↔ WebView Bridge Library (A) - Two Parts
#### Part 1: The Bridge
- **Purpose**: Simple abstraction for C# to WebView communication
- **API**: Simple `send()` / `on()` pattern on both sides
- **Format**: JSON message passing (abstracted from developer)

#### Part 2: Core APIs using the Bridge
- **File System API**: `fs` - file operations for extensions
- **I/O API**: `io` - general input/output operations
- **IPC API**: `ipc` - inter-plugin communication system
- **Goal**: Well-designed APIs for extensions to interact with Control App

### Phase 3: Control App Implementation (C)
- **Base**: Uses WebView+Native Framework Library
- **UI**: Svelte + Tailwind web view menu
- **Features**: VS Code-style extension system

## POC Specification - VS Code-Style Extension System

### Control App Features
- **Menu System**: Svelte + Tailwind web view interface
- **Workspace Management**: Open/Save workspace functionality
- **Extension Management**: VS Code-style extension panel
  - Search available extensions
  - Install/uninstall extensions
  - Extension marketplace interface
- **Settings System**: VS Code-style settings interface
- **Keyboard Shortcuts**: Configurable shortcuts system

### Extension System
- **Extension Types**:
  - **Panel Extensions**: Install a set of dockable panels
  - **Behavioral Extensions**: Add functionality without UI
- **Extension Installation**: Adds new commands to menu/shortcuts
- **Extension Discovery**: Search extensions in toolbar
- **Extension APIs**: Use well-designed APIs (fs, io, ipc) to interact with Control App
- **Extension Powers**: Can change layout, looks, behavior

### Host Panel Integration
- **Panel Loading**: Extension panels load into new/existing host processes
- **Docking System**: Golden Layout for panel management
- **Process Isolation**: Each host process runs independently
- **Communication**: Extensions use IPC API to communicate with other plugins

### Implementation Strategy
- **Build Basic Blocks First**: Focus on core libraries and infrastructure
- **Pooling**: Keep in mind but implement later
- **Performance**: High performance is KEY throughout
- **Modularity**: Each component is a separate, reusable library