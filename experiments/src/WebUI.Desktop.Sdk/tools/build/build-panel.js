import { rollup } from 'rollup';
import svelte from 'rollup-plugin-svelte';
import sveltePreprocess from 'svelte-preprocess';
import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import terser from '@rollup/plugin-terser';
import postcss from 'rollup-plugin-postcss';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

// Get command line arguments
const args = process.argv.slice(2);
const inputFile = args[0];
const outputDir = args[1];
const isDevelopment = args[2] === 'true';

if (!inputFile || !outputDir) {
  console.error('Usage: node build-panel.js <input-file> <output-dir> [is-development]');
  process.exit(1);
}

async function build() {
  try {
    const inputName = path.basename(inputFile, '.svelte');
    
    console.log(`Building ${inputName} from ${inputFile} to ${outputDir}`);
    
    // Create rollup config
    const bundle = await rollup({
      input: inputFile,
      plugins: [
        svelte({
          preprocess: sveltePreprocess({
            postcss: true
          }),
          compilerOptions: {
            dev: isDevelopment
          }
        }),
        postcss({
          extract: false, // Inline CSS in JS
          minimize: !isDevelopment
        }),
        resolve({
          browser: true,
          dedupe: ['svelte'],
          preferBuiltins: false
        }),
        commonjs(),
        !isDevelopment && terser()
      ].filter(Boolean)
    });

    // Write the bundle
    await bundle.write({
      dir: outputDir,
      format: 'es',
      entryFileNames: `${inputName}.js`,
      sourcemap: isDevelopment
    });

    await bundle.close();
    
    console.log(`✅ Successfully built ${inputName}`);
  } catch (error) {
    console.error('Build error:', error);
    process.exit(1);
  }
}

build();