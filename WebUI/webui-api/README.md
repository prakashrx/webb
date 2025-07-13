# WebUI API

Modern TypeScript API for WebUI Platform extensions.

## Features

- **Type-safe** - Full TypeScript support with type definitions
- **Modern** - ES2020+ with clean async/await patterns  
- **Modular** - Tree-shakeable ES modules
- **Simple** - One import, clean API surface

## Installation

```html
<script src="webui-api.js"></script>
```

## Usage

```javascript
// Extension identity
const extensionId = webui.extension.getId();

// Panel management
webui.panel.registerView('main', 'http://localhost:3001/panel.html');
webui.panel.open('main');

// Window controls
webui.panel.minimize();
webui.panel.maximize();
webui.panel.openDevTools();

// IPC communication
webui.ipc.send('message', { data: 'Hello!' });
webui.ipc.on('response', (data) => console.log(data));
webui.ipc.broadcast('global-event', { timestamp: Date.now() });

// Promise-based requests
const response = await webui.ipc.request('get-data', { id: 123 });
```

## Build

```bash
npm install
npm run build
```

## Development

```bash
npm run dev  # Watch mode
```