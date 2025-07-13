// Type definitions for WebUI Platform API

export interface ExtensionContext {
  extensionId: string;
  panelId?: string;
}

export interface PanelRegistration {
  id: string;
  url: string;
}

export interface IpcMessage {
  type: string;
  payload?: any;
  from?: string;
  to?: string;
  timestamp?: Date;
}

// COM Bridge interface (mirrors C# HostApiBridge)
export interface HostApiBridge {
  GetExtensionId(): string;
  Panel: {
    RegisterView(id: string, url: string): void;
    Open(id: string): void;
    ClosePanel(id: string): void;
    On(eventType: string, handlerName: string): string;
    Minimize(): void;
    Maximize(): void;
    Restore(): void;
    Close(): void;
    IsMaximized(): boolean;
    OpenDevTools(): void;
  };
  Ipc: {
    Send(type: string, payload: string): void;
    On(type: string, handlerName: string): string;
    Broadcast(type: string, payload: string): void;
  };
}

// Global WebView2 interface
declare global {
  interface Window {
    chrome?: {
      webview?: {
        hostObjects?: {
          api?: HostApiBridge;
        };
      };
    };
  }
}