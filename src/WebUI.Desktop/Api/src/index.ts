/**
 * WebUI API main entry point
 */

// Core functionality
export { invoke, isAvailable } from './core/index.js';

// Re-export types
export type { Commands } from './types/commands.js';

// Version
export const version = '1.0.0';