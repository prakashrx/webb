# WebUI Platform - POC Implementation Plan (Everything is an Extension)

## 🌟 **Goal: Build a Real End-to-End MVP**

Demonstrate a fully working system where:
- **Workbench UI (toolbar, panel chrome)** is composed of core extensions
- **User extensions** are loaded from HTTP and rendered inside **iframe-isolated panels**
- **IPC** enables communication between all parts (core ⇌ user ⇌ platform)

---

## 📊 **Architecture Snapshot**

```
Workbench Process                     Host Process (WebView2)
├─ Core Extension Loader             ├─ Loads:
├─ User Extension Loader             │  ├─ core.panel-container (renders iframe + chrome)
├─ IPC Router ⇆ Named Pipes        │  ├─ core.main-toolbar (top UI)
└─ Workspace/Process Manager         │  └─ test-extension (loaded remotely)
```

**Key Principles**:
- **Everything Is an Extension**: Platform UI built from core extensions
- **Iframe Isolation**: User panels run in isolated iframes with webui-api.js
- **Central IPC Routing**: All communication flows through workbench router
- **HTTP Extensions**: User extensions served from development servers
- **Clean API Surface**: `webui.panel`, `webui.ipc` namespaces
- **Extension Context**: Each extension gets isolated context with identity

---

## ✅ **Task Breakdown**

### 📅 **Task 1: Implement HostApiBridge (COM Interface)**

**Goal**: Provide JavaScript bridge access to core APIs from inside panel iframes

```csharp
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class HostApiBridge
{
    private readonly IpcTransport _ipcTransport;
    
    public PanelApi Panel { get; }
    public IpcApi Ipc { get; }
    
    public HostApiBridge(string extensionId, IpcTransport ipcTransport)
    {
        _ipcTransport = ipcTransport;
        Panel = new PanelApi(extensionId, ipcTransport);
        Ipc = new IpcApi(extensionId, ipcTransport);
    }
}

[ComVisible(true)]
public class PanelApi
{
    private readonly string _extensionId;
    private readonly IpcTransport _ipc;
    
    public void RegisterView(string panelId, string url)
    {
        _ipc.Send("panel.register", new { 
            extensionId = _extensionId, 
            panelId, 
            url 
        });
    }
    
    public void Open(string panelId)
    {
        _ipc.Send("panel.open", new { 
            extensionId = _extensionId, 
            panelId 
        });
    }
    
    public void On(string eventType, string handlerName)
    {
        _ipc.RegisterHandler($"panel.{eventType}", handlerName);
    }
}

[ComVisible(true)]
public class IpcApi
{
    private readonly string _extensionId;
    private readonly IpcTransport _ipc;
    
    public void Send(string type, string payload)
    {
        _ipc.Send(type, payload, _extensionId);
    }
    
    public void On(string type, string handlerName)
    {
        _ipc.RegisterHandler(type, handlerName);
    }
    
    public void Broadcast(string type, string payload)
    {
        _ipc.Broadcast(type, payload, _extensionId);
    }
}
```

**Subtasks**:
- [ ] Define and expose `HostApiBridge`, `PanelApi`, `IpcApi` via `[ComVisible(true)]`
- [ ] Bridge methods: `registerView`, `open`, `close`, `send`, `on`, `broadcast`
- [ ] Initialize with `extensionId` for proper message routing
- [ ] Wire up message dispatch from iframe to workbench

### 📝 **Task 2: Core JavaScript API Wrapper (`webui-api.js`)**

**Goal**: Provide ergonomic JS API inside every panel iframe

