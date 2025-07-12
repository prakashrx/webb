# WebUI Platform: Extension System Architecture and Runtime Model

## 🌟 Core Philosophy: **Everything is an Extension**

The WebUI Platform is a modular, high-performance desktop environment for building real-time trading applications. It draws inspiration from VS Code’s extension system, Figma's plugin sandboxing, and Bloomberg Terminal's panel layout. **Every UI element—toolbars, charts, order forms, dashboards—is implemented as an extension.**

The platform itself provides only:

* A minimal C# host runtime
* WebView2-based panels
* A message-passing bridge: `webui.api`

Extensions do everything else.

---

## 🚀 Key Benefits

* **Modularity**: All features are implemented as isolated extensions
* **Extensibility**: Users can build their own trading tools
* **Performance**: Only active extensions are loaded
* **Hot Swapping**: Extensions can be added or removed without restarting
* **Marketplace-Ready**: Supports third-party ecosystem by design

---

## 🛠️ Architecture Overview

```
Workbench (Coordinator)
├─ Extension Loader (manifests, sandboxing)
├─ IPC Router (command/event routing)
└─ Workspace Manager (state, layout, serialization)

Host Process (WebView2)
├─ Golden Layout + Extension Runtime
├─ Injected API: webui.api
└─ Loaded Extensions (via manifest URLs)
```

Each **WebView2 host** is a runtime capable of loading multiple panels. The **Workbench** coordinates layout, lifecycle, IPC, and extension movement.

---

## 📁 Extension Folder Layout

An extension typically follows this structure:

```
extension/
├── dist/                   # Static test page, compiled output
│   ├── activate.js         # Entry point after build
│   ├── hello.js
│   └── mybutton.js
├── src/                    # Your Svelte components
│   ├── Hello.svelte
│   └── MyButton.svelte
├── manifest.json           # Extension metadata
├── rollup.config.js        # Build configuration
├── package.json            # NPM dependencies and scripts
└── README.md               # Documentation
```

### Example: `manifest.json`

```json
{
  "id": "hello-world",
  "displayName": "Hello World",
  "version": "1.0.0",
  "main": "dist/activate.js",
  "panels": [
    { "id": "hello", "title": "Hello", "defaultDock": "right" }
  ],
  "commands": [
    { "id": "hello.sayHi", "title": "Say Hello" }
  ]
}
```

### Example: `activate.js`

```js
import Hello from './dist/hello.js';

export function activate(context) {
  context.panel.registerView('hello', host => host.mountSvelte(Hello));
  context.commands.register('hello.sayHi', () => console.log('Hello from WebUI!'));
}
```

---

## 🔍 Extension Load Lifecycle

1. **Workbench sends IPC message to Host**:

```json
{
  "type": "load-extension",
  "url": "http://localhost:3001/manifest.json"
}
```

2. **Host fetches `manifest.json`, imports `main.js`**
3. `main.js` exports an `activate(context)` function
4. Extension registers panels, commands, event subscriptions using `webui.api`

---

## 🛋️ Panel Migration Between Hosts

1. **User drags panel to another window**
2. Source host serializes the panel:

```ts
const state = webui.extensions.serialize("panel-id");
```

3. Sends IPC to Workbench:

```json
{
  "type": "move-panel",
  "extensionId": "market-data",
  "panelId": "panel-id",
  "state": { ... },
  "targetHost": "host-2"
}
```

4. Target host loads extension (if needed), calls `deserialize(state)`

Each panel is portable and self-contained.

---

## 🚪 IPC Design (Host ↔ Workbench)

### Transport Layer

* Named Pipes (dev/default)
* Shared Memory (future, for high-throughput feeds)

### Message Types

* `load-extension`
* `move-panel`
* `execute-command`
* `emit-event`
* `get-service`

### Message Flow

```
JS (WebView)
  ↔ webui-api.js wrapper
    ↔ HostApiBridge (C# COM object)
      ↔ IPC Client
        ↔ Workbench Router
          ↔ Target Host / Service
```

---

## 🔄 `webui.api` Overview

The `webui.api` object provides a flat, extensible interface for interacting with host services:

```ts
// Panel & Workspace
webui.panel.open(...)
webui.panel.close(...)
webui.workspace.save(...)

// Commands & Events
webui.commands.register(...)
webui.commands.execute(...)
webui.events.on(...)

// UI
webui.ui.showNotification(...)
webui.ui.showQuickPick(...)

// Layout & Theming
webui.layout.get()
webui.theme.setZoom(...)

// Keyboard & Clipboard
webui.keyboard.registerShortcut(...)
webui.clipboard.readText()

// Host Services
webui.services.marketData.subscribe(...)

// Binary Channels
webui.binary.postMessage(...)
```

Internally, each namespace maps to a modular C# implementation (`PanelApi`, `CommandsApi`, etc.) exposed via COM to the WebView.

---

## 🎨 Extension Authoring Example

```ts
// activate.js
export function activate({ panel, commands, events, services }) {
  // Panel
  webui.panel.registerView("orders", host => host.mountSvelte(OrderEntry));
  webui.panel.open("orders", { title: "Order Entry" });

  // Command
  webui.commands.register("orders.place", args => services.orderEntry.place(args));

  // Event
  webui.events.on("price.update", price => events.emit("orders.price", price));

  // UI
  webui.ui.setMenu([...]);
  const choice = await webui.ui.showQuickPick(["AAPL", "MSFT"]);

  // Workspace
  await webui.workspace.save("My Custom Layout");
}
```

---

## 🤝 Design Principles

* **Extension-Centric**: Core platform and user tools use the same model
* **Isolated but Connected**: Each panel is sandboxed yet interoperable
* **High-Performance**: Binary channels and virtual DOM updates
* **Simple and Familiar**: VS Code-inspired API and manifest

---

## ✅ Summary

This architecture enables dynamic, performant, and developer-friendly trading applications. By treating every feature as an extension, and every panel as a WebView2-powered app, WebUI delivers the flexibility of the browser with the speed and control of native code.

It’s a seamless environment where trading tools can be built, shared, and deployed with the speed of thought.