/**
 * WebUI API main entry point
 */

// Core functionality
export { invoke, isAvailable } from './core/index.ts';

// Re-export types
export type { Commands } from './types/commands.ts';

// Version
export const version = '1.0.0';