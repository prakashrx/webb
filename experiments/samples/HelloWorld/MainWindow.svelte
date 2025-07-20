<script>
  import { invoke } from '@webui/api';
  
  let count = 0;
  let message = '';
  let echoResult = '';
  let windowTitle = 'HelloWorld';
  
  function increment() {
    count += 1;
  }
  
  async function testEcho() {
    try {
      const result = await invoke('test.echo', { message: `Hello from WebUI! Count: ${count}` });
      echoResult = result;
      message = `Received: ${result}`;
    } catch (error) {
      message = `Error: ${error.message}`;
    }
  }
  
  async function getTime() {
    try {
      const time = await invoke('test.getTime');
      message = `Current time: ${time}`;
    } catch (error) {
      message = `Error: ${error.message}`;
    }
  }

  async function addNumbers() {
    try {
      const result = await invoke('test.addNumbers', { a: count, b: 10 });
      message = `Result: ${result}`;
    } catch (error) {
      message = `Error: ${error.message}`;
    }
  }
  
  // Window management functions
  async function minimizeWindow() {
    try {
      await invoke('window.minimize');
      message = 'Window minimized';
    } catch (error) {
      message = `Error: ${error.message}`;
    }
  }
  
  async function maximizeWindow() {
    try {
      await invoke('window.maximize');
      message = 'Window maximized';
    } catch (error) {
      message = `Error: ${error.message}`;
    }
  }
  
  async function updateTitle() {
    try {
      await invoke('window.setTitle', { title: windowTitle });
      message = `Title updated to: ${windowTitle}`;
    } catch (error) {
      message = `Error: ${error.message}`;
    }
  }
  
  async function getWindowState() {
    try {
      const state = await invoke('window.getState');
      message = `Window state: ${JSON.stringify(state)}`;
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

    <button 
      on:click={addNumbers}
      class="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
    >
      Add Numbers
    </button>
  </div>
  
  <!-- Window Controls Section -->
  <div class="mt-8 p-4 bg-gray-50 rounded-lg">
    <h3 class="text-lg font-semibold mb-4">Window Controls</h3>
    
    <div class="space-y-4">
      <div class="flex space-x-2">
        <button 
          on:click={minimizeWindow}
          class="px-3 py-1 bg-yellow-500 text-white rounded hover:bg-yellow-600"
        >
          Minimize
        </button>
        
        <button 
          on:click={maximizeWindow}
          class="px-3 py-1 bg-green-500 text-white rounded hover:bg-green-600"
        >
          Maximize
        </button>
        
        <button 
          on:click={getWindowState}
          class="px-3 py-1 bg-purple-500 text-white rounded hover:bg-purple-600"
        >
          Get State
        </button>
      </div>
      
      <div class="flex items-center space-x-2">
        <input 
          type="text" 
          bind:value={windowTitle}
          placeholder="Window title"
          class="px-3 py-2 border rounded flex-1"
        />
        <button 
          on:click={updateTitle}
          class="px-4 py-2 bg-indigo-500 text-white rounded hover:bg-indigo-600"
        >
          Set Title
        </button>
      </div>
    </div>
  </div>
  
  {#if message}
    <div class="mt-4 p-4 bg-gray-100 rounded">
      {message}
    </div>
  {/if}
</main>