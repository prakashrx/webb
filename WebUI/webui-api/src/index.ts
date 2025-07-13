/**
 * WebUI Platform API
 * 
 * Clean, modern JavaScript API for WebUI Platform extensions.
 * Provides panel management, IPC communication, and extension utilities.
 */

import { extension } from './extension.js';
import { panel } from './panel.js';
import { ipc } from './ipc.js';
import { isBridgeAvailable } from './utils.js';

// Main WebUI API object
export const webui = {
  extension,
  panel,
  ipc,
  
  /**
   * Check if the WebUI API is available
   */
  isAvailable: isBridgeAvailable,
  
  /**
   * Get API version
   */
  version: '1.0.0'
};

// Auto-initialize and make globally available
if (typeof window !== 'undefined') {
  // Make webui globally available
  (window as any).webui = webui;
  
  // Log initialization
  if (isBridgeAvailable()) {
    console.log(`WebUI API v${webui.version} initialized for extension: ${extension.getId()}`);
  } else {
    console.warn('WebUI API initialized but bridge not available. Make sure this runs in a WebView2 context.');
  }
}

// Export for ES modules
export { extension, panel, ipc };
export default webui;