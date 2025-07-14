<script>
  import '../app.css';
  
  // Settings panel - stubbed for now
  
  function closeSettings() {
    webui.panel.close('settings');
  }
</script>

<div class="w-full h-full bg-white">
  <!-- Settings header -->
  <div class="flex justify-between items-center px-6 py-4 border-b border-gray-200">
    <h1 class="text-lg font-semibold text-gray-800">Settings</h1>
    <button class="text-gray-400 hover:text-gray-600 text-xl" on:click={closeSettings}>
      ×
    </button>
  </div>
  
  <!-- Settings content -->
  <div class="flex h-full">
    <!-- Sidebar -->
    <div class="w-48 bg-gray-50 border-r border-gray-200 p-4">
      <nav class="space-y-2">
        <button class="w-full text-left px-3 py-2 text-sm text-gray-700 bg-blue-50 border-l-2 border-blue-500 rounded-r">
          General
        </button>
        <button class="w-full text-left px-3 py-2 text-sm text-gray-600 hover:bg-gray-100 rounded">
          Appearance
        </button>
        <button class="w-full text-left px-3 py-2 text-sm text-gray-600 hover:bg-gray-100 rounded">
          About
        </button>
      </nav>
    </div>
    
    <!-- Main content -->
    <div class="flex-1 p-6">
      <div class="max-w-2xl">
        <h2 class="text-base font-medium text-gray-800 mb-4">General Settings</h2>
        
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Language</label>
            <select class="w-48 px-3 py-2 border border-gray-300 rounded-md text-sm">
              <option>English</option>
            </select>
          </div>
          
          <div class="flex items-center justify-between">
            <div>
              <label class="block text-sm font-medium text-gray-700">Enable notifications</label>
            </div>
            <div class="relative">
              <input type="checkbox" checked class="sr-only">
              <div class="w-10 h-6 bg-blue-500 rounded-full shadow-inner"></div>
              <div class="absolute w-4 h-4 bg-white rounded-full shadow top-1 right-1 transition"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</div>