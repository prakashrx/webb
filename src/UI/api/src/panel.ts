import { getBridge, generateUUID } from './utils.js';

/**
 * Panel management and window controls
 */
export class PanelManager {
  private handlers = new Map<string, string>();

  /**
   * Get the current panel's ID
   */
  public getId(): string {
    const bridge = getBridge();
    return bridge.Panel.GetId();
  }

  /**
   * Get the current panel's title
   */
  public getTitle(): string {
    const bridge = getBridge();
    return bridge.Panel.GetTitle();
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
   * Close the current window or a specific panel
   */
  public close(panelId?: string): void {
    const bridge = getBridge();
    if (panelId) {
      bridge.Panel.ClosePanel(panelId);
    } else {
      bridge.Panel.Close();
    }
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

}

// Export singleton instance
export const panel = new PanelManager();