```javascript
// webui-api.js - Auto-injected into every panel iframe
(function() {
    'use strict';
    
    // Get extension identity from URL params
    const urlParams = new URLSearchParams(window.location.search);
    const extensionId = urlParams.get('extensionId');
    const panelId = urlParams.get('panelId');
    
    // Get COM bridge from WebView2
    const bridge = window.chrome?.webview?.hostObjects?.api;
    
    if (!bridge) {
        console.error('WebUI HostApiBridge not found');
        return;
    }
    
    // Handler management
    const handlers = new Map();
    let handlerCounter = 0;
    
    function createHandler(userHandler) {
        const handlerId = `h_${extensionId}_${++handlerCounter}`;
        window[handlerId] = (payload) => {
            try {
                const data = payload ? JSON.parse(payload) : null;
                userHandler(data);
            } catch (error) {
                console.error('Handler error:', error);
            }
        };
        return handlerId;
    }
    
    // Public API
    window.webui = {
        extension: {
            getId: () => extensionId,
            getPanelId: () => panelId
        },
        
        panel: {
            registerView: (id, url) => bridge.Panel.RegisterView(id, url),
            open: (id) => bridge.Panel.Open(id),
            close: (id) => bridge.Panel.Close(id),
            on: (eventType, handler) => {
                const handlerId = createHandler(handler);
                bridge.Panel.On(eventType, handlerId);
                return handlerId;
            }
        },
        
        ipc: {
            send: (type, payload) => {
                const jsonPayload = payload ? JSON.stringify(payload) : '';
                bridge.Ipc.Send(type, jsonPayload);
            },
            on: (type, handler) => {
                const handlerId = createHandler(handler);
                bridge.Ipc.On(type, handlerId);
                return handlerId;
            },
            broadcast: (type, payload) => {
                const jsonPayload = payload ? JSON.stringify(payload) : '';
                bridge.Ipc.Broadcast(type, jsonPayload);
            }
        }
    };
    
    console.log(`WebUI API initialized for extension: ${extensionId}`);
})();
```

**Example Extension Usage**:
```javascript
// In extension's activate.js
export function activate() {
    // Register panels
    webui.panel.registerView('main', 'http://localhost:3001/panel.html?extensionId=test&panelId=main');
    webui.panel.open('main');
    
    // Listen for events
    webui.panel.on('opened', () => console.log('Panel opened'));
    webui.ipc.on('refresh', () => console.log('Refresh received'));
}
```

**Subtasks**:
- [ ] Expose `webui.panel`, `webui.ipc` namespaces
- [ ] Internally wire to `window.chrome.webview.hostObjects.api`
- [ ] Auto-generate UUIDs for `on(...)` handlers
- [ ] Parse `?extensionId=` from query string and attach to `webui.extension.getId()`
- [ ] Setup global message dispatch to route to proper handler ID

### 📦 **Task 3: Basic IPC Transport (Workbench ⇌ Host)**

**Goal**: Enable named-pipe-based routing of JSON messages between processes

