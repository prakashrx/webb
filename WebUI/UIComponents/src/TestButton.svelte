<svelte:options customElement="webui-test-button" />

<script>
  import { createEventDispatcher } from 'svelte';
  
  export let text = "Click me";
  export let variant = "primary"; // primary, secondary, danger
  export let size = "medium"; // small, medium, large
  export let disabled = false;
  
  let elementRef;
  
  function handleClick() {
    if (!disabled) {
      // Dispatch custom event that bubbles up to the document
      const event = new CustomEvent('webui-click', {
        detail: { text, variant, timestamp: new Date().toISOString() },
        bubbles: true,
        composed: true
      });
      
      // Dispatch on the custom element itself
      if (elementRef) {
        elementRef.dispatchEvent(event);
      }
    }
  }
</script>

<button 
  bind:this={elementRef}
  class="webui-button {variant} {size}" 
  class:disabled
  on:click={handleClick}
  {disabled}
>
  {text}
</button>

<style>
  .webui-button {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', sans-serif;
    font-size: 13px;
    font-weight: 400;
    line-height: 1.2;
    border: 1px solid transparent;
    border-radius: 2px;
    padding: 4px 14px;
    cursor: pointer;
    transition: all 0.15s ease;
    outline: none;
    user-select: none;
    white-space: nowrap;
  }
  
  /* VS Code inspired color scheme */
  .webui-button.primary {
    background-color: #0e639c;
    color: #ffffff;
    border-color: #0e639c;
  }
  
  .webui-button.primary:hover:not(.disabled) {
    background-color: #1177bb;
    border-color: #1177bb;
  }
  
  .webui-button.primary:active:not(.disabled) {
    background-color: #0a4d7a;
    border-color: #0a4d7a;
  }
  
  .webui-button.secondary {
    background-color: #5a5d5e;
    color: #cccccc;
    border-color: #5a5d5e;
  }
  
  .webui-button.secondary:hover:not(.disabled) {
    background-color: #6c7079;
    border-color: #6c7079;
  }
  
  .webui-button.danger {
    background-color: #a1260d;
    color: #ffffff;
    border-color: #a1260d;
  }
  
  .webui-button.danger:hover:not(.disabled) {
    background-color: #c5341a;
    border-color: #c5341a;
  }
  
  /* Size variants */
  .webui-button.small {
    font-size: 11px;
    padding: 2px 8px;
  }
  
  .webui-button.large {
    font-size: 14px;
    padding: 6px 16px;
  }
  
  /* Disabled state */
  .webui-button.disabled {
    opacity: 0.4;
    cursor: not-allowed;
  }
  
  /* Focus outline */
  .webui-button:focus-visible {
    outline: 1px solid #007acc;
    outline-offset: 2px;
  }
</style> 