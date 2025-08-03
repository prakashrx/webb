# WebUI API Design Document

## Overview

This document outlines the design for the WebUI JavaScript/TypeScript API in our experimental SDK. We're building a modern, clean API from scratch without legacy constraints.

## Core Design Decisions

### 1. Import Strategy: Modern ES Modules Only

```javascript
// All APIs accessed via imports - no globals
import { invoke, open, send } from '@webui/api';
import { currentPanel } from '@webui/api/panel';
import { on, emit } from '@webui/api/message';
```

**Rationale:**
- Clean, modern approach
- Tree-shakeable 
- Better TypeScript support
- No global namespace pollution

### 2. Async Pattern: Smart Async

```javascript
// Async for operations (cross-process, I/O, etc)
await open('settings');
await invoke('save-file', data);
await send('other-panel', message);

// Sync for simple property access
const id = currentPanel.id;        // Direct property
const title = currentPanel.title;  // Direct property
currentPanel.minimize();           // Void operations can be sync

// Async when it makes sense
const result = await invoke('get-data');
const response = await request('fetch-user', { id: 123 });
```

### 3. TypeScript: Generated Types + Clean API

```typescript
// Auto-generated from C# commands (via source generator)
// types/commands.generated.ts
interface Commands {
  'save-file': { args: { path: string, content: string }, returns: void };
  'get-user': { args: { id: string }, returns: User };
  'list-files': { args: { dir: string }, returns: string[] };
}

// Clean, typed API surface
export async function invoke<K extends keyof Commands>(
  command: K,
  args: Commands[K]['args']
): Promise<Commands[K]['returns']>;

// Usage - fully typed
const user = await invoke('get-user', { id: '123' }); // user: User
const files = await invoke('list-files', { dir: '/home' }); // files: string[]
```

### 4. Module Organization: Flat with Logical Groups

```javascript
// Common operations from root
import { invoke, open, send, on, emit } from '@webui/api';

// Grouped imports for organization
import { invoke } from '@webui/api/core';
import { open, currentPanel } from '@webui/api/panel';
import { send, on, emit, broadcast } from '@webui/api/message';

// Object-oriented for stateful APIs
import { currentPanel } from '@webui/api';
currentPanel.title = 'New Title';
currentPanel.minimize();
```

### 5. Bridge Architecture: COM + PostMessage Hybrid

```javascript
// JS → C# via COM (synchronous call, but non-blocking)
bridge.Core.InvokeCommand(commandName, argsJson);

// C# → JS via PostMessage (for async responses)
webView.CoreWebView2.PostWebMessageAsJson(responseJson);

// Enables true async patterns
async function invoke(command, args) {
  const id = crypto.randomUUID();
  
  return new Promise((resolve, reject) => {
    const handler = (event) => {
      if (event.data.id === id) {
        window.removeEventListener('message', handler);
        event.data.error ? reject(event.data.error) : resolve(event.data.result);
      }
    };
    
    window.addEventListener('message', handler);
    getBridge().Core.InvokeCommand(command, JSON.stringify({ id, args }));
  });
}
```

### 6. Bundle Strategy: Virtual Module Injection

The API is injected as a virtual module during build time:

```javascript
// Developer writes
import { invoke } from '@webui/api';

// Build system resolves to injected module
// No npm package needed
// No separate HTTP requests
// Automatically available in all panels
```

## API Structure

### Core Module (`@webui/api/core`)
```typescript
// Command invocation
export function invoke<T>(command: string, args?: any): Promise<T>;

// App lifecycle
export function quit(): Promise<void>;
export function restart(): Promise<void>;
```

### Panel Module (`@webui/api/panel`)
```typescript
// Current panel instance
export const currentPanel: {
  readonly id: string;
  readonly title: string;
  minimize(): void;
  maximize(): void;
  restore(): void;
  close(): void;
  setTitle(title: string): void;
  readonly isMaximized: boolean;
  readonly isMinimized: boolean;
};

// Panel management
export function open(panelId: string, options?: PanelOptions): Promise<void>;
export function close(panelId: string): Promise<void>;
export function list(): Promise<PanelInfo[]>;
```

