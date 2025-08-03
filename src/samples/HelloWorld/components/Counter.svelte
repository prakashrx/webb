<script>
  export let count = 0;
  export let label = "Counter";
  
  function increment() {
    count += 1;
  }
  
  function decrement() {
    count -= 1;
  }
  
  function reset() {
    count = 0;
  }
</script>

<div class="bg-white rounded-lg shadow-md p-6 text-center">
  <h3 class="text-xl font-semibold text-gray-700 mb-4">{label}</h3>
  <div class="text-4xl font-bold text-indigo-600 mb-6">{count}</div>
  <div class="flex justify-center space-x-4">
    <button 
      on:click={decrement} 
      class="px-4 py-2 rounded font-semibold bg-red-500 text-white hover:bg-red-600 transition-colors duration-200"
    >
      -
    </button>
    <button 
      on:click={reset} 
      class="px-4 py-2 rounded font-semibold bg-gray-500 text-white hover:bg-gray-600 transition-colors duration-200"
    >
      Reset
    </button>
    <button 
      on:click={increment} 
      class="px-4 py-2 rounded font-semibold bg-green-500 text-white hover:bg-green-600 transition-colors duration-200"
    >
      +
    </button>
  </div>
</div>