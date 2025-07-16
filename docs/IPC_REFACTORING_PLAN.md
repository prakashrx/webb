# IPC Refactoring Plan

## Overview

This document outlines the refactoring of the IPC (Inter-Process Communication) system in WebUI Platform to support multi-process architecture while maintaining simplicity and elegance.

## Background

The current IPC system was designed with multi-process support in mind but needs refinement to properly abstract transport mechanisms and enable future extension hosting in separate processes.

### Current Architecture Issues

1. **IpcTransport is not abstract** - It's a concrete class with in-memory handlers
2. **Single shared transport** - All panels share the same IpcTransport instance
3. **No process boundaries** - Direct method calls instead of serialized messages
4. **JavaScript callback problem** - IpcApi can't invoke JS handlers properly
5. **Mixed responsibilities** - IpcRouter handles both routing AND request/response

## Naming Changes

### Current → New Names
- `IpcRouter` → `MessageBus`
- `IpcTransport` → `InProcessChannel`
- `IMessageTransport` (new interface) → `IMessageChannel`
- `IpcApi` → `MessageApi`

### Future Channel Implementations
- `NamedPipeChannel` - For Windows cross-process IPC
- `TcpChannel` - For network-based communication
- `WebSocketChannel` - For real-time streaming

## Architecture Design

### Multi-Process Architecture Vision
```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Panel A   │     │   Panel B   │     │ Extension C │
│  (Process 1)│     │  (Process 1)│     │ (Process 2) │
└──────┬──────┘     └──────┬──────┘     └──────┬──────┘
       │                   │                   │
       ▼                   ▼                   ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ MessageBus  │     │ MessageBus  │     │ MessageBus  │
└──────┬──────┘     └──────┬──────┘     └──────┬──────┘
       │                   │                   │
       ▼                   ▼                   ▼
┌──────────────────────────────────────────────────────┐
│              IMessageChannel (interface)              │
├───────────────┬───────────────┬─────────────────────┤
│InProcessChannel│NamedPipeChannel│   TcpChannel      │
└───────────────┴───────────────┴─────────────────────┘
```

### Message Flow Design

```
JavaScript → C#: COM API (MessageApi)
C# → JavaScript: WebView2 PostMessage API

┌─────────────────┐        ┌─────────────────┐
│   JavaScript    │        │       C#        │
│                 │        │                 │
│  webui.send() ──┼──COM──▶│  MessageApi     │
│                 │        │       ↓         │
│                 │        │  MessageBus     │
│                 │        │       ↓         │
│  on('message') ◀┼PostMsg─┤  Panel.Post()   │
│                 │        │                 │
└─────────────────┘        └─────────────────┘
```

## Implementation Steps

### Step 1: Create IMessageChannel Interface

```csharp
// New interface for transport abstraction
public interface IMessageChannel : IDisposable
{
    string ChannelId { get; }
    
    // Send message to specific target or broadcast
    Task SendAsync(ChannelMessage message);
    
    // Subscribe to messages
    IDisposable Subscribe(string pattern, Func<ChannelMessage, Task> handler);
    
    // Events
    event EventHandler<ChannelMessage> MessageReceived;
    event EventHandler<Exception> ErrorOccurred;
}

public record ChannelMessage(
    string Id,
    string Source,      // "process.panel" format
    string? Target,     // null for broadcast
    string Type,
    byte[] Payload,
    Dictionary<string, string>? Headers = null
);
```

### Step 2: Refactor IpcTransport to InProcessChannel

```csharp
// Rename and implement interface
public class InProcessChannel : IMessageChannel
{
    private readonly ConcurrentDictionary<string, List<Func<ChannelMessage, Task>>> _handlers = new();
    
    public string ChannelId { get; } = Guid.NewGuid().ToString();
    
    public event EventHandler<ChannelMessage>? MessageReceived;
    public event EventHandler<Exception>? ErrorOccurred;
    
    public async Task SendAsync(ChannelMessage message)
    {
        // In-process: direct routing
        var handlers = GetHandlers(message.Type, message.Target);
        
        foreach (var handler in handlers)
        {
            try
            {
                await handler(message);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
            }
        }
        
        MessageReceived?.Invoke(this, message);
    }
    
    public IDisposable Subscribe(string pattern, Func<ChannelMessage, Task> handler)
    {
        _handlers.AddOrUpdate(pattern, 
            new List<Func<ChannelMessage, Task>> { handler },
            (_, list) => { list.Add(handler); return list; });
            
        return new Subscription(() => Unsubscribe(pattern, handler));
    }
    
    private List<Func<ChannelMessage, Task>> GetHandlers(string type, string? target)
    {
        var matchingHandlers = new List<Func<ChannelMessage, Task>>();
        
        // Match exact type
        if (_handlers.TryGetValue(type, out var exactHandlers))
            matchingHandlers.AddRange(exactHandlers);
            
        // Match wildcard patterns
        foreach (var kvp in _handlers.Where(h => h.Key.Contains("*")))
        {
            if (MatchesPattern(kvp.Key, type))
                matchingHandlers.AddRange(kvp.Value);
        }
        
        return matchingHandlers;
    }
}
```

