import { defineConfig } from 'vite';
import { svelte } from '@sveltejs/vite-plugin-svelte';

export default defineConfig({
  plugins: [svelte()],
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    lib: {
      entry: 'src/activate.js',
      name: 'CoreExtension',
      fileName: 'activate',
      formats: ['es']
    },
    rollupOptions: {
      external: [], // Don't externalize anything, bundle everything
      output: {
        format: 'es'
      }
    }
  },
  server: {
    port: 3002,
    strictPort: true
  }
});