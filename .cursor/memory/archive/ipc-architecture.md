# 🛰️ WebUI IPC Messaging API

The WebUI IPC (Inter-Process Communication) API enables your extension to talk to the platform and other extensions — across processes — using a unified, message-based interface.

You don’t need to know about pipes, COM, or routing: just send messages, receive responses, and subscribe to events.

---

## ✨ Quick Start

```ts
// Send a command to the platform
await webui.ipc.send("command.execute", { id: "workspace.save" });

// Ask another extension for data
const prices = await webui.ipc.send("extension:market", "market.getPrices", {
  symbols: ["AAPL", "TSLA"]
});

// Broadcast a theme change to all extensions
webui.ipc.broadcast("theme.changed", { theme: "dark" });

// Listen for broadcast events
webui.ipc.on("theme.changed", (payload, from) => {
  console.log(`Theme was changed by ${from}`);
});
```

---

## 🧠 Message Routing Model

Every message goes through the **Workbench**, which acts as the central router.

You don’t need to specify `"workbench"` as a destination — it's always implicit. You only name:

* the **type** of message you're sending
* (optionally) the **target**

### Common Targets

| Target               | Description                         |
| -------------------- | ----------------------------------- |
| *omitted*            | Message handled by Workbench itself |
| `"extension:orders"` | Send directly to another extension  |
| `"broadcast"`        | Send to all loaded extensions       |
| `"self"`             | Send to your own process (loopback) |

---

## 🧾 API Reference

### `webui.ipc.send(type, payload)`

Sends a message to the platform. Returns a `Promise` that resolves with a response (if any).

```ts
const result = await webui.ipc.send("command.execute", {
  id: "orders.refresh"
});
```

---

### `webui.ipc.send(to, type, payload)`

Sends a message to a specific target (e.g. another extension).

```ts
const result = await webui.ipc.send("extension:orders", "orders.place", {
  symbol: "AAPL",
  qty: 100
});
```

---

### `webui.ipc.broadcast(type, payload)`

Sends a message to **all connected extensions**.

```ts
webui.ipc.broadcast("workspace.changed", { name: "Layout 2" });
```

---

### `webui.ipc.on(type, handler)`

Subscribes to a message by type.

```ts
webui.ipc.on("workspace.changed", (payload, from) => {
  console.log("Received layout from", from);
});
```

---

### `webui.ipc.off(type, handler)`

Unsubscribes a listener.

```ts
const handler = (data) => { ... };
webui.ipc.on("theme.changed", handler);
webui.ipc.off("theme.changed", handler);
```

---

## 💬 Message Format (Internal)

Messages are automatically wrapped by the platform with metadata:

```ts
{
  id: "msg-uuid",
  type: "orders.place",
  to: "extension:orders",
  from: "extension:watchlist",
  payload: { symbol: "TSLA" },
  expectsResponse: true
}
```

You don’t need to construct this manually — `webui.ipc.send()` does it for you.

---

## 🧩 Use Cases

### 🔁 Request/Response Between Extensions

```ts
// extension:watchlist
const response = await webui.ipc.send("extension:orders", "orders.quotePreview", {
  symbol: "NVDA"
});
```

```ts
// extension:orders
webui.ipc.on("orders.quotePreview", (payload) => {
  return { preview: calculatePreview(payload.symbol) };
});
```

---

### 📡 Global Event Distribution

```ts
webui.ipc.broadcast("theme.changed", { theme: "dark" });
```

All extensions can listen:

```ts
webui.ipc.on("theme.changed", ({ theme }) => {
  applyTheme(theme);
});
```

---

## ⚠️ Error Handling

If a target is unavailable, or the handler throws, the `send()` call rejects:

```ts
try {
  await webui.ipc.send("extension:analytics", "metrics.export");
} catch (err) {
  console.warn("Analytics extension not responding");
}
```

---

## 🧪 Test Mode

In development, you can mock or inspect all IPC traffic:

```ts
webui.ipc.intercept("*", (msg) => {
  console.log("[IPC Debug]", msg);
});
```

---

## 🧱 Design Principles

* **Zero-config**: No manual registration or wiring
* **Message-oriented**: Think in events, not APIs
* **Routed-by-name**: You don’t need to know pipes or hosts
* **Isolated**: Each extension can sandbox what it listens to
* **Composable**: Messages are just data — easily serialized, replayed, cached

---

## ✅ Summary

The WebUI IPC system lets you:

* Send messages to the platform or other extensions
* Subscribe to global or directed events
* Build clean, decoupled extensions
* Leverage the Workbench as the routing brain

You write this:

```ts
await webui.ipc.send("command.execute", { id: "workspace.save" });
```

…and the platform handles the rest:
routing, transport, response tracking, lifecycle, and retries.
