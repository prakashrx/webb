<script>
  import { invoke } from '@webui/api';
  
  let message = 'Hello from Test Package!';
  
  async function testInvoke() {
    try {
      const result = await invoke('window.getTitle');
      message = `Window title: ${result}`;
    } catch (error) {
      message = `Error: ${error.message}`;
    }
  }
</script>

<main class="flex flex-col items-center justify-center min-h-screen p-8">
  <h1 class="text-4xl font-bold text-blue-600 mb-4">{message}</h1>
  <button 
    on:click={testInvoke}
    class="px-6 py-3 bg-blue-500 text-white rounded hover:bg-blue-600"
  >
    Get Window Title
  </button>
</main>