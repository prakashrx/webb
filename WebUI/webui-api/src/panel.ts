import { getBridge, generateUUID } from './utils.js';
import { extension } from './extension.js';

/**
 * Panel management and window controls
 */
export class PanelManager {
  private handlers = new Map<string, string>();

  /**
   * Register a panel view with the given ID and URL
   */
  public registerView(id: string, url: string): void {
    const bridge = getBridge();
    bridge.Panel.RegisterView(id, url);
  }

  /**
   * Register a Svelte component as a panel (convenience wrapper)
   */
  public registerPanel(id: string, component: any): void {
    // Store the component for later mounting
    (window as any).__webuiPanels = (window as any).__webuiPanels || new Map();
    (window as any).__webuiPanels.set(id, component);
    
    // Register with a special URL that indicates this is a component panel
    this.registerView(id, `component://${id}`);
  }

  /**
   * Open a registered panel
   */
  public open(id: string): void {
    const bridge = getBridge();
    bridge.Panel.Open(id);
  }

  /**
   * Close a specific panel
   */
  public closePanel(id: string): void {
    const bridge = getBridge();
    bridge.Panel.ClosePanel(id);
  }

  /**
   * Listen for panel events
   */
  public on(eventType: string, handler: (data?: any) => void): string {
    const bridge = getBridge();
    const handlerId = generateUUID();
    const handlerName = `panel_${handlerId}`;

    // Store handler for cleanup
    this.handlers.set(handlerId, handlerName);

    // Create global handler function
    (window as any)[handlerName] = (payload?: string) => {
      try {
        const data = payload ? JSON.parse(payload) : undefined;
        handler(data);
      } catch (error) {
        console.error('Panel event handler error:', error);
        handler(payload);
      }
    };

    // Register with bridge
    bridge.Panel.On(eventType, handlerName);
    return handlerId;
  }

  /**
   * Remove an event handler
   */
  public off(handlerId: string): void {
    const handlerName = this.handlers.get(handlerId);
    if (handlerName) {
      delete (window as any)[handlerName];
      this.handlers.delete(handlerId);
    }
  }

  // Window control methods

  /**
   * Minimize the window
   */
  public minimize(): void {
    const bridge = getBridge();
    bridge.Panel.Minimize();
  }

  /**
   * Maximize the window
   */
  public maximize(): void {
    const bridge = getBridge();
    bridge.Panel.Maximize();
  }

  /**
   * Restore the window
   */
  public restore(): void {
    const bridge = getBridge();
    bridge.Panel.Restore();
  }

  /**
   * Close the window
   */
  public close(): void {
    const bridge = getBridge();
    bridge.Panel.Close();
  }

  /**
   * Check if window is maximized
   */
  public isMaximized(): boolean {
    const bridge = getBridge();
    return bridge.Panel.IsMaximized();
  }

  /**
   * Open developer tools
   */
  public openDevTools(): void {
    const bridge = getBridge();
    bridge.Panel.OpenDevTools();
  }

  /**
   * Mount a registered panel component to a container element
   * This is used internally by the platform when loading component-based panels
   */
  public mountPanel(panelId: string, containerId: string): void {
    const panels = (window as any).__webuiPanels;
    if (!panels || !panels.has(panelId)) {
      throw new Error(`Panel '${panelId}' not registered. Did you call registerPanel() in your extension's activate function?`);
    }

    const container = document.getElementById(containerId);
    if (!container) {
      throw new Error(`Container element '${containerId}' not found`);
    }

    const Component = panels.get(panelId);
    try {
      // Mount as Svelte component
      new Component({
        target: container,
        props: {
          // Pass extension context if needed in future
        }
      });
      console.log(`Panel '${panelId}' mounted successfully`);
    } catch (error) {
      console.error(`Failed to mount panel '${panelId}':`, error);
      throw error;
    }
  }
}

// Export singleton instance
export const panel = new PanelManager();