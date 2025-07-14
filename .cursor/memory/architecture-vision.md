# WebUI Platform - Architecture Vision

## Core Principles

### 1. **Clear Separation: Core UI vs Extensions**
- **Core UI**: Built-in Svelte/Tailwind components that provide the application shell
  - Main toolbar (app branding, menus, window controls)
  - Settings screen (native preferences UI)
  - Panel containers (chrome/titlebar around extension content)
  - Context menus (can overflow window bounds)
  
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

1. Update CLAUDE.md with this vision
2. Restructure the codebase
3. Build core UI components
4. Enhance WebUI API
5. Simplify extension system