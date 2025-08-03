<script>
  import { invoke } from '@webui/api';
  import { TitleBar } from '@webui/components';
  import Counter from './components/Counter.svelte';
  
  let greeting = '';
  
  async function sayHello() {
    try {
      const result = await invoke('test.getGreeting', { name: 'WebUI' });
      greeting = result;
    } catch (error) {
      greeting = `Error: ${error.message}`;
    }
  }
</script>

<div class="flex flex-col h-screen bg-gray-50">
  <!-- Built-in TitleBar component -->
  <TitleBar title="Hello World" />
  
  <main class="flex-1 flex flex-col items-center justify-center p-8">
    <div class="text-center mb-12">
      <h1 class="text-5xl font-light text-gray-800 mb-4">Hello, WebUI!</h1>
      <p class="text-xl text-gray-600">A modern desktop framework for .NET</p>
    </div>
    
    <!-- Simple demo button -->
    <button 
      on:click={sayHello}
      class="px-8 py-3 text-lg bg-blue-500 text-white rounded-lg hover:bg-blue-600 transform hover:scale-105 transition-all duration-200 shadow-lg"
    >
      Say Hello
    </button>
    
    {#if greeting}
      <div class="mt-6 p-4 bg-white rounded-lg shadow text-gray-700">
        {greeting}
      </div>
    {/if}
    
    <!-- Local Component Example -->
    <div class="mt-12">
      <Counter />
    </div>
  </main>
</div>