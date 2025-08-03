<script lang="ts">
  import { invoke } from '@webui/api';
  import { TitleBar } from '@webui/components';
  import Counter from './components/Counter.svelte';
  import { fade } from 'svelte/transition';
  import { onMount } from 'svelte';
  
  let greeting: string = '';
  let visible = false;
  
  onMount(() => {
    visible = true;
  });
  
  async function sayHello(): Promise<void> {
    greeting = ''; // Clear to re-trigger animation
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
    {#if visible}
      <div class="text-center mb-12" in:fade={{ duration: 800, delay: 200 }}>
        <h1 class="text-5xl font-light text-gray-800 mb-4">Hello, WebUI!</h1>
        <p class="text-xl text-gray-600">Modern web stack for desktop: Svelte + Tailwind + TypeScript</p>
      </div>
      
      <!-- Simple demo button -->
      <button 
        on:click={sayHello}
        class="px-8 py-3 text-lg bg-blue-500 text-white rounded-lg hover:bg-blue-600 transform hover:scale-105 active:scale-95 transition-all duration-200 shadow-lg"
        in:fade={{ duration: 800, delay: 600 }}
      >
        Say Hello
      </button>
      
      {#if greeting}
        <div 
          class="mt-6 p-4 bg-white rounded-lg shadow text-gray-700"
          in:fade={{ duration: 400 }}
          out:fade={{ duration: 200 }}
        >
          {greeting}
        </div>
      {/if}
      
      <!-- Local Component Example -->
      <div class="mt-12" in:fade={{ duration: 800, delay: 1000 }}>
        <Counter />
      </div>
    {/if}
  </main>
</div>