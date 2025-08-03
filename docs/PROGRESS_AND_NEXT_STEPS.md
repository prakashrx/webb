# WebUI Experimental Progress and Next Steps

## Session Summary (2025-01-20)

### What We Accomplished

#### 1. **Dynamic Command Discovery System** ✅
- Moved from hardcoded commands to convention-based discovery
- Any class ending with `Commands` is auto-registered
- Methods become available as `className.methodName`
- Example:
  ```csharp
  public class WindowCommands
  {
      public async Task Minimize() { ... }
      public async Task<string> GetTitle() { ... }
  }
  ```
  Called as: `await invoke('window.minimize')`

#### 2. **Clean Architecture Separation** ✅
- Framework commands (WindowCommands) belong to WebUI.Desktop
- App-specific commands (TestCommands) belong to the app
- Both are auto-discovered at runtime
- No manual registration needed

#### 3. **Flexible TypeScript API** ✅
- Removed hardcoded command types
- Generic interface allows any command
- Future: Auto-generate TypeScript from C#

#### 4. **Automated Build Process** ✅
- WebUI API builds automatically via MSBuild
- No manual copying of files
- SDK handles everything

### Current Architecture

```
WebUI.Desktop (Framework)
├── Commands/
│   └── WindowCommands.cs    # Framework commands
├── Bridge/
│   ├── HostApiBridge.cs     # COM bridge
│   └── CoreApi.cs           # Invoke handler
└── WebUI.cs                 # Main entry point

HelloWorld (App)
├── TestCommands.cs          # App-specific commands
├── MainWindow.svelte        # UI
└── Program.cs              # WebUI.Run("MainWindow")

WebUI.Api (JavaScript/TypeScript)
├── src/
│   ├── core/invoke.ts      # Dynamic invoke function
│   └── types/commands.ts   # Generic command interface
└── dist/
    └── webui-api.bundle.js # Built and copied by MSBuild
```

### Key Design Decisions

1. **Convention over Configuration**
   - `*Commands` classes are auto-discovered
   - Public methods become commands
   - No attributes or registration needed

2. **Dynamic Invocation**
   - JavaScript: `invoke('className.methodName', args)`
   - No TypeScript definitions required (but can be generated)
   - Flexible and extensible

3. **Assembly Scanning**
   - Scans both WebUI.Desktop and the entry assembly
   - Framework and app commands coexist peacefully

## Next Steps (Based on Vision Document)

### Phase 1: Multi-Panel Support 🔴 (High Priority)

**Current State:**
```csharp
WebUI.Run("MainWindow");  // Single window only
```

**Target State:**
```csharp
var app = WebUI.Create();
app.Run("MainWindow");    // Can open multiple panels
```

**Implementation Steps:**
1. Refactor WebUI.cs to support multiple windows
2. Create PanelManager to track all open panels
3. Update WindowCommands to target specific panels
4. Enable panel-to-panel navigation

**JavaScript API Evolution:**
```javascript
// Current
await invoke('window.minimize');

// Future
webui.panel.minimize();
webui.panel.open('Settings', { data });
webui.panel.close();
```

### Phase 2: Builder Pattern API 🟡 (Medium Priority)

**Enable configuration and DI:**
```csharp
var builder = WebUI.CreateBuilder(args);
builder.Services.AddSingleton<IDataService, DataService>();
builder.WebUI.DefaultTheme = "dark";

var app = builder.Build();
app.MapPanel("MainWindow").Frameless().Size(1200, 40);
app.Run();
```

### Phase 3: Message Bus 🟡 (Medium Priority)

**Inter-panel communication:**
```javascript
// Panel A
webui.message.send('data-updated', { items: [...] });

// Panel B
webui.message.on('data-updated', (data) => {
    updateUI(data.items);
});

// Request/Response
const result = await webui.message.request('get-data', { filter: 'active' });
```

### Phase 4: TypeScript Generation 🟢 (Nice to Have)

**Auto-generate from C#:**
```typescript
// Generated webui.d.ts
interface WebUI {
  window: {
    minimize(): Promise<void>;
    maximize(): Promise<void>;
    getState(): Promise<WindowState>;
  };
  test: {
    echo(args: { message: string }): Promise<string>;
    getTime(): Promise<string>;
  };
}
```

## Technical Debt to Address

1. **Window Targeting**: Currently WindowCommands uses `Application.OpenForms[0]`
2. **Error Handling**: Need better error propagation from C# to JS
3. **Async Patterns**: Some commands return `VoidTaskResult` instead of proper values
4. **Hot Reload**: Only works for Svelte, not C# changes

## How to Continue

### To Run Current Implementation:
```bash
cd experiments/samples/HelloWorld
dotnet run
```

### To Add New Commands:
1. Create a class ending with `Commands`
2. Add public async methods
3. They're automatically available via `invoke()`

### To Work on Multi-Panel Support:
1. Start by creating a PanelManager class
2. Refactor WebUI.Run to WebUI.Create().Run()
3. Update HostApiBridge to support multiple WebView2 instances
4. Create panel lifecycle events

## Key Files to Review

- `/experiments/src/WebUI.Desktop/WebUI.cs` - Entry point
- `/experiments/src/WebUI.Desktop/Commands/CommandRegistry.cs` - Dynamic discovery
- `/experiments/src/WebUI.Api/src/core/invoke.ts` - JavaScript invoke
- `/experiments/samples/HelloWorld/TestCommands.cs` - Example app commands
- `/docs/EXPERIMENT_WEBUI_VISION.md` - The vision we're building towards

## Questions for Next Session

1. Should panels be separate processes or share the same process?
2. How should panel state be persisted (size, position)?
3. Should we support panel docking/tabbing?
4. What's the security model for inter-panel communication?

---

This document captures the current state and provides a roadmap for continuing development.
The experimental approach is working well - we have a clean, extensible architecture that
aligns with the vision of making desktop development as simple as web development.