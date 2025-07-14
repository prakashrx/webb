/**
 * Utility functions for WebUI API
 */

/**
 * Generate a UUID for handlers
 */
export function generateUUID(): string {
  return 'xxxx-xxxx-4xxx-yxxx'.replace(/[xy]/g, function(c) {
    const r = Math.random() * 16 | 0;
    const v = c === 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

/**
 * Get URL parameter value
 */
export function getUrlParameter(name: string): string | null {
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get(name);
}

/**
 * Safe JSON parse with fallback
 */
export function safeJsonParse(json: string): any {
  try {
    return JSON.parse(json);
  } catch {
    return json;
  }
}

/**
 * Safe JSON stringify with fallback
 */
export function safeJsonStringify(obj: any): string {
  try {
    return JSON.stringify(obj);
  } catch {
    return String(obj);
  }
}

/**
 * Check if WebUI bridge is available
 */
export function isBridgeAvailable(): boolean {
  return !!(window.chrome?.webview?.hostObjects?.api);
}

/**
 * Get the COM bridge with error handling
 */
export function getBridge() {
  const bridge = window.chrome?.webview?.hostObjects?.api;
  if (!bridge) {
    throw new Error('WebUI bridge not available. Make sure this runs in a WebView2 context.');
  }
  return bridge;
}