### Step 3: Simplify MessageBus (formerly IpcRouter)

```csharp
public class MessageBus : IDisposable
{
    private readonly IMessageChannel _channel;
    private readonly string _processId;
    private readonly string _panelId;
    private readonly ConcurrentDictionary<string, Func<byte[], Task>> _handlers = new();
    
    public MessageBus(IMessageChannel channel, string processId, string panelId)
    {
        _channel = channel;
        _processId = processId;
        _panelId = panelId;
        
        // Subscribe to messages for this panel
        _channel.Subscribe($"{_processId}.{_panelId}.*", HandleMessage);
        _channel.Subscribe("*.broadcast.*", HandleMessage);
    }
    
    public async Task SendAsync(string type, object? data = null, string? target = null)
    {
        var message = new ChannelMessage(
            Id: Guid.NewGuid().ToString(),
            Source: $"{_processId}.{_panelId}",
            Target: target,
            Type: type,
            Payload: JsonSerializer.SerializeToUtf8Bytes(data ?? new { })
        );
        
        await _channel.SendAsync(message);
    }
    
    public void On<T>(string type, Func<T?, Task> handler)
    {
        _handlers[type] = async (payload) =>
        {
            var data = JsonSerializer.Deserialize<T>(payload);
            await handler(data);
        };
    }
    
    private async Task HandleMessage(ChannelMessage message)
    {
        if (_handlers.TryGetValue(message.Type, out var handler))
        {
            await handler(message.Payload);
        }
    }
    
    public void Dispose()
    {
        _handlers.Clear();
    }
}
```

### Step 4: Create Clean MessageApi (formerly IpcApi)

```csharp
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class MessageApi
{
    private readonly MessageBus _bus;
    
    public MessageApi(MessageBus bus)
    {
        _bus = bus;
    }
    
    // JavaScript to C# communication only
    public void Send(string type, string payload)
    {
        var data = ParsePayload(payload);
        _bus.SendAsync(type, data).GetAwaiter().GetResult();
    }
    
    public void SendTo(string target, string type, string payload)
    {
        var data = ParsePayload(payload);
        _bus.SendAsync(type, data, target).GetAwaiter().GetResult();
    }
    
    public void Broadcast(string type, string payload)
    {
        var data = ParsePayload(payload);
        _bus.SendAsync(type, data, "*").GetAwaiter().GetResult();
    }
    
    private object? ParsePayload(string payload)
    {
        if (string.IsNullOrEmpty(payload)) return null;
        
        try
        {
            return JsonSerializer.Deserialize<object>(payload);
        }
        catch
        {
            return payload; // Return as string if not valid JSON
        }
    }
    
    // Note: No On() method - C# to JS uses PostMessage instead
}
```

### Step 5: Update Panel Class with PostMessage

```csharp
public class Panel : IPanel
{
    private readonly MessageBus _bus;
    private BrowserWindow? _window;
    
    public Panel(string id, MessageBus bus, PanelDefinition definition)
    {
        Id = id;
        _bus = bus;
        Definition = definition;
    }
    
    public async Task InitializeAsync()
    {
        // Subscribe to all messages for this panel
        _bus.On<object>("*", async (data) =>
        {
            await PostMessageToJavaScriptAsync(data);
        });
        
        // Subscribe to typed messages
        _bus.OnTyped<PanelMessage>(async (type, message) =>
        {
            var jsMessage = new { type, data = message };
            await PostMessageToJavaScriptAsync(jsMessage);
        });
    }
    
    private async Task PostMessageToJavaScriptAsync(object message)
    {
        if (_window?.WebView?.CoreWebView2 != null)
        {
            var json = JsonSerializer.Serialize(message);
            await _window.WebView.CoreWebView2.PostWebMessageAsJsonAsync(json);
        }
    }
    
    // Remove ExecuteScriptAsync - no longer needed for messaging
}
```

### Step 6: Update WindowManager

```csharp
public class WindowManager
{
    private readonly IMessageChannel _channel;
    private readonly string _processId = "main";
    
    public WindowManager(string uiContentPath)
    {
        _uiContentPath = uiContentPath;
        
        // Use in-process channel for now
        _channel = new InProcessChannel();
        
        // Future: Check if extension host is needed
        // _channel = IsExtensionProcess() 
        //     ? new NamedPipeChannel() 
        //     : new InProcessChannel();
    }
    
    public async Task<IPanel> CreatePanelAsync(string panelId)
    {
        var definition = _panelDefinitions[panelId];
        var bus = new MessageBus(_channel, _processId, panelId);
        var panel = new Panel(panelId, bus, definition);
        
        _panels[panelId] = panel;
        
        return panel;
    }
}
```

### Step 7: Update JavaScript API with PostMessage