```csharp
// Message format
public class IpcMessage
{
    public string Type { get; set; }
    public string Payload { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool ExpectsResponse { get; set; }
}

// Workbench - Central router
public class WorkbenchIpcRouter
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<IpcMessage>> _pendingRequests = new();
    private readonly ConcurrentDictionary<string, NamedPipeServerStream> _hostConnections = new();
    
    public async Task<IpcMessage> SendToHostAsync(string hostId, IpcMessage message)
    {
        if (message.ExpectsResponse)
        {
            var tcs = new TaskCompletionSource<IpcMessage>();
            _pendingRequests[message.CorrelationId] = tcs;
            
            await WriteMessageToHost(hostId, message);
            return await tcs.Task;
        }
        else
        {
            await WriteMessageToHost(hostId, message);
            return null;
        }
    }
    
    public void RouteMessage(IpcMessage message)
    {
        // Handle response correlation
        if (_pendingRequests.TryRemove(message.CorrelationId, out var tcs))
        {
            tcs.SetResult(message);
            return;
        }
        
        // Route to appropriate handler based on message.To
        switch (message.To)
        {
            case "workbench":
                HandleWorkbenchMessage(message);
                break;
            default:
                // Route to another extension/host
                ForwardMessage(message);
                break;
        }
    }
    
    private async Task WriteMessageToHost(string hostId, IpcMessage message)
    {
        if (_hostConnections.TryGetValue(hostId, out var pipe))
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await pipe.WriteAsync(BitConverter.GetBytes(bytes.Length));
            await pipe.WriteAsync(bytes);
        }
    }
}

// Host - Extension runtime
public class HostIpcTransport
{
    private readonly NamedPipeClientStream _pipe;
    private readonly ConcurrentQueue<IpcMessage> _outboundQueue = new();
    private readonly SemaphoreSlim _writeSemaphore = new(1, 1);
    
    public async Task SendToWorkbenchAsync(IpcMessage message)
    {
        _outboundQueue.Enqueue(message);
        await ProcessOutboundQueue();
    }
    
    public void OnMessageFromWorkbench(Action<IpcMessage> handler)
    {
        _ = Task.Run(async () =>
        {
            while (_pipe.IsConnected)
            {
                try
                {
                    var lengthBytes = new byte[4];
                    await _pipe.ReadAsync(lengthBytes, 0, 4);
                    var length = BitConverter.ToInt32(lengthBytes);
                    
                    var messageBytes = new byte[length];
                    await _pipe.ReadAsync(messageBytes, 0, length);
                    
                    var json = Encoding.UTF8.GetString(messageBytes);
                    var message = JsonSerializer.Deserialize<IpcMessage>(json);
                    
                    handler(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"IPC read error: {ex.Message}");
                }
            }
        });
    }
}
```

**Subtasks**:
- [ ] Define `IpcMessage` class with fields: `type`, `payload`, `from`, `to`, `correlationId`
- [ ] Build `WorkbenchIpcRouter` with `RouteMessage()`, `SendToHostAsync()`, `OnMessageFromHost()`
- [ ] Build `HostIpcTransport` with `SendToWorkbenchAsync()`, `OnMessageFromWorkbench()`
- [ ] Implement request/response correlation with `TaskCompletionSource`
- [ ] Add dev logging and error handling hooks

### 🌐 **Task 4: Extension Loader (Core + Remote)**

**Goal**: Load both platform extensions (local disk) and user extensions (HTTP)

```csharp
public class ExtensionLoader
{
    private readonly HttpClient _httpClient = new();
    private readonly Dictionary<string, Extension> _loadedExtensions = new();
    
    public async Task<Extension> LoadCoreExtensionAsync(string extensionPath)
    {
        var manifestPath = Path.Combine(extensionPath, "manifest.json");
        var manifestJson = await File.ReadAllTextAsync(manifestPath);
        var manifest = JsonSerializer.Deserialize<ExtensionManifest>(manifestJson);
        
        var activateScriptPath = Path.Combine(extensionPath, manifest.Main);
        var activateScript = await File.ReadAllTextAsync(activateScriptPath);
        
        return new Extension
        {
            Manifest = manifest,
            ActivateScript = activateScript,
            Type = ExtensionType.Core
        };
    }
    
    public async Task<Extension> LoadUserExtensionAsync(string manifestUrl)
    {
        var manifestJson = await _httpClient.GetStringAsync(manifestUrl);
        var manifest = JsonSerializer.Deserialize<ExtensionManifest>(manifestJson);
        
        var baseUrl = manifestUrl.Substring(0, manifestUrl.LastIndexOf('/'));
        var activateScriptUrl = $"{baseUrl}/{manifest.Main}";
        var activateScript = await _httpClient.GetStringAsync(activateScriptUrl);
        
        return new Extension
        {
            Manifest = manifest,
            ActivateScript = activateScript,
            Type = ExtensionType.User,
            BaseUrl = baseUrl
        };
    }
    
    public void ActivateExtension(Extension extension, WebView2 webView)
    {
        var context = CreateExtensionContext(extension);
        
        // Execute activate function with context
        var script = $@"
            (function() {{
                {extension.ActivateScript}
                if (typeof activate === 'function') {{
                    const context = {JsonSerializer.Serialize(context)};
                    activate(context);
                }}
            }})();
        ";
        
        webView.ExecuteScriptAsync(script);
        _loadedExtensions[extension.Manifest.Id] = extension;
    }
    
    private ExtensionContext CreateExtensionContext(Extension extension)
    {
        return new ExtensionContext
        {
            ExtensionId = extension.Manifest.Id,
            ExtensionType = extension.Type.ToString(),
            BaseUrl = extension.BaseUrl
        };
    }
}

public class ExtensionManifest
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Version { get; set; }
    public string Main { get; set; }
    public List<PanelDefinition> Panels { get; set; } = new();
}

public class PanelDefinition
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string DefaultDock { get; set; } = "main";
}

public class Extension
{
    public ExtensionManifest Manifest { get; set; }
    public string ActivateScript { get; set; }
    public ExtensionType Type { get; set; }
    public string BaseUrl { get; set; }
}

public class ExtensionContext
{
    public string ExtensionId { get; set; }
    public string ExtensionType { get; set; }
    public string BaseUrl { get; set; }
}

public enum ExtensionType
{
    Core,
    User
}
```

