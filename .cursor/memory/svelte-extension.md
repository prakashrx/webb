Here is the complete markdown article based on your current architecture and workflow:

---

# Building Svelte Extensions for WebUI Platform

## 🌟 Overview

The WebUI Platform enables developers to build modular, high-performance trading applications using Svelte panels — all powered by an extension-first, dynamically loaded architecture.

This guide explains how to build and run a WebUI extension using Svelte, from authoring through bundling to runtime activation.

---

## 📁 Extension Folder Layout

A typical WebUI extension looks like this:

```
extension/
├── dist/                   # Compiled JS output
│   └── activate.js         # Bundled entry point
├── src/                    # Source files
│   ├── activate.js         # Registers your panels, commands, etc.
│   ├── MyPanel.svelte      # Main panel UI
│   └── helpers.js
├── manifest.json           # Metadata and contributions
├── vite.config.js          # Bundler config
├── index.html              # Vite entrypoint (needed for dev server)
├── package.json            # Dev dependencies and scripts
└── README.md
```

---

## 🔧 How It Works: Svelte + Runtime

### 1. Author Your Panel

```js
// src/activate.js
import MyPanel from './MyPanel.svelte';

export function activate(context) {
  webui.panel.registerPanel("orders", MyPanel);
  webui.panel.open("orders");
}
```

```svelte
<!-- src/MyPanel.svelte -->
<script>
  export let name = "Trader";
</script>

<div class="p-2">Hello, {name}!</div>
```

---

### 2. Bundle with Vite

```js
// vite.config.js
import { defineConfig } from 'vite';
import { svelte } from '@sveltejs/vite-plugin-svelte';

export default defineConfig({
  plugins: [svelte()],
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    rollupOptions: {
      input: 'src/activate.js',
      output: {
        format: 'esm'
      }
    }
  },
  server: {
    port: 3001,
    strictPort: true
  }
});
```

```bash
npm install --save-dev vite @sveltejs/vite-plugin-svelte
```

```json
{
  "scripts": {
    "dev": "vite",
    "build": "vite build"
  }
}
```

---

### 3. Serve via HTTP

Run a dev server from your extension root:

```bash
npm run dev
```

This launches the Vite server at `http://localhost:3001` and:

* Supports HMR (Hot Module Reloading) out of the box
* Serves `manifest.json` and `dist/activate.js`

---

### 4. Load from WebUI Workbench

WebUI dynamically loads your extension with:

```json
{
  "type": "load-extension",
  "url": "http://localhost:3001/manifest.json"
}
```

Your manifest might look like:

```json
{
  "id": "orders",
  "main": "dist/activate.js",
  "panels": [
    { "id": "orders", "title": "Order Entry" }
  ]
}
```

---

## ✅ Summary: Runtime Flow

* You write `.svelte` and `activate()` in `src/`
* Bundle compiles everything into `dist/activate.js`
* `MyPanel` becomes a plain JS class
* WebUI loads your manifest and runs `activate(context)`
* You register and open panels using `webui.panel.registerPanel`

---

## 🔄 `webui.api` Highlights

```ts
webui.panel.registerPanel("orders", MyPanel);
webui.panel.open("orders");

webui.ipc.on("orders.refresh", () => {...});
webui.services.marketData.subscribe(...);
```

All DOM and layout details are abstracted away. You only provide components and logic.

---

## 🌀 Hot Reload Support

When you edit `.svelte` files or `activate.js`, Vite detects changes and pushes live updates to the WebView2 container. If HMR isn't supported (e.g. panel logic can't be patched), WebUI can reload the entire panel instance.

---

## 📝 Additional Setup Files

**index.html** (even if unused, required for Vite dev server):

```html
<!DOCTYPE html>
<html><body><script type="module" src="/src/activate.js"></script></body></html>
```

**.gitignore**:

```
/dist
/node_modules
```

---

## Cleaner API

The platform provides `registerPanel` as a convenience wrapper for Svelte components:

```js
// For Svelte components, use registerPanel
webui.panel.registerPanel("orders", MyPanel);
```

Behind the scenes, `registerPanel` wraps the base `registerView` API:

```js
// Internal implementation (you don't need to write this)
webui.panel.registerView("orders", (host) => {
  const instance = new MyPanel({ target: host.element });
  host.onDispose(() => instance.$destroy());
  return instance;
});
```

---

## 🧪 Dev Workflow Recap

| Step  | Action                         | Tool        |
| ----- | ------------------------------ | ----------- |
| 🧑‍💻 | Write `.svelte`, `activate.js` | Your code   |
| 🛠    | Launch Dev Server              | `vite`      |
| 🌐    | Auto-reload on change          | Vite HMR    |
| 🚀    | Load via manifest.json         | WebUI Shell |

---

## 🤝 Design Principles

* **Extension-Centric**: Every UI is a plugin
* **Svelte-Native**: Use idiomatic Svelte, not DOM hacks
* **Declarative Activation**: Just export `activate(context)`
* **Isolated + Reactive**: Each panel is sandboxed
* **Hot Swappable**: Extensions can load/update without restarts

---

## ✅ Conclusion

With WebUI and Svelte, you get a modern, modular extension system where UI panels are just Svelte components, and extension logic is just JavaScript. You focus on functionality — the platform handles everything else.

**Note**: We recommend using [Vite](https://vitejs.dev) for Svelte extensions — it provides fast dev builds, live reload, and a smooth integration with WebUI's panel system.

Write it like an app. Bundle it like a library. Run it like native.

Welcome to the WebUI way.