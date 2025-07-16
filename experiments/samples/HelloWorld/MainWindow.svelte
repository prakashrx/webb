<script>
  let count = 0;
  
  function increment() {
    count += 1;
  }
</script>

<main class="flex flex-col items-center justify-center min-h-screen p-8">
  <h1 class="text-6xl font-thin text-orange-500 mb-4">Hello WebUI! 🚀</h1>
  <p class="text-gray-600 mb-8">This is a Svelte component.</p>
  <button 
    on:click={increment}
    class="px-6 py-3 text-lg font-medium text-orange-500 bg-white border-2 border-orange-500 rounded hover:bg-orange-500 hover:text-white transition-colors duration-200"
  >
    Clicked {count} {count === 1 ? 'time' : 'times'}
  </button>
</main>