**Subtasks**:
- [ ] Parse `manifest.json` for both core and user extensions
- [ ] Fetch `main: activate.js` and `panels[]` definitions
- [ ] Execute `activate(context)` with `context.panel`, `context.ipc`, `context.extensionId`
- [ ] Track loaded extensions with a registry
- [ ] Support deactivation hooks (stubbed for now)

### 🛏️ **Task 5: Core Extension - `core.panel-container`**

**Goal**: Renders panel chrome (titlebar, close button) and wraps an iframe

```javascript
// core/panel-container/manifest.json
{
  "id": "core.panel-container",
  "displayName": "Panel Container",
  "version": "1.0.0",
  "main": "dist/activate.js",
  "type": "core"
}

// core/panel-container/src/PanelContainer.svelte
<script>
  export let panelId;
  export let title;
  export let url;
  export let extensionId;
  
  let iframe;
  
  function close() {
    webui.ipc.send('panel.close', { panelId, extensionId });
  }
  
  function minimize() {
    webui.ipc.send('panel.minimize', { panelId, extensionId });
  }
  
  // Add query params for extension identity
  $: iframeSrc = `${url}?extensionId=${extensionId}&panelId=${panelId}`;
  
  function onIframeLoad() {
    // Inject webui-api.js into iframe
    webui.ipc.send('panel.opened', { panelId, extensionId });
  }
</script>

<div class="panel-container" data-panel-id={panelId}>
  <div class="panel-header">
    <span class="panel-title">{title}</span>
    <div class="panel-actions">
      <button class="panel-btn" on:click={minimize}>−</button>
      <button class="panel-btn" on:click={close}>×</button>
    </div>
  </div>
  <div class="panel-content">
    <iframe 
      bind:this={iframe}
      src={iframeSrc} 
      title={title}
      on:load={onIframeLoad}
      sandbox="allow-scripts allow-same-origin allow-forms"
    ></iframe>
  </div>
</div>

<style>
  .panel-container {
    display: flex;
    flex-direction: column;
    border: 1px solid #e1e4e8;
    border-radius: 6px;
    background: white;
    min-height: 300px;
  }
  
  .panel-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    background: #f6f8fa;
    border-bottom: 1px solid #e1e4e8;
    border-radius: 6px 6px 0 0;
  }
  
  .panel-title {
    font-weight: 600;
    font-size: 14px;
  }
  
  .panel-content {
    flex: 1;
  }
  
  iframe {
    width: 100%;
    height: 100%;
    border: none;
  }
</style>
```

