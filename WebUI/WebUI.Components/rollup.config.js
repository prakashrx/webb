import svelte from 'rollup-plugin-svelte';
import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import css from 'rollup-plugin-css-only';
import livereload from 'rollup-plugin-livereload';
import serve from 'rollup-plugin-serve';
import terser from '@rollup/plugin-terser';
import fs from 'fs';
import path from 'path';

const production = !process.env.ROLLUP_WATCH;

// Get all .svelte files from src directory (including subdirectories)
function getSvelteFiles(dir) {
  const files = [];
  const items = fs.readdirSync(dir);
  
  for (const item of items) {
    const fullPath = path.join(dir, item);
    if (fs.statSync(fullPath).isDirectory()) {
      files.push(...getSvelteFiles(fullPath));
    } else if (item.endsWith('.svelte')) {
      files.push(fullPath);
    }
  }
  return files;
}

const componentFiles = getSvelteFiles('src');

const configs = componentFiles.map(file => {
  const relativePath = path.relative('src', file);
  const name = path.parse(relativePath).name;
  const outputPath = `public/${name}.js`;

  return {
    input: file,
    output: {
      file: outputPath,
      format: 'esm',
      sourcemap: !production,
    },
    plugins: [
      svelte({
        compilerOptions: {
          customElement: true,
          dev: !production
        }
      }),
      css({ output: `${name}.css` }),
      resolve({
        browser: true,
        dedupe: ['svelte']
      }),
      commonjs(),
      !production && serve({
        contentBase: 'public',
        port: 5000,
        open: true
      }),
      !production && livereload('public'),
      production && terser()
    ]
  };
});

export default configs; 