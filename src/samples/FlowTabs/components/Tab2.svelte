<script lang="ts">
  export let name = 'Tab 2';
  
  let count = 0;
  
  function increment() {
    count++;
  }
</script>

<div class="p-5 text-gray-200 h-full">
  <h2 class="text-2xl font-semibold mb-3">{name}</h2>
  <p class="text-gray-400">This is the content of {name}</p>
  
  <div class="mt-8 p-6 bg-gray-800 rounded-lg">
    <h3 class="text-lg font-medium mb-4">Counter Demo</h3>
    <p class="text-3xl font-bold text-blue-400 mb-4">{count}</p>
    <button 
      on:click={increment}
      class="bg-green-500 hover:bg-green-600 text-white px-6 py-2 rounded transition-colors"
    >
      Increment
    </button>
  </div>
</div>