```javascript
// core/panel-container/src/activate.js
import PanelContainer from './PanelContainer.svelte';

export function activate() {
  const containers = new Map();
  
  // Listen for panel registration requests
  webui.ipc.on('panel.register', (data) => {
    createPanelChrome(data.panelId, data.url, data.title, data.extensionId);
  });
  
  // Listen for panel open requests
  webui.ipc.on('panel.open', (data) => {
    showPanel(data.panelId);
  });
  
  // Listen for panel close requests
  webui.ipc.on('panel.close', (data) => {
    removePanel(data.panelId);
  });
  
  function createPanelChrome(panelId, url, title, extensionId) {
    const container = document.createElement('div');
    container.className = 'panel-wrapper';
    document.body.appendChild(container);
    
    const panel = new PanelContainer({
      target: container,
      props: { panelId, title, url, extensionId }
    });
    
    containers.set(panelId, { container, panel });
  }
  
  function showPanel(panelId) {
    const panelData = containers.get(panelId);
    if (panelData) {
      panelData.container.style.display = 'block';
    }
  }
  
  function removePanel(panelId) {
    const panelData = containers.get(panelId);
    if (panelData) {
      panelData.panel.$destroy();
      panelData.container.remove();
      containers.delete(panelId);
    }
  }
}
```

**Subtasks**:
- [ ] Create `panel-container.svelte` with title bar and iframe
- [ ] Accept props: `title`, `url`, `panelId`, `extensionId`
- [ ] Mount `<iframe src=...>` with proper query params (`?extensionId=...&panelId=...`)
- [ ] Send lifecycle events back via `webui.panel.on("opened")`
- [ ] Allow panel resize, close, drag (minimal for POC)

### 📈 **Task 6: Core Extension - `core.main-toolbar`**

**Goal**: Provide basic UI to load remote test extensions and view workspace status

```javascript
// core/main-toolbar/manifest.json
{
  "id": "core.main-toolbar",
  "displayName": "Main Toolbar",
  "version": "1.0.0",
  "main": "dist/activate.js",
  "type": "core",
  "panels": [
    { "id": "toolbar", "title": "Main Toolbar" }
  ]
}

// core/main-toolbar/src/Toolbar.svelte
<script>
  let extensionUrl = 'http://localhost:3001/manifest.json';
  let loadedExtensions = [];
  let status = '';
  
  async function loadExtension() {
    if (!extensionUrl.trim()) return;
    
    try {
      status = 'Loading extension...';
      
      // Request workbench to load the extension
      webui.ipc.send('extension.load', { 
        url: extensionUrl,
        type: 'user' 
      });
      
      status = 'Extension load requested';
    } catch (error) {
      status = `Error: ${error.message}`;
    }
  }
  
  // Listen for extension loaded confirmations
  webui.ipc.on('extension.loaded', (data) => {
    loadedExtensions = [...loadedExtensions, data.manifest];
    status = `Loaded: ${data.manifest.displayName}`;
  });
  
  function openExtensionPanel(extension, panelId) {
    webui.ipc.send('panel.open', {
      extensionId: extension.id,
      panelId: panelId
    });
  }
</script>

<div class="toolbar">
  <div class="toolbar-section">
    <h3>WebUI Platform</h3>
  </div>
  
  <div class="toolbar-section">
    <div class="extension-loader">
      <input 
        bind:value={extensionUrl} 
        placeholder="Extension manifest URL"
        class="url-input"
      />
      <button on:click={loadExtension} class="load-btn">
        Load Extension
      </button>
    </div>
    {#if status}
      <div class="status">{status}</div>
    {/if}
  </div>
  
  <div class="toolbar-section">
    <div class="loaded-extensions">
      <h4>Loaded Extensions:</h4>
      {#each loadedExtensions as ext}
        <div class="extension-item">
          <span class="ext-name">{ext.displayName}</span>
          {#each ext.panels || [] as panel}
            <button 
              on:click={() => openExtensionPanel(ext, panel.id)}
              class="panel-btn"
            >
              {panel.title}
            </button>
          {/each}
        </div>
      {/each}
    </div>
  </div>
</div>

<style>
  .toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 16px;
    background: #24292e;
    color: white;
    border-bottom: 1px solid #444;
  }
  
  .toolbar-section {
    display: flex;
    align-items: center;
    gap: 12px;
  }
  
  .url-input {
    padding: 4px 8px;
    border: 1px solid #555;
    border-radius: 3px;
    background: #2d3339;
    color: white;
    width: 300px;
  }
  
  .load-btn, .panel-btn {
    padding: 4px 12px;
    border: none;
    border-radius: 3px;
    background: #0366d6;
    color: white;
    cursor: pointer;
  }
  
  .status {
    font-size: 12px;
    opacity: 0.8;
  }
  
  .extension-item {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 12px;
  }
  
  .ext-name {
    font-weight: 600;
  }
</style>
```

