/**
 * WebView2 host object bridge types
 */

export interface HostApiBridge {
  Core: CoreApi;
}

export interface CoreApi {
  InvokeCommand(command: string, argsJson: string): void;
}

// WebView2 global types
declare global {
  interface Window {
    chrome?: {
      webview?: {
        hostObjects?: {
          sync?: {
            api?: HostApiBridge;
          };
        };
        postMessage?(message: any): void;
        addEventListener?(type: 'message', listener: (event: MessageEvent) => void): void;
        removeEventListener?(type: 'message', listener: (event: MessageEvent) => void): void;
      };
    };
  }
}

export interface InvokeRequest {
  id: string;
  command: string;
  args?: any;
}

export interface InvokeResponse {
  id: string;
  result?: any;
  error?: string;
}