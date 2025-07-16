# WebUI Platform - Current State (January 2025)

## What We've Built

### Core Framework (WebUI.Core)

The Core framework provides a clean, reusable foundation for building WebView2-based applications:

1. **Screen Abstraction**
   - `IScreen` - Interface defining screen contract
   - `Screen` - Combines BrowserWindow + WebUI API + IPC in one abstraction
   - `ScreenOptions` - Configuration for creating screens
   - `ScreenManager` - Manages multiple screens and their lifecycle

2. **Key Features**
   - Automatic WebUI API injection via `AddScriptToExecuteOnDocumentCreatedAsync`
   - Virtual host mapping for serving UI assets
   - Built-in IPC communication
   - COM bridge for JavaScript ↔ C# interaction
   - Message routing between screens

3. **Usage Pattern**
   ```csharp
   var screen = await screenManager.CreateScreenAsync(new ScreenOptions
   {
       Id = "main-toolbar",
       Title = "WebUI Platform",
       Width = 1200,
       Height = 40,
       IsFrameless = true,
       UiModule = "workbench",
       PanelId = "main-toolbar"
   });
   screen.Show();
   ```

### Workbench Application (WebUI.Workbench)

The Workbench is an application built on top of the Core framework:

1. **Multi-Screen Support**
   - Main toolbar (frameless, always on top)
   - Settings screen (standard window)
   - Workspace screen (main application area)

2. **Built-in UI Components**
   - Located in `src/UI/workbench/`
   - Svelte + Tailwind CSS
   - Not extensions - they're part of the application

3. **Message Handling**
   ```csharp
   _screenManager.MessageReceived += (sender, message) => {
       switch (message.Type)
       {
           case "open-settings":
               OpenSettingsScreen();
               break;
           case "open-workspace":
               OpenWorkspaceScreen();
               break;
       }
   };
   ```

### Directory Structure

```
src/
├── Platform/
│   ├── Core/
│   │   ├── Hosting/        # BrowserWindow, WebViewHost
│   │   ├── Screens/        # Screen, ScreenManager, IScreen
│   │   ├── Api/            # HostApiBridge, PanelApi, IpcApi
│   │   └── Communication/  # IpcTransport
│   └── Workbench/
│       └── Extensions/     # ExtensionHost, ExtensionManager (legacy)
└── UI/
    ├── workbench/          # Workbench UI components
    │   ├── src/
    │   │   ├── panels/     # MainToolbar.svelte, Settings.svelte
    │   │   └── activate.js # UI module entry point
    │   └── dist/           # Built output
    └── api/                # WebUI JavaScript API
        └── dist/
            └── webui-api.js
```

## Key Architecture Decisions

1. **Moved Away from "Everything is an Extension"**
   - Core UI components are built-in, not extensions
   - Clear separation between framework and application
   - Extensions will run inside panel containers (future)

2. **Composition Over Inheritance**
   - Screen uses BrowserWindow via composition
   - ScreenManager orchestrates multiple screens
   - Clean separation of concerns

3. **WebUI API Injection**
   - Injected via code, not HTML
   - Available before page loads
   - Consistent across all screens

## What's Working

1. **Screen Creation** - Easy to create new screens with different configurations
2. **WebUI API** - JavaScript API successfully injected and available
3. **Virtual Host Mapping** - `http://webui.local/` serves UI assets correctly
4. **IPC Communication** - Messages flow between JavaScript and C#
5. **Multi-Window Support** - Multiple screens can be opened and managed

## Known Issues Resolved

1. **White Screen Issue** - Fixed by injecting WebUI API before page load
2. **Extension System Complexity** - Removed in favor of built-in UI components
3. **Directory Structure** - Reorganized for clarity

## Next Steps

1. **Panel Container System** - Create containers for hosting extension content
2. **Workspace Layout** - Implement docking/splitting for panels
3. **Enhanced WebUI API** - Add context menus, dialogs, notifications
4. **Extension System** - Simple iframe-based extensions with manifest

## Technical Stack

- **.NET 9.0** with Windows Forms
- **WebView2** for web rendering
- **Svelte + Vite** for UI components
- **TypeScript** for WebUI API
- **Tailwind CSS** for styling

## Development Workflow

```bash
# Build everything
cd src/Platform
dotnet build

# Run the application
cd src/Platform/Workbench
dotnet run

# Development mode for UI (with hot reload)
cd src/UI/workbench
npm run dev
```

## Architecture Benefits

1. **Reusable Core** - Any application can use the Screen framework
2. **Clear Boundaries** - Framework vs Application vs UI
3. **Easy Testing** - Each layer can be tested independently
4. **Future-Proof** - Ready for extension system without major refactoring