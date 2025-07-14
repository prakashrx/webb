<script>
  import '../app.css';
  
  // Main toolbar component for WebUI Platform
  
  function handleNewWorkspace() {
    console.log('New Workspace clicked');
    // TODO: Implement workspace creation
  }
  
  function handleSettings() {
    console.log('Settings clicked');
    // Open settings panel via webui API
    webui.panel.open('settings');
  }
  
  function handleCommands() {
    console.log('Commands clicked');
    // TODO: Open command palette
  }
  
  function minimizeWindow() {
    webui.panel.minimize();
  }
  
  function maximizeWindow() {
    webui.panel.maximize();
  }
  
  function closeWindow() {
    webui.panel.close();
  }
</script>

<div class="flex justify-between items-center h-10 bg-gray-50 border-b border-gray-200 px-4 select-none font-sans" style="-webkit-app-region: drag;">
  <!-- Left section: Branding and navigation -->
  <div class="flex items-center gap-6">
    <div class="flex items-center gap-2">
      <div class="w-2 h-2 bg-green-500 rounded-full"></div>
      <span class="text-sm font-semibold text-gray-800">WebUI Platform</span>
    </div>
    
    <nav class="flex items-center gap-4" style="-webkit-app-region: no-drag;">
      <button class="text-gray-600 text-xs px-3 py-1.5 rounded hover:bg-gray-200 hover:text-gray-800 transition-all" on:click={handleNewWorkspace}>
        New Workspace
      </button>
      <button class="text-gray-600 text-xs px-3 py-1.5 rounded hover:bg-gray-200 hover:text-gray-800 transition-all" on:click={handleSettings}>
        Settings
      </button>
      <button class="text-gray-600 text-xs px-3 py-1.5 rounded hover:bg-gray-200 hover:text-gray-800 transition-all" on:click={handleCommands}>
        Commands
      </button>
    </nav>
  </div>
  
  <!-- Right section: Window controls -->
  <div class="flex items-center" style="-webkit-app-region: no-drag;">
    <div class="flex items-center">
      <button class="w-8 h-8 flex items-center justify-center text-gray-600 hover:bg-gray-200 transition-colors text-sm" on:click={minimizeWindow}>
        −
      </button>
      <button class="w-8 h-8 flex items-center justify-center text-gray-600 hover:bg-gray-200 transition-colors text-sm" on:click={maximizeWindow}>
        □
      </button>
      <button class="w-8 h-8 flex items-center justify-center text-gray-600 hover:bg-red-500 hover:text-white transition-colors text-sm" on:click={closeWindow}>
        ×
      </button>
    </div>
  </div>
</div>