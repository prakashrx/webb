# WebUI Platform - Project Overview

## Value Proposition

**Platform for building trading applications** — A VS Code–style extensible desktop shell where traders and developers can create real‑time grids, charts, and custom tools as plugins.

**Who**: Internal quants, desk developers, third‑party plugin authors.

## Architecture Layers

### 1. Infrastructure (Foundation)

* **WebUI.Core**: C# + WebView2 core (hosting, IPC router, process & WebView pooling)
* **WebUI.UIComponents**: Svelte component library + build tooling

### 2. Applications (User‑Facing)

* **WebUI.Shell**: Control App (floating toolbar, workspace manager)
* **WebUI.Host**: Host processes (isolated plugin containers with Golden Layout)

### 3. Extension System

* **Extension Loader**: Manifest discovery, dependency resolution, activation lifecycle
* **API Injection**: `webui.api` interface (commands, events, window, services)

## Workspace Concept

A **workspace** is a saved configuration of:

* Core UI (toolbar, menus, status)
* Active extensions & their panels
* Panel layout & docking (Golden Layout)

Workspaces are JSON files you can load/save/share.

## Current Status & Phases

1. **Phase 1 (✅)**: Framework — WebView2 hosting & basic IPC
2. **Phase 2 (✅)**: Foundation — Svelte build, clean project structure  
3. **Phase 3 (🔄)**: Core API Architecture — HostApiBridge, modular API components
4. **Phase 4 (⏳)**: Extension Foundation — Manifest system, extension loader
5. **Phase 5 (⏳)**: Developer Experience — Documentation, CLI tools, scaffolding
6. **Phase 6 (⏳)**: POC Extension — Convert MainToolbar to first extension

**Current Progress**: 6/16 tasks completed (37.5%)

## Next Steps (Top 3)

1. **Design `webui.api` API**

   * `api.commands`, `api.events`, `api.window`, `api.services`
2. **Build HostApiBridge**

   * Modular API components with lazy-loading
   * Clean JavaScript interface abstraction
3. **Bootstrap POC Extension**

   * Minimal manifest + Svelte panel using host APIs
   * Validate extension load & messaging

## Key Use Case: High‑Performance Grid UI

* **Challenge**: Render millions of real‑time rows without lag
* **Pattern**: Backend‑owned columnar store + frontend viewport

  * **Pull**: `getRows(start, count)` on demand
  * **Push**: `pushUpdate(rowIndex, data)` for visible rows
* **Implementation**: AG Grid in Svelte + `webui.api` transports Arrow batches

## Technical Stack & Project Structure

* **Backend**: .NET 9.0, WebView2, Windows Forms
* **Frontend**: Svelte + Tailwind, Rollup/Vite, TypeScript
* **IPC**: JSON & binary over native WebView2 bridge
* **Dev**: Hot reload, real WebView testing, workspace persistence

```
WebUI.Platform/             # Solution
├── Shell/                  # Control App (WebUI.Shell project)
├── Host/                   # Future: Plugin hosts (Golden Layout)
├── Core/                   # Core libs (WebUI.Core project)
├── UIComponents/           # Svelte UI library
├── Extensions/             # Future: Extension system
└── Tools/                  # Future: CLI tools
```

## Design Principles

* **Modular**: Clear separation of foundation vs. apps vs. extensions
* **Performant**: Zero‑copy feeds, minimal IPC overhead
* **Flexible**: Unlimited docking (Golden Layout), workspace‑driven UI
* **Extensible**: Simple plugin model, powerful host APIs

## Implementation Roadmap (Detail)

### Phase 3: IPC Router

* Multi‑process message bus (JSON & binary)
* Pub/sub with wildcard support
* RPC pattern for commands
* Track host processes & capabilities

### Phase 4: API Injection

* Finalize TypeScript interface for `webui.api`
* Inject into WebViews with context isolation
* Implement permissions & error boundaries

### Phase 5: Extension Loader

* Discover & parse manifests
* Resolve dependencies
* Activate extensions in hosts

### Phase 6: POC Extension

* "Hello World" plugin with UI panel
* Test host commands & events
* Confirm developer workflow

---

*Ready to transform trading workflows with a modular, high‑performance desktop platform.*
