/**
 * Core invoke functionality for calling C# commands
 */
import type { Commands } from '../types/commands.js';
/**
 * Invoke a command on the C# backend
 */
export declare function invoke<K extends keyof Commands>(command: K, args: Commands[K]['args']): Promise<Commands[K]['returns']>;
export declare function invoke<T = any>(command: string, args?: any): Promise<T>;
//# sourceMappingURL=invoke.d.ts.map