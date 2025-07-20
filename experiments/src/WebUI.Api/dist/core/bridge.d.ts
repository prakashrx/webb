/**
 * Core bridge functionality for WebUI API
 */
import type { HostApiBridge } from '../types/bridge.js';
/**
 * Get the WebView2 host bridge
 * @throws Error if bridge is not available
 */
export declare function getBridge(): HostApiBridge;
/**
 * Check if the WebUI bridge is available
 */
export declare function isAvailable(): boolean;
/**
 * Generate a unique ID for requests
 */
export declare function generateId(): string;
//# sourceMappingURL=bridge.d.ts.map