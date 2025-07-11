# WebUI Components Library

A modular Svelte component library for the WebUI Trading Framework, inspired by VS Code's extensible design.

## 🎯 Philosophy

- **Individually Loadable**: Each component compiles to its own JS file for on-demand loading
- **Web Components**: Universal compatibility with any host environment (C# WebView2, browser, etc.)
- **VS Code Inspired**: Clean, professional design language that users already know
- **Event-Driven**: Components communicate via custom events that C# can intercept
- **Modular Architecture**: Components organized by functionality and reusability

## 🚀 Quick Start

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build
```

## 📁 Project Structure

```
WebUI.Components/
├── src/
│   ├── core/           # Basic UI primitives
│   ├── layout/         # Layout and container components
│   ├── forms/          # Form controls and inputs
│   ├── menus/          # Navigation and menu components
│   ├── trading/        # Trading-specific components
│   └── TestButton.svelte # Simple test component
├── public/
│   ├── index.html      # Test page
│   └── *.js            # Compiled components (generated)
├── package.json
├── rollup.config.js
└── README.md
```

## 🧩 Planned Component Architecture

### Core Components (`src/core/`)
Basic UI primitives that form the foundation:

- **Button** - Primary, secondary, danger variants with sizing
- **Icon** - SVG icon system with VS Code icon set
- **Badge** - Status indicators, counters, labels
- **Tooltip** - Contextual help and information
- **Spinner** - Loading indicators
- **ProgressBar** - Progress indication for operations
- **Checkbox** - Boolean input controls
- **RadioButton** - Single-choice selection
- **Toggle** - On/off switches

### Layout Components (`src/layout/`)
Container and layout management:

- **Panel** - Resizable, dockable panels
- **TabStrip** - Tab navigation with close buttons
- **SplitView** - Horizontal/vertical split containers
- **StatusBar** - Bottom status display
- **Toolbar** - Top action bars
- **Sidebar** - Collapsible side navigation
- **ActivityBar** - VS Code-style activity sidebar
- **ViewContainer** - Panel container with headers

### Form Components (`src/forms/`)
User input and data entry:

- **Input** - Text input with validation
- **Select** - Dropdown selection
- **MultiSelect** - Multiple choice selection
- **DatePicker** - Date selection
- **NumericInput** - Number input with formatting
- **TextArea** - Multi-line text input
- **FileUpload** - File selection
- **FormGroup** - Form organization
- **ValidationMessage** - Error display

### Menu Components (`src/menus/`)
Navigation and command systems:

- **MenuBar** - Top-level application menu
- **ContextMenu** - Right-click context actions
- **CommandPalette** - VS Code-style command search
- **Dropdown** - Dropdown menus and actions
- **Breadcrumb** - Navigation breadcrumbs
- **TreeView** - Hierarchical data display
- **QuickPick** - Quick selection dialogs

### Trading Components (`src/trading/`)
Trading-specific business logic:

- **OrderForm** - Buy/sell order entry
- **QuickOrder** - One-click order buttons
- **PositionDisplay** - Position information
- **PnLDisplay** - Profit/loss indicators
- **PriceDisplay** - Price formatting and coloring
- **MarketDepth** - Order book display
- **TradeHistory** - Transaction history
- **WatchList** - Symbol monitoring
- **AlertPanel** - Trading alerts and notifications

## 🎨 Design System

### Color Palette (VS Code Dark Theme)
```css
/* Background */
--bg-primary: #1e1e1e;
--bg-secondary: #2d2d30;
--bg-tertiary: #3e3e42;

/* Text */
--text-primary: #cccccc;
--text-secondary: #888888;
--text-accent: #ffffff;

/* Actions */
--accent-primary: #0e639c;
--accent-secondary: #5a5d5e;
--accent-danger: #a1260d;
--accent-success: #107c10;
--accent-warning: #ff8c00;

/* Borders */
--border-primary: #3e3e42;
--border-secondary: #555555;
```

### Typography
```css
/* Font Stack */
font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', sans-serif;

/* Sizes */
--font-size-small: 11px;
--font-size-normal: 13px;
--font-size-large: 14px;
--font-size-title: 16px;
```

### Spacing
```css
/* Consistent spacing scale */
--spacing-xs: 4px;
--spacing-sm: 8px;
--spacing-md: 16px;
--spacing-lg: 24px;
--spacing-xl: 32px;
```

## 🔧 Component API Pattern

Each component follows a consistent API pattern:

```svelte
<svelte:options tag="webui-component-name" />

<script>
  // Props with defaults
  export let variant = "primary";
  export let size = "medium";
  export let disabled = false;
  
  // Event dispatch
  function handleAction(data) {
    const event = new CustomEvent('webui-action', {
      detail: { ...data, timestamp: new Date().toISOString() }
    });
    dispatchEvent(event);
  }
</script>

<!-- Template with consistent class naming -->
<div class="webui-component {variant} {size}" class:disabled>
  <slot></slot>
</div>

<style>
  /* Component-specific styles */
  .webui-component {
    /* Base styles */
  }
</style>
```

## 🔌 C# Integration

Components are designed to integrate seamlessly with C# WebView2:

```csharp
// Load component
await webView.NavigateAsync("ComponentName.js", isModule: true);

// Listen for events
webView.WebMessageReceived += (sender, args) => {
    var message = args.TryGetWebMessageAsString();
    var eventData = JsonSerializer.Deserialize<ComponentEvent>(message);
    HandleComponentEvent(eventData);
};
```

## 🧪 Testing Strategy

- **Visual Testing**: Live preview page with all component variants
- **Event Testing**: JavaScript event logging and C# event handling
- **Cross-Browser**: Ensure Web Components work in WebView2 and modern browsers
- **Performance**: Lazy loading and minimal bundle sizes

## 📈 Development Roadmap

### Phase 1: Core Foundation ✅
- [x] Project setup and build system
- [x] Basic test component (TestButton)
- [x] Documentation and architecture

### Phase 2: Essential Components 🔄
- [ ] Core components (Button, Icon, Badge, etc.)
- [ ] Basic layout components (Panel, TabStrip)
- [ ] Form components (Input, Select, Checkbox)

### Phase 3: Advanced UI 🔄
- [ ] Menu system (MenuBar, ContextMenu, CommandPalette)
- [ ] Complex layout (SplitView, Docking)
- [ ] Trading components (OrderForm, PriceDisplay)

### Phase 4: Integration & Polish 🔄
- [ ] C# WebView2 integration examples
- [ ] Theme system and customization
- [ ] Performance optimization
- [ ] Comprehensive testing suite

## 🤝 Integration Points

### With WebUI.Framework
- Components load via `WebViewHost.NavigateAsync()`
- Events captured via `MessageReceived` event handler
- Styling coordinated with Windows Forms parent

### With WebUI.Bridge (Future)
- Component events routed through bridge system
- File system access for component assets
- Inter-panel communication for complex workflows

### With Trading System
- Market data binding and updates
- Order execution and status feedback
- Risk management and position tracking

## 📝 Usage Examples

### Basic Button
```html
<webui-test-button 
  text="Execute Trade" 
  variant="primary" 
  size="large">
</webui-test-button>
```

### Trading Order Form (Future)
```html
<webui-order-form 
  symbol="AAPL" 
  side="buy" 
  quantity="100"
  order-type="market">
</webui-order-form>
```

### Dockable Panel (Future)
```html
<webui-panel 
  title="Market Watch" 
  closeable="true" 
  resizable="true">
  <webui-watchlist symbols="AAPL,MSFT,GOOGL"></webui-watchlist>
</webui-panel>
```

---

## 🔥 Why This Approach Rocks

1. **VS Code Familiarity**: Users already know the UI patterns
2. **Modular Loading**: Only load components you need
3. **Universal Compatibility**: Works anywhere Web Components are supported
4. **Easy Integration**: Simple event-driven communication with C#
5. **Rapid Development**: Consistent API patterns and styling
6. **Future-Proof**: Built on web standards, not framework-specific

This architecture provides the foundation for a professional, extensible trading platform that scales from simple panels to complex multi-window workflows. 