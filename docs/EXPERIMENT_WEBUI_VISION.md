# WebUI Framework Vision - Experimental Prototype

## Executive Summary

WebUI is an experimental .NET library that brings the simplicity of WinForms and the modern builder pattern of ASP.NET Core to desktop application development using web technologies. Just add a NuGet package, drop in Svelte files, and run - no configuration required.

**Core Philosophy**: Make desktop development as simple as `WebUI.Run("MainWindow")` while leveraging modern web technologies.

## The Developer Experience

### Getting Started

WebUI is distributed as a single NuGet package that includes both the runtime library and build tools, providing a seamless development experience.

**Installation:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="WebUI.Desktop" Version="1.0.0" />
  </ItemGroup>
  
  <ItemGroup>
    <Panel Include="MainWindow.svelte" />
  </ItemGroup>
</Project>
```

**Program.cs:**
```csharp
using WebUI;

WebUI.Run("MainWindow");
```

**MainWindow.svelte:**
```svelte
<h1>Hello WebUI!</h1>
```

That's it. Run `dotnet run` and your desktop app launches. The SDK handles everything else automatically.

## Project Structure - Your Choice

**Minimal**
```
MyApp/
├── MyApp.csproj
├── Program.cs
└── MainWindow.svelte
```

**Organized by Feature**
```
MyApp/
├── MyApp.csproj
├── Program.cs
├── MainWindow.svelte
├── Settings.svelte
├── Editors/
│   ├── TextEditor.svelte
│   └── CodeEditor.svelte
└── Dialogs/
    ├── OpenFile.svelte
    └── SaveFile.svelte
```

**Any structure works** - just declare your Svelte files as `<Panel>` items in your project.

## Core Concepts

### 1. Zero Configuration

- No webpack.config.js
- No rollup.config.js  
- No tailwind.config.js
- No package.json in your project root
- No node_modules in your project root (kept in obj/ folder)

Everything is handled by the WebUI NuGet package through MSBuild integration. Node.js is only required during development for building assets. All Node.js artifacts are kept in the obj/ folder, maintaining a clean project structure.

### 2. Convention-Based Panel Discovery

- `MainWindow.svelte` → Panel ID: "MainWindow"
- `Settings.svelte` → Panel ID: "Settings"
- `Editors/TextEditor.svelte` → Panel ID: "Editors.TextEditor"

### 3. Builder Pattern API

**Minimal API**
```csharp
using WebUI;

var app = WebUI.Create(args);
app.Run("MainWindow");
```

**With Configuration**
```csharp
using WebUI;

var builder = WebUI.CreateBuilder(args);

// Add your services
builder.Services.AddSingleton<IDataService, DataService>();

// Configure WebUI
builder.WebUI.DefaultTheme = "dark";
builder.WebUI.EnableHotReload = true;

var app = builder.Build();

// Configure panels
app.MapPanel("MainWindow")
   .Frameless()
   .Size(1200, 40);

app.MapPanel("Settings", panel =>
{
    panel.Width = 600;
    panel.Height = 400;
    panel.Center();
});

app.Run();
```

## Technical Architecture

### NuGet Package Architecture

WebUI is distributed as a single NuGet package that includes both runtime and build tools, providing seamless integration with the .NET build system.

**Package Structure:**
```
WebUI.Desktop.nupkg
├── lib/
│   └── net9.0-windows/
│       └── WebUI.Desktop.dll     (runtime library)
├── build/
│   ├── WebUI.Desktop.props       (MSBuild properties)
│   └── WebUI.Desktop.targets     (build pipeline)
└── tools/
    ├── build/                    (Node.js build tools)
    │   ├── build-panel.js       (Svelte compiler)
    │   ├── base.css             (Tailwind base)
    │   └── package.json         (dependencies)
    └── WebUI.Api/               (TypeScript API source)
        ├── src/
        ├── package.json
        └── rollup.config.js
