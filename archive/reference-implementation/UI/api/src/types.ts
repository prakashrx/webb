// Type definitions for WebUI Platform API

export interface PanelContext {
  panelId: string;
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
  Panel: {
    Open(id: string): void;
    Close(): void;
    ClosePanel(id: string): void;
    Minimize(): void;
    Maximize(): void;
    Restore(): void;
    IsMaximized(): boolean;
    OpenDevTools(): void;
    GetId(): string;
    GetTitle(): string;
  };
  Message: {
    Send(type: string, payload: string): void;
    SendTo(target: string, type: string, payload: string): void;
    Broadcast(type: string, payload: string): void;
  };
}

// WebView2 message event
export interface WebViewMessageEvent {
  data: any;
}

// Global WebView2 interface
declare global {
  interface Window {
    chrome?: {
      webview?: {
        hostObjects?: {
          api?: HostApiBridge;
        };
        addEventListener?(event: 'message', handler: (event: WebViewMessageEvent) => void): void;
        removeEventListener?(event: 'message', handler: (event: WebViewMessageEvent) => void): void;
      };
    };
  }
}