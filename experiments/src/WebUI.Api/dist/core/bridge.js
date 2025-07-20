/**
 * Core bridge functionality for WebUI API
 */
let bridge = null;
/**
 * Get the WebView2 host bridge
 * @throws Error if bridge is not available
 */
export function getBridge() {
    if (!bridge) {
        const hostBridge = window.chrome?.webview?.hostObjects?.sync?.api;
        if (!hostBridge) {
            throw new Error('WebUI bridge not available. Make sure this code is running in a WebView2 context.');
        }
        bridge = hostBridge;
    }
    return bridge;
}
/**
 * Check if the WebUI bridge is available
 */
export function isAvailable() {
    return !!window.chrome?.webview?.hostObjects?.sync?.api;
}
/**
 * Generate a unique ID for requests
 */
export function generateId() {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
}
//# sourceMappingURL=bridge.js.map