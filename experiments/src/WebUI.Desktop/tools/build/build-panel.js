import { rollup } from 'rollup';
import svelte from 'rollup-plugin-svelte';
import sveltePreprocess from 'svelte-preprocess';
import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import terser from '@rollup/plugin-terser';
import postcss from 'rollup-plugin-postcss';
import alias from '@rollup/plugin-alias';
import tailwindcss from 'tailwindcss';
import autoprefixer from 'autoprefixer';
import path from 'path';
import { fileURLToPath } from 'url';
import { existsSync } from 'fs';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

// Path to WebUI API - now always in the same relative location
const WEBUI_API_PATH = path.join(__dirname, '..', '..', 'Api', 'dist', 'webui-api.bundle.js');

// Path to WebUI Components
const WEBUI_COMPONENTS_PATH = path.join(__dirname, '..', '..', 'Components', 'src', 'index.js');

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
    
    // Create a wrapper that imports base CSS and the Svelte component
    const wrapperContent = `
import '${path.join(__dirname, 'base.css').replace(/\\/g, '/')}';
import Component from '${inputFile.replace(/\\/g, '/')}';
export default Component;
`;
    
    const tempWrapper = path.join(__dirname, `${inputName}-wrapper.js`);
    await import('fs').then(fs => fs.promises.writeFile(tempWrapper, wrapperContent));
    
    // Create rollup config
    const bundle = await rollup({
      input: tempWrapper,
      plugins: [
        alias({
          entries: [
            { 
              find: '@webui/api', 
              replacement: WEBUI_API_PATH
            },
            { 
              find: '@webui/components', 
              replacement: WEBUI_COMPONENTS_PATH
            }
          ]
        }),
        svelte({
          preprocess: sveltePreprocess({
            postcss: {
              plugins: [
                tailwindcss({
                  content: [inputFile]
                }),
                autoprefixer()
              ]
            }
          }),
          compilerOptions: {
            dev: isDevelopment
          },
          emitCss: true
        }),
        postcss({
          extract: false, // Inline CSS in JS
          minimize: !isDevelopment,
          plugins: [
            tailwindcss({
              content: [inputFile]
            }),
            autoprefixer()
          ]
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
    
    // Clean up temp wrapper file
    await import('fs').then(fs => fs.promises.unlink(tempWrapper));
    
    console.log(`✅ Successfully built ${inputName}`);
  } catch (error) {
    console.error('Build error:', error);
    process.exit(1);
  }
}

build();