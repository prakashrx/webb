import { getBridge, generateUUID, safeJsonStringify } from './utils.js';

/**
 * Message service for panel communication
 * Handles both JS → C# (via COM) and C# → JS (via PostMessage)
 */
export class MessageService {
  private handlers = new Map<string, Set<(data?: any) => void>>();
  private handlerRefs = new Map<string, (data?: any) => void>();
  
  constructor() {
    // Listen for messages from C# via PostMessage
    if (window.chrome?.webview?.addEventListener) {
      window.chrome.webview.addEventListener('message', (event) => {
        this._handleMessage(event.data);
      });
    }
  }
  
  /**
   * Send a message to the platform or other panels
   */
  public send(type: string, payload?: any): void {
    const bridge = getBridge();
    const jsonPayload = payload ? safeJsonStringify(payload) : '';
    bridge.Message.Send(type, jsonPayload);
  }
  
  /**
   * Send a message to a specific target panel
   */
  public sendTo(target: string, type: string, payload?: any): void {
    const bridge = getBridge();
    const jsonPayload = payload ? safeJsonStringify(payload) : '';
    bridge.Message.SendTo(target, type, jsonPayload);
  }
  
  /**
   * Broadcast a message to all panels
   */
  public broadcast(type: string, payload?: any): void {
    const bridge = getBridge();
    const jsonPayload = payload ? safeJsonStringify(payload) : '';
    bridge.Message.Broadcast(type, jsonPayload);
  }
  
  /**
   * Listen for messages of a specific type
   */
  public on(type: string, handler: (data?: any) => void): string {
    if (!this.handlers.has(type)) {
      this.handlers.set(type, new Set());
    }
    
    const handlerId = generateUUID();
    this.handlers.get(type)!.add(handler);
    
    // Store handler reference for removal
    this.handlerRefs.set(handlerId, handler);
    
    return handlerId;
  }
  
  /**
   * Remove a message handler
   */
  public off(type: string, handlerId: string): void {
    const handler = this.handlerRefs.get(handlerId);
    if (handler && this.handlers.has(type)) {
      this.handlers.get(type)!.delete(handler);
      this.handlerRefs.delete(handlerId);
      
      // Clean up empty handler sets
      if (this.handlers.get(type)!.size === 0) {
        this.handlers.delete(type);
      }
    }
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
        this.off(responseType, handlerId);
        reject(new Error(`Message request timeout after ${timeout}ms`));
      }, timeout);

      // Listen for response
      const handlerId = this.on(responseType, (response) => {
        clearTimeout(timeoutId);
        this.off(responseType, handlerId);
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
  
  /**
   * Internal: Handle messages from C# via PostMessage
   */
  public _handleMessage(message: any): void {
    if (!message || typeof message.type !== 'string') {
      return;
    }
    
    const { type, data } = message;
    
    // Dispatch to exact type handlers
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
    
    // Dispatch to wildcard handlers (*)
    const wildcardHandlers = this.handlers.get('*');
    if (wildcardHandlers) {
      wildcardHandlers.forEach(handler => {
        try {
          handler(message);
        } catch (error) {
          console.error('Error in wildcard message handler:', error);
        }
      });
    }
    
    // TODO: Support pattern matching (e.g., "data.*", "*.update")
  }
}

// Export singleton instance
export const message = new MessageService();