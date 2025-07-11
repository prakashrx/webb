<svelte:options customElement="webui-main-toolbar" />

<script>
  export let title = "WebUI Trading Platform";
  export let version = "1.0.0";
  export let showMenus = true;
  export let showClose = true;
  
  function handleMenuAction(action) {
    const event = new CustomEvent('webui-menu', {
      detail: { action, timestamp: new Date().toISOString() },
      bubbles: true,
      composed: true
    });
    document.dispatchEvent(event);
  }
  
  function handleClose() {
    const event = new CustomEvent('webui-close', {
      detail: { action: 'close', timestamp: new Date().toISOString() },
      bubbles: true,
      composed: true
    });
    document.dispatchEvent(event);
  }
</script>

<div class="webui-toolbar">
  <div class="toolbar-section left">
    <div class="logo">
      <div class="logo-icon">
        <svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor">
          <path d="M1 3h14v2H1V3zm0 4h14v2H1V7zm0 4h14v2H1v-2z"/>
        </svg>
      </div>
      <span class="logo-text">{title}</span>
      <div class="status-indicator active"></div>
    </div>
  </div>
  
  {#if showMenus}
  <div class="toolbar-section center">
    <div class="menu-items">
      <button class="menu-item" on:click={() => handleMenuAction('workspace')}>
        <span>Workspace</span>
      </button>
      <button class="menu-item" on:click={() => handleMenuAction('extensions')}>
        <span>Extensions</span>
      </button>
      <button class="menu-item" on:click={() => handleMenuAction('settings')}>
        <span>Settings</span>
      </button>
      <button class="menu-item" on:click={() => handleMenuAction('help')}>
        <span>Help</span>
      </button>
    </div>
  </div>
  {/if}
  
  <div class="toolbar-section right">
    <div class="status-info">
      <span class="version">v{version}</span>
    </div>
    
    {#if showClose}
    <button class="close-btn" on:click={handleClose} title="Close">
      <svg width="12" height="12" viewBox="0 0 12 12" fill="currentColor">
        <path d="M9.5 3.5L8.5 2.5 6 5 3.5 2.5 2.5 3.5 5 6 2.5 8.5 3.5 9.5 6 7 8.5 9.5 9.5 8.5 7 6z"/>
      </svg>
    </button>
    {/if}
  </div>
</div>

<style>
  .webui-toolbar {
    display: flex;
    align-items: center;
    height: 48px;
    background: linear-gradient(135deg, #2d3748 0%, #4a5568 100%);
    color: #ffffff;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', sans-serif;
    font-size: 13px;
    user-select: none;
    overflow: hidden;
    border-bottom: 1px solid #3e3e42;
  }
  
  .toolbar-section {
    display: flex;
    align-items: center;
    height: 100%;
  }
  
  .toolbar-section.left {
    padding-left: 16px;
    min-width: 200px;
  }
  
  .toolbar-section.center {
    flex: 1;
    justify-content: center;
  }
  
  .toolbar-section.right {
    padding-right: 16px;
    gap: 12px;
  }
  
  /* Logo Section */
  .logo {
    display: flex;
    align-items: center;
    gap: 8px;
    font-weight: 600;
    font-size: 14px;
  }
  
  .logo-icon {
    width: 24px;
    height: 24px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border-radius: 4px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #ffffff;
  }
  
  .logo-text {
    font-weight: 500;
  }
  
  .status-indicator {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    margin-left: 4px;
  }
  
  .status-indicator.active {
    background: #48bb78;
    box-shadow: 0 0 4px rgba(72, 187, 120, 0.5);
  }
  
  /* Menu Items */
  .menu-items {
    display: flex;
    gap: 4px;
  }
  
  .menu-item {
    padding: 6px 12px;
    border: none;
    border-radius: 4px;
    background: rgba(255, 255, 255, 0.1);
    color: #cccccc;
    font-size: 12px;
    font-family: inherit;
    cursor: pointer;
    transition: all 0.15s ease;
    border: 1px solid transparent;
  }
  
  .menu-item:hover {
    background: rgba(255, 255, 255, 0.2);
    border-color: rgba(255, 255, 255, 0.3);
    color: #ffffff;
  }
  
  .menu-item:active {
    background: rgba(255, 255, 255, 0.3);
    transform: translateY(1px);
  }
  
  /* Status Info */
  .status-info {
    display: flex;
    align-items: center;
    gap: 8px;
  }
  
  .version {
    font-size: 11px;
    color: #888888;
    font-family: 'Consolas', 'Monaco', monospace;
  }
  
  /* Close Button */
  .close-btn {
    width: 32px;
    height: 32px;
    border: none;
    border-radius: 4px;
    background: rgba(255, 255, 255, 0.1);
    color: #cccccc;
    cursor: pointer;
    transition: all 0.15s ease;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid transparent;
  }
  
  .close-btn:hover {
    background: rgba(255, 75, 75, 0.8);
    border-color: rgba(255, 75, 75, 1);
    color: #ffffff;
  }
  
  .close-btn:active {
    background: rgba(200, 50, 50, 0.9);
    transform: translateY(1px);
  }
  
  /* Focus states */
  .menu-item:focus-visible,
  .close-btn:focus-visible {
    outline: 2px solid #007acc;
    outline-offset: 2px;
  }
  
  /* Responsive adjustments */
  @media (max-width: 800px) {
    .toolbar-section.left {
      min-width: 150px;
    }
    
    .logo-text {
      display: none;
    }
    
    .menu-items {
      gap: 2px;
    }
    
    .menu-item {
      padding: 4px 8px;
      font-size: 11px;
    }
  }
</style> 