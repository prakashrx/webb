<script lang="ts">
  import { invoke } from '@webui/api';
  import { onMount } from 'svelte';
  import { GoldenLayout } from 'golden-layout';
  import Tab1 from './components/Tab1.svelte';
  import Tab2 from './components/Tab2.svelte';

  let container: HTMLDivElement;
  let layout: GoldenLayout;

  onMount(() => {
    layout = new GoldenLayout(container);

    // Register components using the new v2 API with Svelte components
    layout.registerComponentFactoryFunction('tab1', (container) => {
      new Tab1({
        target: container.element,
        props: { name: 'Tab 1' }
      });
    });

    layout.registerComponentFactoryFunction('tab2', (container) => {
      new Tab2({
        target: container.element,
        props: { name: 'Tab 2' }
      });
    });

    // Layout configuration with v2 property names - single stack for Chrome-like tabs
    const config = {
      root: {
        type: 'stack',
        content: [
          {
            type: 'component',
            componentType: 'tab1',
            title: 'Tab 1'
          },
          {
            type: 'component',
            componentType: 'tab2',
            title: 'Tab 2'
          }
        ]
      },
      settings: {
        showPopoutIcon: false,
        showMaximiseIcon: false,
        showCloseIcon: true
      }
    };

    // Load the layout using the new v2 method
    layout.loadLayout(config);

    // Handle resize with ResizeObserver
    const resizeObserver = new ResizeObserver(() => {
      layout.updateSize();
    });
    resizeObserver.observe(container);

    // Also handle window resize events with a small delay
    let resizeTimeout: number;
    const handleResize = () => {
      clearTimeout(resizeTimeout);
      resizeTimeout = setTimeout(() => {
        layout.updateSize();
      }, 100);
    };
    window.addEventListener('resize', handleResize);

    return () => {
      resizeObserver.disconnect();
      window.removeEventListener('resize', handleResize);
      layout.destroy();
    };
  });

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

<div class="flex flex-col h-screen bg-gray-900">
  <div class="window-header">
    <!-- Window controls -->
    <div class="window-controls">
      <button 
        on:click={minimizeWindow}
        class="window-control minimize"
      >
        <svg width="10" height="1" viewBox="0 0 10 1">
          <rect width="10" height="1" fill="currentColor"/>
        </svg>
      </button>
      <button 
        on:click={maximizeWindow}
        class="window-control maximize"
      >
        <svg width="10" height="10" viewBox="0 0 10 10">
          <rect width="9" height="9" x="0.5" y="0.5" fill="none" stroke="currentColor" stroke-width="1"/>
        </svg>
      </button>
      <button 
        on:click={closeWindow}
        class="window-control close"
      >
        <svg width="10" height="10" viewBox="0 0 10 10">
          <path d="M0,0 L10,10 M10,0 L0,10" stroke="currentColor" stroke-width="1.5"/>
        </svg>
      </button>
    </div>
  </div>
  
  <main class="flex-1 overflow-hidden">
    <div bind:this={container} class="container"></div>
  </main>
</div>

<style>
  @import 'golden-layout/dist/css/goldenlayout-base.css';
  @import 'golden-layout/dist/css/themes/goldenlayout-dark-theme.css';
  
  .container {
    width: 100%;
    height: 100%;
  }

  .window-header {
    position: absolute;
    top: 0;
    right: 0;
    z-index: 50;
    height: 30px;
  }

  .window-controls {
    display: flex;
    height: 100%;
    -webkit-app-region: no-drag;
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

  /* Make Golden Layout header draggable */
  :global(.lm_header) {
    -webkit-app-region: drag;
    height: 30px !important;
  }

  :global(.lm_tab) {
    -webkit-app-region: no-drag;
  }

  :global(.lm_controls) {
    -webkit-app-region: no-drag;
  }

  /* Adjust tab styling to match title bar */
  :global(.lm_tab) {
    height: 26px !important;
    margin-top: 2px !important;
    padding: 0 12px 5px !important;
    padding-right: 30px !important; /* More space for close button */
    min-width: 120px !important;
    border-radius: 8px 8px 0 0 !important; /* Rounded top corners */
  }

  /* Fix close button visibility and positioning */
  :global(.lm_close_tab) {
    background-image: none !important;
    width: 18px !important;
    height: 18px !important;
    opacity: 0.6 !important;
    transition: opacity 0.2s;
    position: absolute !important;
    top: 4px !important; /* Properly position at top */
    right: 8px !important;
    display: flex !important;
    align-items: center !important;
    justify-content: center !important;
    border-radius: 3px;
  }

  :global(.lm_close_tab:hover) {
    opacity: 1 !important;
    background: rgba(255, 255, 255, 0.1) !important;
  }

  :global(.lm_close_tab::before) {
    content: '×';
    font-size: 18px;
    line-height: 1;
    color: #fff;
    font-weight: 300;
  }

  /* Better tab title positioning */
  :global(.lm_tab .lm_title) {
    padding-top: 4px !important;
  }

  /* Active tab styling */
  :global(.lm_tab.lm_active) {
    background: #1a1a1a !important;
  }

  /* Ensure Golden Layout fills the container */
  :global(.lm_goldenlayout) {
    width: 100% !important;
    height: 100% !important;
    position: absolute !important;
    left: 0 !important;
    top: 0 !important;
  }

  :global(.lm_root) {
    width: 100% !important;
    height: 100% !important;
  }

  /* Fix stack and items width */
  :global(.lm_stack) {
    width: 100% !important;
  }

  :global(.lm_stack > .lm_items) {
    width: 100% !important;
    left: 0 !important;
    right: 0 !important;
  }

  /* Ensure the tab container takes full width */
  :global(.lm_header .lm_tabs) {
    width: calc(100% - 150px) !important; /* Leave space for window controls */
  }

  /* Ensure content takes full width */
  :global(.lm_content) {
    width: 100% !important;
  }

  /* Make sure the container has position relative for absolute children */
  .container {
    position: relative !important;
  }
</style>