# WebUI Platform — API Design Document

A comprehensive reference for the **WebUI Platform** JavaScript ↔ C# bridge (`webui.api`), designed to empower both **core** and **user** extensions to build a full-featured, Bloomberg-style trading workspace using Svelte/Tailwind panels.

---

## Table of Contents

1. [Design Principles](#design-principles)
2. [High-Level Architecture](#high-level-architecture)
3. [API Surface Overview](#api-surface-overview)
4. [Namespace Details](#namespace-details)

   1. [Panel & Workspace (`webui.panel`, `webui.workspace`)](#panel--workspace)
   2. [Universal Messaging (`webui.ipc`)](#universal-messaging)
   3. [UI Helpers (`webui.ui`)](#ui-helpers)
   4. [Layout & Theming (`webui.layout`, `webui.theme`)](#layout--theming)
   5. [Keyboard & Clipboard (`webui.keyboard`, `webui.clipboard`)](#keyboard--clipboard)
   6. [Host Services (`webui.services`)](#host-services)
5. [Extension Manifest Schema](#extension-manifest-schema)
6. [Sample Usage Patterns](#sample-usage-patterns)
7. [Implementation Notes](#implementation-notes)
8. [Versioning & Stability](#versioning--stability)
9. [Further Reading](#further-reading)

---

## Design Principles

* **Extension-First**: All UI logic lives in extensions—no built-in panels.
* **VS Code Inspiration**: Familiar `commands`, `events`, `panel` model.
* **Flat Ergonomic Surface**: Shallow call chains (e.g. `webui.panel.open()`).
* **Modular Under the Hood**: Internally grouped by concern in C#.
* **Dockable by Default**: Panels auto-dock via Golden Layout.
* **Domain-Ready**: High-perf trading services and binary streams.

---

## High-Level Architecture

```
[Extension JS/TS]   ⇄  [webui-api.js wrapper]   ⇄  [HostApiBridge C# COM Object]
                                  |                            |
                                  |                            ⇄ [IPC Transport]
           ----------------------------------------------------
           |        |         |       |         |          |
        Panel    IPC        UI      Layout    Keyboard   Services
        Workspace  Messaging  Helpers  & Theme  & Clipboard
```

* **JS Wrapper** flattens and promisifies host methods.
* **HostApiBridge** exposes nested C# objects (`PanelApi`, `IpcApi`, `UiApi`, etc.) via COM under `window.chrome.webview.hostObjects.api`.
* **IPC Transport** handles both local and cross-process messaging through a unified interface.

---

## API Surface Overview

```ts
// Panel & Workspace
webui.panel.registerView(...)
webui.panel.registerPanel(...)  // Svelte convenience wrapper
webui.panel.open(...)      
webui.workspace.save()     
webui.workspace.open(...)

// Universal Messaging (Local + Cross-Process)
webui.ipc.on(...)
webui.ipc.send(...)
webui.ipc.broadcast(...)

// UI Helpers
webui.ui.showQuickPick(...)
webui.ui.showNotification(...)
webui.ui.setMenu(...)

// Layout & Theming
webui.layout.get()
webui.layout.apply(...)
webui.theme.get()
webui.theme.setZoom(...)

// Keyboard & Clipboard
webui.keyboard.registerShortcut(...)
webui.clipboard.readText()

// Host Services
webui.services.log.info(...)
webui.services.storage.get(...)
webui.services.marketData.subscribe(...)
```

---

## Namespace Details

### Panel & Workspace

#### `webui.panel`

| Method                       | Signature                                          | Description                                         |
| ---------------------------- | -------------------------------------------------- | --------------------------------------------------- |
| `registerView(id, provider)` | `(string, (host: PanelHost) => void) → Disposable` | Registers a factory function for custom panels.     |
| `registerPanel(id, component)` | `(string, SvelteComponent) → Disposable` | Convenience wrapper for Svelte components.         |
| `open(id, opts?)`            | `(string, PanelOptions?) → Promise<PanelHandle>`   | Opens (and docks) a panel; auto-reuse if exists.    |
| `close(id)`                  | `(string) → void`                                  | Closes the panel.                                   |
| `onDidActivate(fn)`          | `(viewId: string) → Disposable`                    | Fires when a panel gains focus.                     |
| `onDidClose(fn)`             | `(viewId: string) → Disposable`                    | Fires when a panel is closed.                       |

#### `webui.workspace`

| Method            | Signature                     | Description                             |
| ----------------- | ----------------------------- | --------------------------------------- |
| `save(name?)`     | `(string?) → Promise<string>` | Persists current layout as a workspace. |
| `open(name)`      | `(string) → Promise<void>`    | Loads a saved workspace.                |
| `list()`          | `() → Promise<string[]>`      | Returns available workspace names.      |
| `delete(name)`    | `(string) → Promise<void>`    | Removes a saved workspace.              |
| `onDidChange(fn)` | `(name: string) → Disposable` | Fires on workspace switch.              |

---

### Universal Messaging

#### `webui.ipc`

All communication (local handlers, cross-process calls, broadcasts) unified under one clean API:

| Method                   | Signature                                      | Description                               |
| ------------------------ | ---------------------------------------------- | ----------------------------------------- |
| `on(type, handler)`      | `(string, (payload:any, from:string)=>any) → Disposable` | Registers message handler (local or remote). |
| `send(type, payload?)`   | `(string, any?) → Promise<any>`                | Sends to platform/workbench.             |
| `send(to, type, payload?)` | `(string, string, any?) → Promise<any>`       | Sends to specific extension.              |
| `broadcast(type, payload?)` | `(string, any?) → void`                     | Sends to all extensions.                  |
| `off(type, handler)`     | `(string, function) → void`                    | Removes message handler.                  |

**Examples:**
```ts
// Register handlers (works for both local and remote calls)
webui.ipc.on("orders.refresh", () => refreshOrderData());
webui.ipc.on("orders.place", (order) => placeOrder(order));

// Call platform services
await webui.ipc.send("workspace.save", { name: "MyLayout" });

// Call other extensions
const result = await webui.ipc.send("extension:orders", "orders.getPositions");

// Broadcast to all
webui.ipc.broadcast("theme.changed", { theme: "dark" });
```

---

### UI Helpers

#### `webui.ui`

| Method                             | Signature                                                                                | Description                |                    |
| ---------------------------------- | ---------------------------------------------------------------------------------------- | -------------------------- | ------------------ |
| `showQuickPick(items)`             | \`(string\[]) → Promise\<string                                                          | undefined>\`               | Dropdown selector. |
| `showInputBox(opts)`               | \`({ prompt\:string, value?\:string }) → Promise\<string                                 | undefined>\`               | Text input.        |
| `showOpenDialog(opts)`             | `({ title?:string; filters?:FileFilter[]; multiSelect?:boolean }) → Promise<FileInfo[]>` | File open.                 |                    |
| `showSaveDialog(opts)`             | \`({ title?\:string; defaultName?\:string; filters?\:FileFilter }) → Promise\<FileInfo   | undefined>\`               | File save.         |
| `showNotification(opts)`           | `({ title:string; body:string; type?:string; durationMs?:number }) → void`               | Non-modal toast.           |                    |
| `setMenu(items)`                   | `(MenuItem[]) → void`                                                                    | Declares application menu. |                    |
| `showContextMenu(items, pos)`      | `(MenuItem[], {x:number,y:number}) → void`                                               | Right-click menu.          |                    |
| `setStatusBarItems(items)`         | `(StatusBarItem[]) → void`                                                               | Status bar widgets.        |                    |
| `showTooltip(text, target, opts?)` | `(string, HTMLElement, TooltipOptions?) → void`                                          | Floating tooltip.          |                    |

---

### Layout & Theming

#### `webui.layout`

| Method            | Signature                            | Description                          |
| ----------------- | ------------------------------------ | ------------------------------------ |
| `get()`           | `() → Promise<LayoutDescriptor>`     | Returns current Golden Layout state. |
| `apply(desc)`     | `(LayoutDescriptor) → Promise<void>` | Restores a saved layout.             |
| `onDidChange(fn)` | `(LayoutDescriptor) → Disposable`    | Fires on layout change.              |

#### `webui.theme`

| Method              | Signature                  | Description               |                         |                     |
| ------------------- | -------------------------- | ------------------------- | ----------------------- | ------------------- |
| `getCurrent()`      | `() → Promise<string>`     | Returns current theme ID. |                         |                     |
| `set(id)`           | `(string) → Promise<void>` | Switches theme.           |                         |                     |
| `onDidChange(fn)`   | `(string) → Disposable`    | Fires on theme change.    |                         |                     |
| `setZoom(factor)`   | `(number) → void`          | Adjusts UI scale.         |                         |                     |
| `setDensity(level)` | \`("compact"               | "standard"                | "comfortable") → void\` | Adjusts UI density. |

---

### Keyboard & Clipboard

#### `webui.keyboard`

| Method                          | Signature                                 | Description              |
| ------------------------------- | ----------------------------------------- | ------------------------ |
| `registerShortcut(id, key, cb)` | `(string, string, ()=>void) → Disposable` | Binds a shortcut.        |
| `unregisterShortcut(id)`        | `(string) → void`                         | Removes a binding.       |
| `getShortcuts()`                | `() → Promise<{id:string;key:string}[]>`  | Lists current shortcuts. |
| `onDidPressKey(fn)`             | `(KeyEvent) → Disposable`                 | Global key listener.     |

#### `webui.clipboard`

| Method             | Signature                  | Description   |
| ------------------ | -------------------------- | ------------- |
| `readText()`       | `() → Promise<string>`     | Reads text.   |
| `writeText(text)`  | `(string) → Promise<void>` | Writes text.  |
| `readImage()`      | `() → Promise<Blob>`       | Reads image.  |
| `writeImage(blob)` | `(Blob) → Promise<void>`   | Writes image. |

---

### Host Services

#### `webui.services`

| Service              | Key          | Example Methods                                |
| -------------------- | ------------ | ---------------------------------------------- |
| Logging              | `log`        | `debug/info/warn/error(msg)`                   |
| Storage              | `storage`    | `get<T>(key)/set<T>(key,value)/delete(key)`    |
| Fetch                | `fetch`      | `fetch(url, init):Promise<Response>`           |
| Telemetry            | `telemetry`  | `track(event, props?)`                         |
| Internationalization | `i18n`       | `translate(key, vars?)`                        |
| Market Data          | `marketData` | `subscribe(symbols, cb):Disposable`            |
| Order Entry          | `orderEntry` | `place(req):Promise<resp>`, `onFill(fn)`       |
| Portfolio            | `portfolio`  | `get():Promise<Portfolio>`, `onChange(fn)`     |
| Charting             | `charting`   | `render(container, opts):Promise<ChartHandle>` |
| Analytics            | `analytics`  | `run(query):Promise<Result>`                   |

---



## Extension Manifest Schema

```jsonc
{
  "id": "string",
  "displayName": "string",
  "version": "string",
  "main": "path/to/dist/activate.js",
  "panels": [
    {
      "id": "string",
      "title": "string",
      "defaultDock": "left|right|bottom|main",
      "reuseIfExists": true
    }
  ],
  "menus"?: [
    {
      "id": "string",
      "label": "string",
      "submenu": [{ "id":"", "label":"", "ipc":"" }]
    }
  ],
  "contributes"?: {
    "keyboard": [{ "id":"", "key":"", "ipc"?:"", "when"?:"" }],
    "services": ["marketData","orderEntry",...]
  }
}
```

---

## Sample Usage Patterns

```ts
// activate.js
import OrdersPanel from './OrdersPanel.svelte';

export function activate(context) {
  // Register & open panel (Svelte component directly)
  webui.panel.registerPanel("orders", OrdersPanel);
  webui.panel.open("orders", { title: "Order Entry", defaultDock:"right" });

  // IPC Message Handlers
  webui.ipc.on("orders.place", async (order) => {
    return await webui.services.orderEntry.place(order);
  });
  
  webui.ipc.on("orders.refresh", () => {
    // Refresh order data and notify other extensions
    webui.ipc.broadcast("orders.updated", getOrderData());
  });

  // Listen for cross-extension events
  webui.ipc.on("market.update", (data) => {
    // Update UI with new market data
    updateOrderPrices(data);
  });

  // Call other extensions
  const positions = await webui.ipc.send("extension:portfolio", "positions.get");
  
  // Call platform services
  await webui.ipc.send("workspace.save", { name: "MyLayout" });

  // UI Helpers
  webui.ui.setMenu([{ id:"file", label:"File", submenu:[...] }]);
  const sel = await webui.ui.showQuickPick(["AAPL","MSFT"]);
}
```

---

## Implementation Notes

* **HostApiBridge** in C# exposes nested objects (`PanelApi`, `IpcApi`, `UiApi`, etc.) marked `[ComVisible]`.
* **JS wrapper** (`webui-api.js`) injects promises, flattens, and adds helpers (`host.mountSvelte`).
* **IpcApi** handles both local message routing and cross-process communication transparently.
* **Svelte/Tailwind** components mount into `PanelHost.element`.
* **Golden Layout** lifecycle and persistence abstracted by `api.layout` & `api.workspace`.

---

## Versioning & Stability

* **SemVer** for the API: breaking changes only on major version bumps.
* **Deprecation Guides** for any renamed methods.
* **Experimental Flags** (`api.experimental.*`) for unreleased features.

---

## Further Reading

* [VS Code Extension API](https://code.visualstudio.com/api)
* [Golden Layout Documentation](https://golden-layout.com/docs/)
* [WebView2 COM Injection](https://docs.microsoft.com/webview2/)