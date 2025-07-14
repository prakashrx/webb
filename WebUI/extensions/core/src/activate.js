// WebUI Core Extension - Entry Point
// This extension provides the foundational UI components for the WebUI Platform

import MainToolbar from './panels/MainToolbar.svelte';
import Settings from './panels/Settings.svelte';

export function activate(context) {
  console.log('WebUI Core extension activated');
  
  // Check if WebUI API is available
  if (typeof window !== 'undefined' && window.webui && window.webui.panel) {
    // Register core panels using registerPanel for Svelte components
    window.webui.panel.registerPanel('main-toolbar', MainToolbar);
    window.webui.panel.registerPanel('settings', Settings);
    
    console.log('Core extension panels registered successfully');
  } else {
    console.error('WebUI API not available - cannot register core extension panels');
  }
  
  // The main toolbar will be opened by the workbench on startup
  // Other panels are opened on-demand via user interaction
}