```

**Development Repository Structure:**
```
experiments/
├── src/
│   ├── WebUI.Desktop/           (combined runtime + SDK)
│   │   ├── build/              (MSBuild files)
│   │   ├── tools/              (build tools)
│   │   └── *.cs                (runtime code)
│   └── WebUI.Api/              (TypeScript API)
│       └── src/
├── samples/
│   └── HelloWorld/
│       ├── HelloWorld.csproj
│       ├── Program.cs
│       └── MainWindow.svelte
└── packages/                   (local NuGet output)
```

The package automatically:
- Sets up proper output types and target frameworks
- Includes WebView2 as a transitive dependency
- Configures implicit usings for WebUI namespace
- Compiles declared `<Panel>` items
- Manages the Node.js build pipeline transparently
- Builds WebUI.Api on-demand from source

### Build Pipeline

1. **MSBuild Integration**
   - SDK props/targets handle entire build process
   - Custom tasks for Svelte compilation
   - Uses local Node.js installation for development
   - **Production output has zero Node.js dependency**

2. **Resource Embedding**
   - Development: Served from file system with watchers
   - Production: All JavaScript/CSS embedded as assembly resources
   - No runtime dependency on Node.js or npm packages

3. **Auto-Injection**
   - WebUI JavaScript API
   - Tailwind CSS (purged automatically)
   - TypeScript definitions
   - Hot reload client (dev only)

### Runtime Architecture

```
┌─────────────────────────────────────────┐
│          Your .NET Application          │
│                                         │
│  ┌─────────────┐     ┌──────────────┐  │
│  │   Program   │     │  Your Services│  │
│  │             │     │  (Optional)   │  │
│  └──────┬──────┘     └──────┬───────┘  │
│         │                    │          │
│  ┌──────▼────────────────────▼───────┐  │
│  │         WebUI Runtime             │  │
│  │  ┌─────────────┐ ┌──────────────┐ │  │
│  │  │WindowManager│ │ Message Bus  │ │  │
│  │  └─────────────┘ └──────────────┘ │  │
│  │  ┌─────────────┐ ┌──────────────┐ │  │
│  │  │Panel Registry│ │Resource Server│ │ │
│  │  └─────────────┘ └──────────────┘ │  │
│  └────────────────┬─────────────────┘  │
│                   │                     │
└───────────────────┼─────────────────────┘
                    │
     ┌──────────────▼─────────────┐
     │      WebView2 Panels       │
     │  ┌──────┐ ┌──────┐ ┌──────┐│
     │  │Main  │ │Settings│ │About ││
     │  │Window│ │ Panel │ │Panel ││
     │  └──────┘ └──────┘ └──────┘│
     └────────────────────────────┘
```

## Svelte Development

### Basic Panel
```svelte
<!-- Settings.svelte -->
<script>
  let theme = 'dark';
  
  function save() {
    webui.message.send('settings-updated', { theme });
    webui.panel.close();
  }
</script>

<div class="p-6">
  <h1 class="text-2xl font-bold mb-4">Settings</h1>
  
  <label class="block mb-4">
    Theme:
    <select bind:value={theme} class="ml-2 px-2 py-1 border rounded">
      <option>light</option>
      <option>dark</option>
    </select>
  </label>
  
  <button onclick={save} class="px-4 py-2 bg-blue-500 text-white rounded">
    Save
  </button>
</div>
```

### Panel Communication
```svelte
<!-- DataPanel.svelte -->
<script>
  import { onMount } from 'svelte';
  
  let data = [];
  
  // Listen for updates
  webui.message.on('data-update', (newData) => {
    data = newData;
  });
  
  // Request data on mount
  onMount(async () => {
    const response = await webui.message.request('get-data');
    data = response.data;
  });
  
  // Open detail view
  function showDetails(item) {
    webui.panel.open('DetailView', { itemId: item.id });
  }
</script>
```

### Using .NET Services
```javascript
// Auto-generated service proxy
const dataService = webui.services.get('IDataService');
const items = await dataService.getItems();

// Or via messages (for complex operations)
const result = await webui.message.request('ProcessData', { 
  input: data 
});
```

## Advanced Features

### Hot Reload (Automatic in Development)
- Save a `.svelte` file
- Changes appear instantly
- No manual refresh needed
- State preserved when possible

### TypeScript Support (Automatic)
```typescript
// webui.d.ts is auto-generated
interface WebUI {
  panel: {
    open(name: 'Settings' | 'About' | 'DataPanel', data?: any): void;
    close(): void;
  };
  message: {
    send<T>(type: string, data: T, target?: string): void;
    on<T>(type: string, handler: (data: T) => void): void;
    request<T, R>(type: string, data?: T): Promise<R>;
  };
}
```

### Tailwind Integration (Automatic)
- Full Tailwind CSS available
- Automatically purged for production
- IntelliSense support in VS Code
- Custom config via `webui.json` if needed

### Adding Dependencies (Optional)
When you need additional npm packages, add them to `webui.json`:
```json
{
  "dependencies": {
    "chart.js": "^4.0.0",
    "date-fns": "^2.30.0",
    "@tanstack/svelte-table": "^8.0.0"
  },
  "tailwind": {
    "extend": {
      "colors": {
        "brand": "#007acc"
      }
    }
  }
}
```

The build system automatically:
- Generates package.json in `obj/` folder
- Installs dependencies in `obj/node_modules/`
- Bundles everything into your panels
- Keeps your project root clean

### Panel Lifecycle
```svelte
<script>
  import { onMount, onDestroy } from 'svelte';
  
  onMount(() => {
    // Panel opened
    webui.panel.setTitle('My Panel');
  });
  
  onDestroy(() => {
    // Panel closing
    webui.message.send('panel-closing');
  });
  
  // Prevent close
  webui.panel.onBeforeClose(() => {
    return confirm('Are you sure?');
  });
