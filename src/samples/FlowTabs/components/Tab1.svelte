<script lang="ts">
  export let name = 'Tab 1';
</script>

<div class="p-5 text-gray-200 h-full">
  <h2 class="text-2xl font-semibold mb-3">{name}</h2>
  <p class="text-gray-400">This is the content of {name}</p>
  <div class="mt-5">
    <button class="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded transition-colors">
      Click me
    </button>
    <p class="mt-3 text-gray-400">Some interactive content here</p>
  </div>
</div>