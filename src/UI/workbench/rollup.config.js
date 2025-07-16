import svelte from 'rollup-plugin-svelte';
import sveltePreprocess from 'svelte-preprocess';
import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import terser from '@rollup/plugin-terser';
import css from 'rollup-plugin-css-only';
import postcss from 'rollup-plugin-postcss';
import fs from 'fs';
import path from 'path';

// Collect all .svelte files in ./src directory
const srcDir = './src';
const inputFiles = fs.readdirSync(srcDir)
  .filter(f => f.endsWith('.svelte'))
  .reduce((entries, file) => {
    const name = path.parse(file).name.toLowerCase();
    entries[name] = path.resolve(srcDir, file);
    return entries;
  }, {});

console.log('Building components:', Object.keys(inputFiles));

export default {
  input: inputFiles,
  output: {
    dir: 'dist',
    format: 'es',
    entryFileNames: '[name].js',
    sourcemap: false
  },
  plugins: [
    postcss({
      extract: 'style.css',
      minimize: true
    }),
    svelte({
      preprocess: sveltePreprocess({
        postcss: true
      }),
      emitCss: true, // Emit CSS for external processing
      compilerOptions: {
        dev: false
      }
    }),
    resolve({
      browser: true,
      dedupe: ['svelte']
    }),
    commonjs(),
    terser()
  ]
};