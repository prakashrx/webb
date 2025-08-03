<script>
  import './tailwind.css';
  
  // Workspace panel - shows trading workspace
  
  function closeWorkspace() {
    webui.panel.close();
  }
</script>

<div class="w-full h-full bg-gray-900 text-gray-100 flex flex-col">
  <!-- Workspace header -->
  <div class="flex justify-between items-center px-4 py-2 bg-gray-800 border-b border-gray-700">
    <h1 class="text-sm font-medium">Trading Workspace</h1>
    <button class="text-gray-400 hover:text-gray-200 text-lg" on:click={closeWorkspace}>
      ×
    </button>
  </div>
  
  <!-- Workspace content -->
  <div class="flex-1 flex">
    <!-- Left sidebar -->
    <div class="w-48 bg-gray-800 border-r border-gray-700 p-3">
      <h2 class="text-xs font-semibold text-gray-400 uppercase mb-3">Watchlists</h2>
      <div class="space-y-1">
        <div class="px-2 py-1.5 bg-gray-700 rounded text-sm cursor-pointer hover:bg-gray-600">
          Tech Stocks
        </div>
        <div class="px-2 py-1.5 rounded text-sm cursor-pointer hover:bg-gray-700">
          Crypto
        </div>
        <div class="px-2 py-1.5 rounded text-sm cursor-pointer hover:bg-gray-700">
          Favorites
        </div>
      </div>
    </div>
    
    <!-- Main area -->
    <div class="flex-1 flex flex-col">
      <!-- Chart area -->
      <div class="flex-1 bg-gray-850 m-4 rounded border border-gray-700 flex items-center justify-center">
        <div class="text-center">
          <div class="text-gray-500 mb-2">
            <svg class="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
          </div>
          <p class="text-gray-400 text-sm">Chart Component</p>
          <p class="text-gray-500 text-xs mt-1">Trading charts will appear here</p>
        </div>
      </div>
      
      <!-- Bottom panel -->
      <div class="h-48 bg-gray-800 border-t border-gray-700 p-4">
        <div class="flex gap-4 text-xs font-medium text-gray-400 mb-3">
          <button class="hover:text-gray-200">POSITIONS</button>
          <button class="hover:text-gray-200">ORDERS</button>
          <button class="hover:text-gray-200">HISTORY</button>
        </div>
        <div class="text-center text-gray-500 text-sm mt-8">
          No positions open
        </div>
      </div>
    </div>
    
    <!-- Right sidebar -->
    <div class="w-64 bg-gray-800 border-l border-gray-700 p-3">
      <h2 class="text-xs font-semibold text-gray-400 uppercase mb-3">Order Entry</h2>
      <div class="space-y-3">
        <div>
          <label class="block text-xs text-gray-400 mb-1">Symbol</label>
          <input type="text" class="w-full px-2 py-1 bg-gray-700 border border-gray-600 rounded text-sm" placeholder="AAPL">
        </div>
        <div>
          <label class="block text-xs text-gray-400 mb-1">Quantity</label>
          <input type="number" class="w-full px-2 py-1 bg-gray-700 border border-gray-600 rounded text-sm" placeholder="100">
        </div>
        <div class="flex gap-2">
          <button class="flex-1 py-1.5 bg-green-600 hover:bg-green-700 rounded text-sm font-medium">BUY</button>
          <button class="flex-1 py-1.5 bg-red-600 hover:bg-red-700 rounded text-sm font-medium">SELL</button>
        </div>
      </div>
    </div>
  </div>
</div>

<style>
  .bg-gray-850 {
    background-color: #1a1a1a;
  }
</style>