```javascript
// core/main-toolbar/src/activate.js
import Toolbar from './Toolbar.svelte';

export function activate() {
  // Create toolbar container
  const toolbarContainer = document.createElement('div');
  toolbarContainer.id = 'main-toolbar';
  toolbarContainer.style.position = 'fixed';
  toolbarContainer.style.top = '0';
  toolbarContainer.style.left = '0';
  toolbarContainer.style.right = '0';
  toolbarContainer.style.zIndex = '1000';
  
  document.body.appendChild(toolbarContainer);
  
  // Mount Svelte toolbar
  new Toolbar({
    target: toolbarContainer
  });
  
  // Adjust body margin for toolbar
  document.body.style.marginTop = '60px';
}
```

**Subtasks**:
- [ ] Create `toolbar.svelte` with a "Load Extension" button
- [ ] Trigger IPC call to workbench: `webui.ipc.send("extension.load", { url: "..." })`
- [ ] Show workspace name, loaded extensions, panel buttons
- [ ] Optional: Show workspace status, layout controls

### 🚀 **Task 7: User Test Extension (`test-extension`)**

**Goal**: Simple remote extension served from `localhost:3001`

**Folder Structure**:
```
test-extension/
├─ manifest.json
├─ dist/
│  ├─ activate.js
│  └─ panel.html
├─ src/
│  ├─ activate.js
│  └─ TestPanel.svelte
├─ vite.config.js
└─ package.json
```

```json
// manifest.json
{
  "id": "test-extension",
  "displayName": "Test Extension",
  "version": "1.0.0",
  "main": "dist/activate.js",
  "panels": [
    { "id": "main", "title": "Test Panel" }
  ]
}
```

```javascript
// src/activate.js
export function activate() {
  // Register panel with iframe URL
  webui.panel.registerView('main', 'http://localhost:3001/panel.html');
  webui.panel.open('main');
  
  // Listen for refresh events
  webui.ipc.on('refresh', () => {
    console.log('Refresh received in test extension');
  });
  
  // Send ready signal
  webui.ipc.send('extension.ready', { 
    extensionId: webui.extension.getId() 
  });
}
```

```html
<!-- dist/panel.html -->
<!DOCTYPE html>
<html>
<head>
  <title>Test Panel</title>
  <script src="http://localhost:3001/webui-api.js"></script>
</head>
<body>
  <div id="app"></div>
  
  <script type="module">
    import TestPanel from './TestPanel.js';
    
    // Wait for webui API to initialize
    if (window.webui) {
      new TestPanel({
        target: document.getElementById('app')
      });
    }
  </script>
</body>
</html>
```

```svelte
<!-- src/TestPanel.svelte -->
<script>
  let message = 'Hello from Test Extension!';
  let counter = 0;
  
  function sendMessage() {
    webui.ipc.send('test.message', { 
      counter: counter++,
      timestamp: new Date().toISOString()
    });
  }
  
  function sendToWorkbench() {
    webui.ipc.send('test.ping', { source: 'test-panel' });
  }
  
  // Listen for messages
  webui.ipc.on('test.pong', (data) => {
    console.log('Received pong:', data);
    message = `Pong received at ${new Date().toLocaleTimeString()}`;
  });
</script>

<div class="test-panel">
  <h2>Test Extension Panel</h2>
  <p>Extension ID: {webui.extension.getId()}</p>
  <p>Panel ID: {webui.extension.getPanelId()}</p>
  
  <div class="controls">
    <button on:click={sendMessage}>Send IPC Message</button>
    <button on:click={sendToWorkbench}>Ping Workbench</button>
  </div>
  
  <div class="status">
    {message}
  </div>
</div>

<style>
  .test-panel {
    padding: 20px;
    font-family: Arial, sans-serif;
  }
  
  .controls {
    margin: 20px 0;
  }
  
  .controls button {
    margin-right: 10px;
    padding: 8px 16px;
    background: #0366d6;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
  }
  
  .status {
    padding: 10px;
    background: #f6f8fa;
    border-radius: 4px;
    margin-top: 20px;
  }
</style>
```

