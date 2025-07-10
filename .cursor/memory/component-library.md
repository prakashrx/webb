# Extension UI Patterns - Component Strategy

## 🎯 Extension-First Architecture

**Core Philosophy**: Everything is an extension. Components are built within extensions to create rich, modular UIs that integrate with the `window.host` API.

**Who builds components**: Extension developers creating trading panels, workspace management, settings UIs, etc.

## 🏗️ Extension Component Model

### Extension Structure
```
extensions/
├── toolbar/                    # Core toolbar extension
│   ├── manifest.json          # Extension metadata
│   ├── src/
│   │   ├── ToolbarPanel.svelte # Main UI component
│   │   └── activate.js         # Extension entry point
│   └── package.json
├── market-data/               # Market data extension
│   ├── manifest.json
│   ├── src/
│   │   ├── PriceGrid.svelte   # High-performance data grid
│   │   ├── ChartPanel.svelte  # Trading charts
│   │   └── activate.js
│   └── package.json
└── workspace/                 # Workspace management extension
    ├── manifest.json
    ├── src/
    │   ├── WorkspacePanel.svelte
    │   └── activate.js
    └── package.json
```

### Host API Integration
Extensions use the `window.host` API to:
- Register commands and handle events
- Communicate with other extensions
- Access core services (DOT tables, storage, etc.)
- Manage panel lifecycle

```javascript
// activate.js - Extension entry point
export function activate(context) {
    // Register commands
    const disposable = host.commands.registerCommand('market-data.refresh', () => {
        // Handle refresh command
    });
    
    // Create panel
    const panel = host.window.createPanel('market-data', {
        title: 'Market Data',
        component: 'PriceGrid.svelte'
    });
    
    // Subscribe to data events
    host.events.on('market.price-update', (data) => {
        panel.postMessage('price-update', data);
    });
    
    context.subscriptions.push(disposable);
}
```

## 🎨 UI Component Patterns

### 1. Panel Components
**Purpose**: Main UI containers that extensions register as panels

```svelte
<!-- PriceGrid.svelte -->
<script>
    import { onMount } from 'svelte';
    
    let priceData = [];
    
    onMount(() => {
        // Subscribe to host events
        host.events.on('price-update', (data) => {
            priceData = data.prices;
        });
        
        // Request initial data
        host.commands.executeCommand('market-data.get-prices');
    });
</script>

<div class="price-grid">
    {#each priceData as price}
        <div class="price-row">
            <span class="symbol">{price.symbol}</span>
            <span class="price">{price.last}</span>
        </div>
    {/each}
</div>
```

### 2. Toolbar Components  
**Purpose**: Extensions that contribute to the main application toolbar

```svelte
<!-- ToolbarPanel.svelte -->
<script>
    function handleMenuClick(item) {
        // Execute host command
        host.commands.executeCommand(`toolbar.${item.action}`);
    }
</script>

<div class="toolbar">
    <div class="logo">
        <span class="status-indicator active"></span>
        Trading Platform
    </div>
    
    <nav class="menu">
        {#each menuItems as item}
            <button on:click={() => handleMenuClick(item)}>
                {item.label}
            </button>
        {/each}
    </nav>
    
    <button class="close-btn" on:click={() => host.commands.executeCommand('app.close')}>
        ×
    </button>
</div>
```

### 3. High-Performance Data Components
**Purpose**: Components that handle real-time data streams efficiently

```svelte
<!-- DataGrid.svelte -->
<script>
    import { onMount } from 'svelte';
    
    let rows = [];
    let viewport = { start: 0, end: 100 };
    
    onMount(() => {
        // High-performance binary data channel
        host.binary.onMessage('market-data', (buffer) => {
            // Process Arrow format or other binary data
            const newRows = deserializeRows(buffer);
            updateVisibleRows(newRows);
        });
    });
    
    function updateVisibleRows(newRows) {
        // Update only visible rows for performance
        rows = newRows.slice(viewport.start, viewport.end);
    }
</script>

<div class="data-grid" bind:this={gridElement}>
    <!-- Virtualized grid rendering -->
</div>
```

## 🔧 Development Workflow

### Extension Development Process
1. **Create Extension Directory**: Set up manifest and project structure
2. **Develop Svelte Components**: Build UI using modern Svelte patterns
3. **Integrate Host API**: Connect to commands, events, and services
4. **Test in Host Environment**: Run extension in actual WebView2 host
5. **Package & Deploy**: Bundle extension for distribution

### Build Configuration
Extensions use their own build process:
```json
{
  "name": "market-data-extension",
  "scripts": {
    "build": "rollup -c",
    "dev": "rollup -c -w"
  },
  "devDependencies": {
    "rollup": "^3.0.0",
    "rollup-plugin-svelte": "^7.0.0"
  }
}
```

## 📦 Core Extension Examples

### 1. MainToolbar Extension
- **Purpose**: Main application toolbar with workspace management
- **Components**: ToolbarPanel.svelte
- **Host Integration**: Window management commands, workspace events

### 2. MarketData Extension  
- **Purpose**: Real-time price displays and trading grids
- **Components**: PriceGrid.svelte, ChartPanel.svelte
- **Host Integration**: Binary data streams, DOT table subscriptions

### 3. Settings Extension
- **Purpose**: Application configuration and preferences
- **Components**: SettingsPanel.svelte, ConfigForm.svelte
- **Host Integration**: Configuration API, storage service

### 4. Workspace Extension
- **Purpose**: Workspace management and panel layout
- **Components**: WorkspacePanel.svelte, LayoutManager.svelte
- **Host Integration**: Panel management, workspace persistence

## 🎯 Design Principles

### Extension-Centric
- Every UI element is owned by an extension
- Extensions communicate via host API, not direct coupling
- Extensions can be developed, tested, and deployed independently

### Performance-Focused
- Components handle high-frequency data efficiently
- Binary data channels for real-time streams
- Virtualized rendering for large datasets

### VS Code-Inspired
- Familiar extension model for developers
- Consistent API patterns across all extensions
- Rich marketplace potential for third-party extensions

## 🚀 Next Steps

### Phase 1: Core Extension Framework
- [ ] Define extension manifest format
- [ ] Implement extension loader in C#
- [ ] Create `window.host` API injection

### Phase 2: Foundation Extensions
- [ ] Convert MainToolbar to extension
- [ ] Build Settings extension
- [ ] Create Workspace management extension

### Phase 3: Trading Extensions
- [ ] High-performance data grid extension
- [ ] Chart panel extension
- [ ] Order management extension

---

*Building a modular, extensible trading platform where every UI element is a plugin that can be developed, tested, and deployed independently.* 