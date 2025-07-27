/**
 * Core invoke functionality for calling C# commands
 */

import { getBridge, generateId } from './bridge.js';
import type { Commands } from '../types/commands.js';
import type { InvokeResponse } from '../types/bridge.js';

// Store pending requests
const pendingRequests = new Map<string, {
  resolve: (value: any) => void;
  reject: (reason: any) => void;
  timeout: number;
}>();

// Initialize message listener once
let listenerInitialized = false;

function initializeListener() {
  if (listenerInitialized || !window.chrome?.webview?.addEventListener) {
    return;
  }

  window.chrome.webview.addEventListener('message', (event) => {
    const response = event.data as InvokeResponse;
    
    if (response && response.id) {
      const pending = pendingRequests.get(response.id);
      if (pending) {
        clearTimeout(pending.timeout);
        pendingRequests.delete(response.id);
        
        if (response.error) {
          pending.reject(new Error(response.error));
        } else {
          pending.resolve(response.result);
        }
      }
    }
  });

  listenerInitialized = true;
}

/**
 * Invoke a command on the C# backend
 */
export async function invoke<K extends keyof Commands>(
  command: K,
  args: Commands[K]['args']
): Promise<Commands[K]['returns']>;

export async function invoke<T = any>(
  command: string,
  args?: any
): Promise<T>;

export async function invoke(
  command: string,
  args?: any
): Promise<any> {
  // Ensure listener is initialized
  initializeListener();
  
  const bridge = getBridge();
  const id = generateId();
  
  return new Promise((resolve, reject) => {
    // Set timeout (default 30 seconds)
    const timeout = setTimeout(() => {
      pendingRequests.delete(id);
      reject(new Error(`Command '${command}' timed out after 30 seconds`));
    }, 30000);
    
    // Store pending request
    pendingRequests.set(id, { resolve, reject, timeout });
    
    // Send command to C#
    try {
      const request = {
        id,
        command,
        args
      };
      
      bridge.Core.InvokeCommand(command, JSON.stringify(request));
    } catch (error) {
      // Clean up on immediate error
      clearTimeout(timeout);
      pendingRequests.delete(id);
      reject(error);
    }
  });
}