**Subtasks**:
- [ ] In `activate.js`: `registerView('main', 'http://localhost:3001/panel.html')`, `open('main')`, `ipc.on('refresh', handler)`
- [ ] In `panel.html`: Load `webui-api.js`, initialize with `extensionId` and connect to HostApiBridge
- [ ] Render `TestPanel.svelte` with IPC testing controls
- [ ] Set up Vite dev server with live reload

### 📊 **Task 8: End-to-End Testing & Lifecycle Events**

**Goal**: Prove it all works together

**Test Sequence**:
1. **Start test extension dev server**: `npm run dev` (serves on `localhost:3001`)
2. **Launch Workbench**: Load `core.panel-container`, `core.main-toolbar`, etc.
3. **Load test extension**: Use toolbar to load `http://localhost:3001/manifest.json`
4. **Open panel**: Click "Test Panel" button in toolbar
5. **Verify iframe loads**: Panel chrome wraps iframe with proper query params
6. **Test IPC**: Click buttons in test panel, verify messages route correctly
7. **Test lifecycle**: Close panel, verify cleanup and lifecycle events

**Success Criteria**:
- [ ] Load and activate core extensions from disk
- [ ] Load and activate test extension from HTTP
- [ ] Register iframe panel views per extension
- [ ] IPC works between iframe ⇌ host ⇌ workbench ⇌ other extensions
- [ ] Lifecycle events (`opened`, `closed`) fire correctly
- [ ] All code is readable, testable, and incrementally extensible

---

## ✅ **Deliverables**

After completing all tasks, you will have:

- **Working Extension Architecture**: Everything is an extension, including platform UI
- **Core Extensions**: `core.panel-container`, `core.main-toolbar` providing platform chrome
- **Extension Loader**: Unified loading for both core (disk) and user (HTTP) extensions
- **COM Bridge**: Clean `webui.panel`, `webui.ipc` API accessible from extension iframes
- **IPC Transport**: Named pipe communication with central routing through workbench
- **Test Extension**: Complete user extension with Svelte components and live reload
- **End-to-End Validation**: Full workflow from extension development to deployment

**Technical Achievements**:
- ✅ Iframe-based panel isolation with query parameter identity
- ✅ UUID-based handler management for thread safety
- ✅ Thread-safe IPC with correlation IDs and proper async patterns
- ✅ Panel lifecycle events (opened, closed) with error bubbling
- ✅ Clean separation between core platform and user extensions
- ✅ Development workflow with HTTP-served extensions and live reload

---

## 🔧 **Key Refinements for Production-Ready POC**

### **1. Iframe-Based Panel Isolation**
- **Problem**: Raw HTML injection is limited and doesn't support complex extensions
- **Solution**: Use `<iframe src="http://localhost:3001/panel.html">` for proper isolation
- **Benefits**: Aligns with Vite live reload, supports complex bundling, provides security

### **2. UUID-Based Handler Management**
- **Problem**: String-based handler names are brittle and not thread-safe
- **Solution**: Generate UUIDs internally: `"h_" + crypto.randomUUID()`
- **Benefits**: Thread-safe, automatic cleanup, no global namespace pollution

