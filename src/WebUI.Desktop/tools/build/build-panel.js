import { rollup } from 'rollup';
import svelte from 'rollup-plugin-svelte';
import sveltePreprocess from 'svelte-preprocess';
import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import terser from '@rollup/plugin-terser';
import postcss from 'rollup-plugin-postcss';
import alias from '@rollup/plugin-alias';
import typescript from '@rollup/plugin-typescript';
import tailwindcss from 'tailwindcss';
import autoprefixer from 'autoprefixer';
import path from 'path';
import { fileURLToPath } from 'url';
import { existsSync } from 'fs';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

// Get command line arguments
const args = process.argv.slice(2);
const inputFile = args[0];
const outputDir = args[1];
const isDevelopment = args[2] === 'true';
const msbuildProjectDir = args[3]; // MSBuild project directory
const webuiApiPath = args[4]; // WebUI API path
const webuiComponentsPath = args[5]; // WebUI Components path
const webuiBuildToolsPath = args[6]; // WebUI build tools path

if (!inputFile || !outputDir || !msbuildProjectDir || !webuiApiPath || !webuiComponentsPath || !webuiBuildToolsPath) {
  console.error('Usage: node build-panel.js <input-file> <output-dir> <is-development> <project-dir> <api-path> <components-path> <build-tools-path>');
  process.exit(1);
}

// Path to WebUI API source
const WEBUI_API_PATH = path.join(webuiApiPath, 'index.ts');

// Path to WebUI Components
const WEBUI_COMPONENTS_PATH = path.join(webuiComponentsPath, 'index.ts');

// Get the Svelte file's directory for local imports
const inputFileDir = path.dirname(inputFile);

// Use MSBuild project directory
const projectDir = msbuildProjectDir;

async function build() {
  try {
    const inputName = path.basename(inputFile, '.svelte');
    
    console.log(`Building ${inputName} from ${inputFile} to ${outputDir}`);
    
    // Create a wrapper that imports base CSS and the Svelte component
    const wrapperContent = `
import '${path.join(webuiBuildToolsPath, 'base.css').replace(/\\/g, '/')}';
import Component from '${inputFile.replace(/\\/g, '/')}';
export default Component;
`;
    
    const tempWrapper = path.join(__dirname, `${inputName}-wrapper.js`);
    await import('fs').then(fs => fs.promises.writeFile(tempWrapper, wrapperContent));
    
    // Create rollup config
    const bundle = await rollup({
      input: tempWrapper,
      plugins: [
        // We need esbuild to handle TypeScript files since @rollup/plugin-typescript has issues with tsconfig: false
        {
          name: 'typescript-handler',
          async transform(code, id) {
            if (id.endsWith('.ts') && !id.endsWith('.d.ts')) {
              const esbuild = await import('esbuild');
              const result = await esbuild.transform(code, {
                loader: 'ts',
                target: 'es2020',
                format: 'esm',
                sourcemap: isDevelopment
              });
              return {
                code: result.code,
                map: result.map
              };
            }
            return null;
          }
        },
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
            typescript: {
              tsconfigFile: false,
              compilerOptions: {
                target: "ES2020",
                module: "ESNext",
                moduleResolution: "node",
                allowSyntheticDefaultImports: true,
                verbatimModuleSyntax: true
              }
            },
            postcss: {
              plugins: [
                tailwindcss({
                  content: [
                    inputFile,
                    path.join(projectDir, '**/*.svelte'),
                    path.join(projectDir, '**/*.js'),
                    path.join(projectDir, '**/*.ts')
                  ]
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
          config: false, // Don't load external config
          plugins: [
            tailwindcss({
              content: [
                inputFile,
                path.join(projectDir, '**/*.svelte'),
                path.join(projectDir, '**/*.js'),
                path.join(projectDir, '**/*.ts')
              ]
            }),
            autoprefixer()
          ]
        }),
        resolve({
          extensions: ['.mjs', '.js', '.ts', '.json', '.svelte'],
          browser: true,
          dedupe: ['svelte'],
          preferBuiltins: false,
          // Look for modules in project's obj/webui/node_modules
          moduleDirectories: [
            path.join(msbuildProjectDir, 'obj', 'webui', 'node_modules'),
            'node_modules'
          ]
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