</script>
```

## Implementation Roadmap

### Phase 1: Core Prototype ✓
- [x] Basic WebView2 window management
- [x] Panel system with IPC
- [x] Message bus implementation
- [x] Manual Svelte compilation

### Phase 2: SDK Implementation (Current Focus)
- [ ] Create custom .NET SDK structure
- [ ] Implement Sdk.props and Sdk.targets
- [ ] Custom MSBuild tasks for Svelte compilation
- [ ] Node.js toolchain integration (dev-time only)
- [ ] Auto-discovery of .svelte files
- [ ] Resource embedding system
- [ ] Development/Production modes

### Phase 3: Developer Experience
- [ ] Hot reload via WebSockets
- [ ] Auto-generated TypeScript definitions
- [ ] Service proxy generation
- [ ] VS Code extension for IntelliSense
- [ ] Project templates

### Phase 4: Production Features
- [ ] AOT compilation support
- [ ] Single-file deployment
- [ ] Code signing integration
- [ ] Auto-updater support
- [ ] Crash reporting

## Success Metrics

1. **Time to Hello World**: < 2 minutes
2. **Lines of Code**: Minimal (5 lines for basic app)
3. **Build Time**: < 3 seconds for hot reload
4. **Package Size**: < 50MB for basic app
5. **Memory Usage**: Comparable to Electron

## Design Principles

1. **It Just Works™**: No configuration needed
2. **Progressive Complexity**: Start simple, add features as needed
3. **Familiar Patterns**: ASP.NET Core builder, DI, environments
4. **Web Standards**: Regular Svelte, standard Tailwind
5. **Fast Iteration**: Sub-second hot reload
6. **Production Ready**: AOT, single-file, signed executables

## Example Applications

### Text Editor (10 minutes)
```csharp
using WebUI;

var app = WebUI.Create(args);
app.MapPanel("Editor").Frameless();
app.Run("Editor");
```

### Database Browser (30 minutes)
```csharp
using WebUI;

var builder = WebUI.CreateBuilder(args);
builder.Services.AddSingleton<IDbConnection>(_ => 
    new SqlConnection(connectionString));

var app = builder.Build();

app.MapPanel("QueryEditor").Size(800, 600);
app.MapPanel("Results").Below("QueryEditor");

app.Run("QueryEditor");
```

### Multi-Window App (Complex)
```csharp
using WebUI;

var builder = WebUI.CreateBuilder(args);

builder.Services.AddSingleton<IDataService, DataService>();
builder.Services.AddHostedService<DataSyncService>();

builder.WebUI.ConfigureTheme(theme =>
{
    theme.Dark();
    theme.AccentColor = "#0066cc";
});

var app = builder.Build();

app.MapPanel("MainWindow")
   .Frameless()
   .AlwaysOnTop()
   .Size(1200, 40);

app.MapPanel("Dashboard", "Workspace", "Analytics")
   .DefaultSize(800, 600)
   .RememberPosition();

// Global message handling
app.MessageBus.Map<GetDataRequest, GetDataResponse>(
    async (request, services) =>
    {
        var dataService = services.GetRequiredService<IDataService>();
        var data = await dataService.GetDataAsync(request.Filter);
        return new GetDataResponse(data);
    });

app.Run();
```

## Conclusion

WebUI represents a fundamental shift in desktop application development - making it as simple as web development while maintaining the power and performance of native applications. By hiding complexity and providing smart defaults, developers can focus on building great user experiences instead of fighting with build tools and configuration.

The goal is simple: **From idea to running desktop app in under 5 minutes.**

---

*This is an experimental vision document. The actual implementation should prioritize simplicity and developer experience above all else.*