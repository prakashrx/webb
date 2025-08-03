<script lang="ts">
  import { tweened } from 'svelte/motion';
  import { cubicOut } from 'svelte/easing';
  import { scale } from 'svelte/transition';
  
  let count: number = 0;
  const displayCount = tweened(0, {
    duration: 600,
    easing: cubicOut
  });
  
  function increment(): void {
    count += 1;
    displayCount.set(count);
  }
  
  function decrement(): void {
    count -= 1;
    displayCount.set(count);
  }
  
  // For key block animation
  $: countKey = Math.round($displayCount);
</script>

<div class="bg-white rounded-lg shadow-md p-6 text-center">
  <h3 class="text-sm font-medium text-gray-500 uppercase tracking-wider mb-2">Component Example</h3>
  <div class="text-3xl font-semibold text-gray-800 mb-4 relative h-10 flex items-center justify-center">
    {#key countKey}
      <span 
        in:scale={{ duration: 400, start: 0.5 }}
        class="absolute animate-number-change"
      >
        {countKey}
      </span>
    {/key}
  </div>
  <div class="flex justify-center space-x-2">
    <button 
      on:click={decrement} 
      class="w-10 h-10 rounded-full bg-gray-200 hover:bg-gray-300 text-gray-700 font-medium transition-colors duration-200"
    >
      −
    </button>
    <button 
      on:click={increment} 
      class="w-10 h-10 rounded-full bg-gray-200 hover:bg-gray-300 text-gray-700 font-medium transition-colors duration-200"
    >
      +
    </button>
  </div>
</div>

<style>
  @keyframes numberChange {
    0% {
      transform: scale(0.5);
      opacity: 0.5;
      filter: blur(2px);
    }
    50% {
      transform: scale(1.2);
      text-shadow: 0 0 20px rgba(59, 130, 246, 0.8);
      color: rgb(59, 130, 246);
    }
    100% {
      transform: scale(1);
      opacity: 1;
      filter: blur(0);
      text-shadow: none;
    }
  }
  
  .animate-number-change {
    animation: numberChange 0.4s ease-out;
  }
</style>