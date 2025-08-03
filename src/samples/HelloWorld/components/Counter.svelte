<script lang="ts">
  let count: number = 0;
  
  function increment(): void {
    count += 1;
  }
  
  function decrement(): void {
    count -= 1;
  }
</script>

<div class="bg-white rounded-lg shadow-md p-6 text-center">
  <h3 class="text-sm font-medium text-gray-500 uppercase tracking-wider mb-2">Component Example</h3>
  <div class="text-3xl font-semibold text-gray-800 mb-4">{count}</div>
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