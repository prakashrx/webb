import { getBridge, generateUUID, safeJsonStringify } from './utils.js';
import { extension } from './extension.js';
import type { IpcMessage } from './types.js';

/**
 * Inter-process communication manager
 */
export class IpcManager {
  private handlers = new Map<string, string>();

  /**
   * Send a message to the platform or other extensions
   */
  public send(type: string, payload?: any): void {
    const bridge = getBridge();
    const jsonPayload = payload ? safeJsonStringify(payload) : '';
    bridge.Ipc.Send(type, jsonPayload);
  }

  /**
   * Listen for IPC messages of a specific type
   */
  public on(type: string, handler: (data?: any) => void): string {
    const bridge = getBridge();
    const handlerId = generateUUID();
    const handlerName = `ipc_${handlerId}`;

    // Store handler for cleanup
    this.handlers.set(handlerId, handlerName);

    // Create global handler function
    (window as any)[handlerName] = (payload?: string) => {
      try {
        const data = payload ? JSON.parse(payload) : undefined;
        handler(data);
      } catch (error) {
        console.error('IPC handler error:', error);
        handler(payload);
      }
    };

    // Register with bridge
    bridge.Ipc.On(type, handlerName);
    return handlerId;
  }

  /**
   * Remove an IPC handler
   */
  public off(handlerId: string): void {
    const handlerName = this.handlers.get(handlerId);
    if (handlerName) {
      delete (window as any)[handlerName];
      this.handlers.delete(handlerId);
    }
  }

  /**
   * Broadcast a message to all extensions
   */
  public broadcast(type: string, payload?: any): void {
    const bridge = getBridge();
    const jsonPayload = payload ? safeJsonStringify(payload) : '';
    bridge.Ipc.Broadcast(type, jsonPayload);
  }

  /**
   * Send a message and wait for a response (promise-based)
   */
  public async request(type: string, payload?: any, timeout: number = 5000): Promise<any> {
    return new Promise((resolve, reject) => {
      const correlationId = generateUUID();
      const responseType = `${type}.response.${correlationId}`;
      
      // Set up timeout
      const timeoutId = setTimeout(() => {
        this.off(handlerId);
        reject(new Error(`IPC request timeout after ${timeout}ms`));
      }, timeout);

      // Listen for response
      const handlerId = this.on(responseType, (response) => {
        clearTimeout(timeoutId);
        this.off(handlerId);
        resolve(response);
      });

      // Send request with correlation ID
      const requestPayload = {
        correlationId,
        data: payload
      };
      
      this.send(`${type}.request`, requestPayload);
    });
  }
}

// Export singleton instance
export const ipc = new IpcManager();