### **3. Thread-Safe IPC with Correlation**
- **Problem**: Async message handling can cause race conditions
- **Solution**: Use `TaskCompletionSource` for request/response correlation
- **Benefits**: Proper async patterns, no blocking, reliable message delivery

### **4. Panel Lifecycle Events**
- **Problem**: Extensions need to know when panels are opened/closed
- **Solution**: Add `context.panel.on('opened')` and `context.panel.on('closed')`
- **Benefits**: Enables proper resource management and state synchronization

### **5. Error Bubbling and Reporting**
- **Problem**: Extension errors are silent and hard to debug
- **Solution**: Bubble errors via IPC with stack traces
- **Benefits**: Better developer experience, easier debugging

---

## 🎨 **Elegant API Design**

### **JavaScript Side (Extension)**
```javascript
// Beautiful simplicity with proper namespace structure
export function activate(context) {
  context.panel.registerView("test", "http://localhost:3001/panel.html");
  context.panel.open("test");
  
  // Panel lifecycle events
  context.panel.on("opened", () => console.log("Panel opened"));
  context.panel.on("closed", () => console.log("Panel closed"));
  
  // IPC with proper cleanup
  const refreshHandler = context.ipc.on("refresh", () => {
    // Handle refresh
  });
  
  context.ipc.send("ready", { extension: "test" });
}

// Clean namespace access
webui.panel.registerView("test", "http://localhost:3001/panel.html");
webui.ipc.send("message", payload);
```

### **C# Side (Implementation)**
```csharp
// Clean, maintainable modular architecture
[ComVisible(true)]
public class HostApiBridge
{
    public PanelApi Panel { get; }
    public IpcApi Ipc { get; }
    public ExtensionContext Context { get; }
}

[ComVisible(true)]
public class PanelApi
{
    private readonly Dictionary<string, string> _panels = new();
    private readonly IpcTransport _ipc;
    
    public void RegisterView(string id, string html)
    {
        _panels[id] = html;
        _ipc.Send("panel.registered", new { id, html });
    }
}
```

---

## 🚀 **Implementation Strategy**

### **Phase 1: Core Infrastructure** (Tasks 1-2)
- Build HostApiBridge and IPC transport
- Focus on clean, simple implementation
- Thorough testing of basic functionality

### **Phase 2: Extension Loading** (Tasks 3-4) 
- Implement extension loader and panel containers
- Keep manifest schema minimal
- Test with static HTML first

### **Phase 3: End-to-End** (Tasks 5-6)
- Create test extension with Svelte
- Validate complete workflow
- Refine based on developer experience

---

## 📏 **Quality Standards**

### **Code Quality**
- **Readable**: Clear variable names, logical structure
- **Maintainable**: Single responsibility, loose coupling
- **Testable**: Easy to unit test individual components
- **Documented**: Clear XML comments on public APIs

### **Architecture Quality**
- **Simple**: Obvious how things work
- **Elegant**: Minimal complexity for maximum functionality
- **Extensible**: Easy to add features later
- **Robust**: Handles errors gracefully

---

## 🎯 **Next Action**

**Start with Task 1**: Implement HostApiBridge (COM Interface)

**Focus**: Build the foundational COM bridge that enables JavaScript extensions to communicate with the platform through clean, modular APIs.

**Implementation Order**:
1. **Tasks 1-3**: Core infrastructure (COM bridge, JavaScript wrapper, IPC transport)
2. **Task 4**: Extension loader for both core and user extensions
3. **Tasks 5-6**: Core extensions that provide platform UI
4. **Task 7**: Test extension to validate the complete workflow
5. **Task 8**: End-to-end testing and validation

**Key Success Criteria**:
- ✅ Core extensions provide all platform UI (toolbar, panel chrome)
- ✅ User extensions load from HTTP with live reload
- ✅ Iframe isolation works with proper extension identity
- ✅ IPC routing enables communication between all components
- ✅ Clean, maintainable codebase ready for future enhancements

---

*This is a production-aligned POC that proves the full WebUI architecture can be brought to life with minimal complexity and clean layering.* 