```typescript
// types.ts
export interface WebUIMessage {
  type: string;
  data?: any;
}

// messaging.ts
export class MessageManager {
  private handlers = new Map<string, Set<(data?: any) => void>>();
  
  constructor() {
    // Listen for messages from C# via PostMessage
    window.chrome.webview.addEventListener('message', (event) => {
      const message: WebUIMessage = event.data;
      this.dispatch(message.type, message.data);
    });
  }
  
  // Send message to C# (JS → C#)
  public send(type: string, payload?: any): void {
    const bridge = getBridge();
    bridge.Messaging.Send(type, safeJsonStringify(payload));
  }
  
  // Send to specific target
  public sendTo(target: string, type: string, payload?: any): void {
    const bridge = getBridge();
    bridge.Messaging.SendTo(target, type, safeJsonStringify(payload));
  }
  
  // Listen for messages (C# → JS)
  public on(type: string, handler: (data?: any) => void): string {
    if (!this.handlers.has(type)) {
      this.handlers.set(type, new Set());
    }
    
    const handlerId = generateUUID();
    this.handlers.get(type)!.add(handler);
    
    // Store handler reference for removal
    (this as any)[`_handler_${handlerId}`] = handler;
    
    return handlerId;
  }
  
  // Remove handler
  public off(type: string, handlerId: string): void {
    const handler = (this as any)[`_handler_${handlerId}`];
    if (handler && this.handlers.has(type)) {
      this.handlers.get(type)!.delete(handler);
      delete (this as any)[`_handler_${handlerId}`];
    }
  }
  
  // Broadcast to all panels
  public broadcast(type: string, payload?: any): void {
    const bridge = getBridge();
    bridge.Messaging.Broadcast(type, safeJsonStringify(payload));
  }
  
  // Internal: dispatch messages from C#
  private dispatch(type: string, data: any): void {
    // Exact match
    const handlers = this.handlers.get(type);
    if (handlers) {
      handlers.forEach(handler => {
        try {
          handler(data);
        } catch (error) {
          console.error(`Error in message handler for ${type}:`, error);
        }
      });
    }
    
    // Wildcard patterns (future enhancement)
    // TODO: Support patterns like "data.*" or "*.update"
  }
}

// Export singleton
export const messaging = new MessageManager();
```

### Step 8: Update HostApiBridge

```csharp
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class HostApiBridge
{
    public PanelApi Panel { get; }
    public MessageApi Messaging { get; } // Renamed from Ipc
    
    public HostApiBridge(IPanel panel, MessageBus bus)
    {
        Panel = new PanelApi(panel);
        Messaging = new MessageApi(bus);
    }
}
```

## Benefits of This Design

1. **Clean Separation**: 
   - JS → C#: COM API (MessageApi)
   - C# → JS: WebView2 PostMessage
   
2. **No Script Injection**: No more ExecuteScriptAsync with handler names

3. **Type Safety**: Structured messages through PostMessage

4. **Standard Pattern**: Uses WebView2's intended messaging API

5. **Process Ready**: Can swap channels without changing MessageBus

6. **Testable**: Can mock IMessageChannel for testing

7. **Performance**: PostMessage is optimized by WebView2

## API Usage Examples

### JavaScript Side
```javascript
// Send message to C#
webui.messaging.send('data.update', { symbol: 'AAPL', price: 150 });

// Send to specific panel
webui.messaging.sendTo('panel2', 'data.request', { symbol: 'AAPL' });

// Listen for messages from C#
webui.messaging.on('data.update', (data) => {
  console.log('Received:', data);
});

// Broadcast to all panels
webui.messaging.broadcast('theme.changed', { theme: 'dark' });
```

### C# Side
```csharp
// Send to JavaScript
await panel.PostMessageAsync("data.update", new { symbol = "AAPL", price = 150 });

// Listen for messages from JavaScript
bus.On<DataUpdate>("data.update", async (data) => {
    Console.WriteLine($"Received: {data.Symbol} @ {data.Price}");
});
```

## Future Extensions

### Named Pipe Channel (Windows)
```csharp
public class NamedPipeChannel : IMessageChannel
{
    private readonly NamedPipeServerStream _server;
    private readonly NamedPipeClientStream _client;
    
    public async Task SendAsync(ChannelMessage message)
    {
        var bytes = MessageSerializer.Serialize(message);
        await _client.WriteAsync(bytes);
    }
}
```

### Extension Host Process
```csharp
public class ExtensionHost
{
    private readonly Process _process;
    private readonly IMessageChannel _channel;
    
    public async Task LoadExtensionAsync(string extensionPath)
    {
        _process = Process.Start(new ProcessStartInfo
        {
            FileName = "WebUI.ExtensionHost.exe",
            Arguments = extensionPath,
            UseShellExecute = false
        });
        
        _channel = new NamedPipeChannel($"webui-extension-{_process.Id}");
        await _channel.ConnectAsync();
    }
}
```

## Migration Path

1. **Phase 1**: Implement new architecture alongside old
2. **Phase 2**: Update all panels to use new messaging API
3. **Phase 3**: Remove old IpcApi.On() method
4. **Phase 4**: Add extension host support
5. **Phase 5**: Implement cross-process channels

## Summary

This refactoring:
- Maintains simplicity while preparing for multi-process
- Uses WebView2's native PostMessage for C# → JS communication
- Removes clunky script injection patterns
- Provides clean, type-safe messaging in both directions
- Sets foundation for future extension isolation