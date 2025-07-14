import { getBridge, getUrlParameter } from './utils.js';
import type { ExtensionContext } from './types.js';

/**
 * Extension identity and context management
 */
export class ExtensionManager {
  private _context: ExtensionContext | null = null;

  /**
   * Get the current extension context
   */
  public getContext(): ExtensionContext {
    if (!this._context) {
      this._context = this.initializeContext();
    }
    return this._context;
  }

  /**
   * Get the extension ID
   */
  public getId(): string {
    return this.getContext().extensionId;
  }

  /**
   * Get the panel ID (if available)
   */
  public getPanelId(): string | undefined {
    return this.getContext().panelId;
  }

  /**
   * Initialize extension context from URL parameters or bridge
   */
  private initializeContext(): ExtensionContext {
    // Try to get from URL parameters first (for iframe panels)
    const extensionId = getUrlParameter('extensionId');
    const panelId = getUrlParameter('panelId');

    if (extensionId) {
      return {
        extensionId,
        panelId: panelId || undefined
      };
    }

    // Fallback to bridge
    try {
      const bridge = getBridge();
      return {
        extensionId: bridge.GetExtensionId()
      };
    } catch (error) {
      throw new Error('Unable to determine extension identity. Make sure extensionId is provided via URL parameter or bridge.');
    }
  }
}

// Export singleton instance
export const extension = new ExtensionManager();