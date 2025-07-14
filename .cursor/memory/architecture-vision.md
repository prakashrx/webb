# WebUI Platform - Architecture Vision

## Current State (January 2025)

### What We Have Built
- **Core Framework** (`WebUI.Core`):
  - `Screen` abstraction that combines BrowserWindow + WebUI API + IPC
  - `ScreenManager` for managing multiple screens
  - WebUI API injection via `AddScriptToExecuteOnDocumentCreatedAsync`
  - Virtual host mapping for serving UI assets
  - Clean, reusable infrastructure for any WebView2-based application

- **Workbench Application** (`WebUI.Workbench`):
  - Uses Core framework to create application screens
  - Built-in UI components (toolbar, settings) in `src/UI/workbench/`
  - Multi-screen support with message routing
  - No more "everything is an extension" - clear separation

- **Directory Structure**:
  ```
  src/
  ├── Platform/
  │   ├── Core/          # Framework (Screens, Hosting, Communication, API)
  │   └── Workbench/     # Application using the framework
  └── UI/
      ├── workbench/     # Workbench UI components
      └── api/           # WebUI JavaScript API
  ```

### Architecture Achievements
1. **Clean Abstraction**: Creating a screen is now just a few lines of code
2. **Automatic API Injection**: WebUI API available in all screens
3. **Proper Separation**: Core framework vs Application vs UI
4. **Multi-Screen Support**: Easy to create and manage multiple windows

## Future Vision

### 1. **Clear Separation: Core UI vs Extensions**
- **Core UI**: Built-in Svelte/Tailwind components that provide the application shell
  - Main toolbar (app branding, menus, window controls) ✅ Done
  - Settings screen (native preferences UI) ✅ Done
  - Panel containers (chrome/titlebar around extension content) 🔄 Next
  - Context menus (can overflow window bounds) 📋 Future
  
- **Extensions**: External plugins that run INSIDE panel containers
  - Provide content, not chrome
  - Cannot modify core UI directly
  - Communicate via WebUI API
  - Run in isolated iframes with extension identity

### 2. **WebUI API: Native UI Patterns**
The WebUI API should enable building native-feeling UIs from web technologies:

```javascript
// Core UI has full access
webui.window.minimize();
webui.window.setTitle("My App");
webui.contextMenu.show(items, { x, y });
webui.dialog.showOpenFile({ filters: [...] });
webui.notification.show("Build complete", { icon: "success" });

// Extensions have scoped access
webui.panel.setTitle("My Panel");
webui.commands.register("myext.refresh", () => {...});
webui.settings.contribute({
  "myext.apiUrl": {
    type: "string",
    default: "https://api.example.com"
  }
});
```

### 3. **Extension Contributions Model**
Extensions can contribute to core UI without owning it:

```javascript
// Extension manifest.json
{
  "contributes": {
    "commands": [
      {
        "command": "myext.refresh",
        "title": "Refresh Data",
        "icon": "refresh"
      }
    ],
    "menus": {
      "toolbar": [
        {
          "command": "myext.refresh",
          "when": "myext.active"
        }
      ]
    },
    "settings": {
      "myext.apiUrl": {
        "type": "string",
        "default": "https://api.example.com",
        "description": "API endpoint URL"
      }
    }
  }
}
```

### 4. **Panel System**
- Core provides panel containers with native chrome
- Extensions provide panel content
- Panels can be docked, floated, tabbed
- Panel state managed by core (size, position, visibility)

### 5. **IPC Architecture**
- Core UI ↔ Platform: Direct COM bridge
- Extensions ↔ Platform: Scoped IPC through panel container
- Extensions ↔ Extensions: Message bus with permissions

## Implementation Phases

### Phase 1: Core UI Foundation
1. Create `/ui/core/` directory structure
2. Build main toolbar component (Svelte)
3. Build settings screen component
4. Build panel container component
5. Implement basic WebUI API for core

### Phase 2: Enhanced WebUI API
1. Add native window controls
2. Add context menu support (with overflow)
3. Add file dialog support
4. Add notification support
5. Add keyboard shortcut registration

### Phase 3: Extension System
1. Define extension manifest schema
2. Build extension loader (simplified)
3. Implement contribution points
4. Add extension isolation (iframe with identity)
5. Create example extension

### Phase 4: Panel Management
1. Implement panel docking system
2. Add panel persistence
3. Support floating panels
4. Add panel tabs
5. Implement layout save/restore

## Benefits of This Architecture

1. **Cleaner Code Organization**
   - Core UI in `/ui/core/`
   - Extensions in `/extensions/`
   - Clear boundaries and responsibilities

2. **Better Developer Experience**
   - Core UI can use hot reload during development
   - Extensions get a stable API to build against
   - No confusion about what's core vs extension

3. **More Native Feel**
   - Context menus that overflow windows
   - Proper window controls
   - Native file dialogs
   - Keyboard shortcuts that work everywhere

4. **Easier to Maintain**
   - Core UI changes don't break extensions
   - Extensions can't break core UI
   - Clear upgrade path

## Example Structure

```
WebUI/
├── Core/                      # C# platform code
│   ├── Windows/              # WebView2 hosting
│   ├── Api/                  # COM bridges
│   └── Extensions/           # Extension management
├── ui/                       # Web UI code
│   ├── core/                 # Built-in UI components
│   │   ├── toolbar/         # Main toolbar
│   │   ├── settings/        # Settings screen
│   │   └── panel-container/ # Panel chrome
│   └── shared/              # Shared UI utilities
├── extensions/               # Extension code
│   └── example-extension/
└── webui-api/               # TypeScript API
```

## Next Steps

### Immediate (What We're Building Next)

1. **Panel Container System**
   - Create `PanelContainer.svelte` in workbench UI
   - Provides chrome (titlebar, tabs, resize handles)
   - Hosts extension content in iframes
   - Manages panel state (docked, floating, minimized)

2. **Workspace Screen**
   - Main application workspace that hosts panel containers
   - Golden Layout or similar for docking/splitting
   - Persists layout to JSON
   - Manages panel lifecycle

3. **Enhanced WebUI API**
   - Add `webui.contextMenu` for native menus
   - Add `webui.dialog` for file/folder selection
   - Add `webui.notification` for system notifications
   - Different API surface for core UI vs extensions

4. **Extension System (Simplified)**
   - Extensions are just web apps served over HTTP
   - Manifest defines contributions (commands, menus, settings)
   - Extensions run in iframes with scoped API access
   - No more complex loading - just iframe with URL

### Code Example of Target State

```typescript
// In Workbench UI (full access)
import { webui } from '@webui/api';

// Create context menu
webui.contextMenu.show([
  { label: 'Cut', command: 'edit.cut' },
  { label: 'Copy', command: 'edit.copy' },
  { label: 'Paste', command: 'edit.paste' }
], { x: 100, y: 200 });

// In Extension (scoped access)
webui.panel.setTitle('My Data Grid');
webui.commands.register('myext.refresh', async () => {
  const data = await fetchData();
  updateGrid(data);
});
```

### Architecture Principles Moving Forward

1. **Core UI is Sacred**: Only built-in components can modify the shell
2. **Extensions are Guests**: They run in sandboxed iframes with limited API
3. **Native Feel**: Use OS capabilities (context menus, dialogs) not web replacements
4. **Performance First**: Direct COM bridge for core, message passing for extensions
5. **Developer Joy**: Hot reload everywhere, clear APIs, good docs