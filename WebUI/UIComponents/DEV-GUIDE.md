# WebUI Components - Development Guide

## 🚀 Getting Started

### 1. Install Dependencies
```bash
# Navigate to components directory
cd WebUI.Components

# Install with npm
npm install
```

### 2. Start Development
```bash
# Start development server with hot reload
npm run dev

# Or use the alternate command
npm start
```

This will:
- Build all `.svelte` files from `src/` into `public/`
- Start a development server at `http://localhost:5000`
- Watch for changes and rebuild automatically
- Enable live reload in the browser

### 3. Test Components
- Open `http://localhost:5000` in your browser
- The test page shows all component variants
- Click buttons to see event logging in action

## 📝 Creating New Components

### 1. Create Component File
```bash
# Create in appropriate subdirectory
touch src/core/MyButton.svelte
```

### 2. Component Template
```svelte
<svelte:options tag="webui-my-button" />

<script>
  export let text = "Click me";
  export let variant = "primary";
  export let disabled = false;
  
  function handleClick() {
    if (!disabled) {
      const event = new CustomEvent('webui-click', {
        detail: { text, variant, timestamp: new Date().toISOString() }
      });
      dispatchEvent(event);
    }
  }
</script>

<button 
  class="webui-my-button {variant}" 
  class:disabled
  on:click={handleClick}
  {disabled}
>
  {text}
</button>

<style>
  .webui-my-button {
    /* Your styles here */
  }
</style>
```

### 3. Test in HTML
Add to `public/index.html`:
```html
<script type="module" src="MyButton.js"></script>
<webui-my-button text="Test" variant="primary"></webui-my-button>
```

## 🔧 Integration with C# WebView

### 1. Load Component
```csharp
// In your C# WebView code
await webView.NavigateAsync("MyButton.js", isModule: true);
```

### 2. Listen for Events
```csharp
webView.WebMessageReceived += (sender, args) => {
    var message = args.TryGetWebMessageAsString();
    // Handle component events
};
```

### 3. Use in HTML
```html
<webui-my-button text="From C#" variant="primary"></webui-my-button>
```

## 🎨 Styling Guidelines

### Use VS Code Color Palette
```css
/* Background colors */
background-color: #1e1e1e; /* Primary */
background-color: #2d2d30; /* Secondary */
background-color: #3e3e42; /* Tertiary */

/* Text colors */
color: #cccccc; /* Primary text */
color: #888888; /* Secondary text */
color: #ffffff; /* Accent text */

/* Action colors */
background-color: #0e639c; /* Primary action */
background-color: #5a5d5e; /* Secondary action */
background-color: #a1260d; /* Danger */
```

### Consistent Class Naming
- Use `webui-` prefix for all classes
- Use component name as root class
- Use modifiers for variants: `.webui-button.primary`
- Use state classes: `.webui-button.disabled`

## 📁 File Organization

```
src/
├── core/           # Basic UI primitives
│   ├── Button.svelte
│   ├── Icon.svelte
│   └── Badge.svelte
├── layout/         # Layout components
│   ├── Panel.svelte
│   ├── TabStrip.svelte
│   └── SplitView.svelte
├── forms/          # Form controls
│   ├── Input.svelte
│   ├── Select.svelte
│   └── Checkbox.svelte
├── menus/          # Navigation
│   ├── MenuBar.svelte
│   ├── ContextMenu.svelte
│   └── CommandPalette.svelte
└── trading/        # Trading-specific
    ├── OrderForm.svelte
    ├── PriceDisplay.svelte
    └── WatchList.svelte
```

## 🧪 Testing

### 1. Visual Testing
- Use the test page at `http://localhost:5000`
- Test all component variants and states
- Verify responsive behavior

### 2. Event Testing
- Click components and check event log
- Verify event data structure
- Test event handling in C# integration

### 3. Cross-Browser Testing
- Test in Chrome (WebView2 base)
- Test in other browsers for compatibility
- Verify Web Components support

## 🔄 Build Process

### Development Build
```bash
npm run dev
```
- Builds with source maps
- Starts development server
- Enables live reload

### Production Build
```bash
npm run build
```
- Minifies JavaScript
- Removes source maps
- Optimizes for production

## 📦 Output Structure

After building, components are available in `public/`:
```
public/
├── index.html      # Test page
├── TestButton.js   # Individual component
├── TestButton.css  # Component styles
├── MyButton.js     # Another component
└── MyButton.css    # Its styles
```

## 🔗 Integration Tips

### 1. Dynamic Loading
```javascript
// Load component on demand
const script = document.createElement('script');
script.type = 'module';
script.src = 'ComponentName.js';
document.head.appendChild(script);
```

### 2. Event Handling
```javascript
// Listen for all component events
document.addEventListener('webui-click', handleClick);
document.addEventListener('webui-change', handleChange);
document.addEventListener('webui-submit', handleSubmit);
```

### 3. State Management
```javascript
// Update component props
const component = document.querySelector('webui-my-button');
component.setAttribute('text', 'Updated Text');
component.setAttribute('disabled', 'true');
```

## 🚨 Common Issues

### 1. Component Not Loading
- Check if JS file is generated in `public/`
- Verify `<script type="module">` is used
- Check browser console for errors

### 2. Events Not Firing
- Ensure event listeners are attached
- Check event name matches component
- Verify `dispatchEvent` is called

### 3. Styling Issues
- Check if CSS file is generated
- Verify CSS is loaded in page
- Check for specificity conflicts

## 💡 Best Practices

1. **Consistent Naming**: Use `webui-` prefix for all custom elements
2. **Event Naming**: Use `webui-` prefix for all custom events
3. **Props**: Provide sensible defaults for all props
4. **Documentation**: Add comments for complex logic
5. **Testing**: Test all variants and edge cases
6. **Performance**: Keep components lightweight and focused 