<script>
  import { invoke } from '@webui/api';
  
  // Component props
  export let title = 'WebUI Application';
  export let showControls = true;
  
  // Window control functions
  async function minimizeWindow() {
    await invoke('window.minimize');
  }
  
  async function maximizeWindow() {
    await invoke('window.maximize');
  }
  
  async function closeWindow() {
    await invoke('window.close');
  }
</script>

<!-- Modern frameless title bar inspired by VS Code / Discord -->
<div 
  class="titlebar"
  style="-webkit-app-region: drag;"
>
  <!-- Title -->
  <div class="title">
    {title}
  </div>
  
  <!-- Window Controls -->
  {#if showControls}
    <div class="window-controls" style="-webkit-app-region: no-drag;">
      <!-- Minimize -->
      <button 
        class="window-control minimize"
        on:click={minimizeWindow}
        aria-label="Minimize"
      >
        <svg width="10" height="1" viewBox="0 0 10 1">
          <rect width="10" height="1" fill="currentColor" />
        </svg>
      </button>
      
      <!-- Maximize/Restore -->
      <button 
        class="window-control maximize"
        on:click={maximizeWindow}
        aria-label="Maximize"
      >
        <svg width="10" height="10" viewBox="0 0 10 10">
          <rect x="0" y="0" width="10" height="10" fill="none" stroke="currentColor" stroke-width="1" />
        </svg>
      </button>
      
      <!-- Close -->
      <button 
        class="window-control close"
        on:click={closeWindow}
        aria-label="Close"
      >
        <svg width="10" height="10" viewBox="0 0 10 10">
          <path d="M0,0 L10,10 M10,0 L0,10" stroke="currentColor" stroke-width="1" fill="none" />
        </svg>
      </button>
    </div>
  {/if}
</div>

<style>
  .titlebar {
    position: relative;
    height: 30px;
    background: #2b2b2b;
    color: #cccccc;
    display: flex;
    align-items: center;
    justify-content: space-between;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
    font-size: 13px;
    -webkit-user-select: none;
    user-select: none;
  }
  
  .title {
    flex: 1;
    text-align: center;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    padding: 0 10px;
  }
  
  .window-controls {
    display: flex;
    height: 100%;
    flex-shrink: 0;
  }
  
  .window-control {
    width: 46px;
    height: 100%;
    border: none;
    background: transparent;
    color: #999;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: background-color 0.1s;
  }
  
  .window-control:hover {
    background: rgba(255, 255, 255, 0.1);
    color: #fff;
  }
  
  .window-control.close:hover {
    background: #e81123;
    color: #fff;
  }
  
  .window-control svg {
    pointer-events: none;
  }
  
  /* Light theme support */
  :global(.light-theme) .titlebar {
    background: #f3f3f3;
    color: #333;
  }
  
  :global(.light-theme) .window-control {
    color: #666;
  }
  
  :global(.light-theme) .window-control:hover {
    background: rgba(0, 0, 0, 0.1);
    color: #000;
  }
  
  :global(.light-theme) .window-control.close:hover {
    background: #e81123;
    color: #fff;
  }
</style>