### Message Module (`@webui/api/message`)
```typescript
// Event handling
export function on(event: string, handler: (data: any) => void): () => void;
export function once(event: string, handler: (data: any) => void): () => void;
export function off(event: string, handler?: (data: any) => void): void;
export function emit(event: string, data?: any): void;

// Inter-panel communication
export function send(target: string, event: string, data?: any): Promise<void>;
export function broadcast(event: string, data?: any): Promise<void>;

// Request/Response pattern
export function request<T>(target: string, event: string, data?: any): Promise<T>;
export function handle<T>(event: string, handler: (data: any) => T | Promise<T>): () => void;
```

### Dialog Module (`@webui/api/dialog`)
```typescript
// File dialogs
export function open(options?: OpenDialogOptions): Promise<string | string[] | null>;
export function save(options?: SaveDialogOptions): Promise<string | null>;

// Message dialogs  
export function message(text: string, options?: MessageOptions): Promise<void>;
export function confirm(text: string, options?: ConfirmOptions): Promise<boolean>;
```

### Window Module (`@webui/api/window`)
```typescript
// Window management
export function create(url: string, options?: WindowOptions): Promise<PanelHandle>;
export function getAll(): Promise<PanelHandle[]>;
export function getCurrent(): PanelHandle;
```

## Implementation Details

### Type Generation

A C# source generator will create TypeScript definitions:

```csharp
[WebUICommand]
public record SaveFileCommand(string Path, string Content) : ICommand<bool>;

// Generates:
interface Commands {
  'save-file': { 
    args: { path: string; content: string };
    returns: boolean;
  };
}
```

### Error Handling

All async operations use standard Promise rejection:

```typescript
try {
  await invoke('risky-operation', data);
} catch (error) {
  console.error('Operation failed:', error.message);
}
```

### Event System

Clean, efficient event handling:

```typescript
// Auto-cleanup with return function
const unlisten = on('data-update', (data) => {
  console.log('Got update:', data);
});

// Later...
unlisten(); // Removes handler

// Or use once for single events
once('ready', () => {
  console.log('Panel ready!');
});
```

## Usage Examples

### Basic Svelte Component

```svelte
<script lang="ts">
  import { invoke } from '@webui/api';
  import { currentPanel } from '@webui/api/panel';
  import { on } from '@webui/api/message';
  
  let data = [];
  
  onMount(async () => {
    // Load initial data
    data = await invoke('get-data');
    
    // Listen for updates
    const unlisten = on('data-updated', (newData) => {
      data = newData;
    });
    
    // Set panel title
    currentPanel.title = 'My Panel';
    
    // Cleanup
    return () => unlisten();
  });
  
  async function save() {
    await invoke('save-data', { data });
  }
</script>
```

### Inter-Panel Communication

```typescript
// Panel A
import { broadcast } from '@webui/api/message';

async function changeTheme(theme: string) {
  await broadcast('theme-changed', { theme });
}

// Panel B
import { on } from '@webui/api/message';

on('theme-changed', ({ theme }) => {
  document.body.className = theme;
});
```

### File Operations

```typescript
import { open, save } from '@webui/api/dialog';
import { invoke } from '@webui/api';

async function openFile() {
  const path = await open({
    filters: [
      { name: 'Text Files', extensions: ['txt', 'md'] },
      { name: 'All Files', extensions: ['*'] }
    ]
  });
  
  if (path) {
    const content = await invoke('read-file', { path });
    return content;
  }
}
```

## Benefits

1. **Modern Developer Experience**
   - Familiar import syntax
   - Full TypeScript support
   - IntelliSense everywhere
   - Tree-shaking enabled

2. **Performance**
   - Only load what you use
   - Efficient event system
   - No global namespace checks
   - Minimal bridge overhead

3. **Type Safety**
   - Auto-generated command types
   - Full API type coverage
   - Compile-time validation
   - Better refactoring support

4. **Simplicity**
   - No configuration needed
   - Works out of the box
   - Clear, intuitive API
   - Excellent documentation

## Next Steps

1. Implement core bridge infrastructure
2. Build type generation system
3. Create virtual module injection
4. Port existing functionality
5. Add comprehensive tests
6. Write developer documentation

This design provides a clean, modern API that JavaScript developers will find familiar and pleasant to use, while maintaining the power needed for desktop application development.