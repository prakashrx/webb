<script>
  import { invoke } from '@webui/api';
  
  let count = 0;
  let message = '';
  let echoResult = '';
  
  function increment() {
    count += 1;
  }
  
  async function testEcho() {
    try {
      const result = await invoke('echo', { message: `Hello from WebUI! Count: ${count}` });
      echoResult = result;
      message = `Received: ${result}`;
    } catch (error) {
      message = `Error: ${error.message}`;
    }
  }
  
  async function getTime() {
    try {
      const time = await invoke('get-time');
      message = `Current time: ${time}`;
    } catch (error) {
      message = `Error: ${error.message}`;
    }
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
  
  <div class="mt-8 space-x-4">
    <button 
      on:click={testEcho}
      class="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
    >
      Test Echo
    </button>
    
    <button 
      on:click={getTime}
      class="px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600"
    >
      Get Time
    </button>
  </div>
  
  {#if message}
    <div class="mt-4 p-4 bg-gray-100 rounded">
      {message}
    </div>
  {/if}
</main>