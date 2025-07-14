import typescript from '@rollup/plugin-typescript';
import resolve from '@rollup/plugin-node-resolve';
import terser from '@rollup/plugin-terser';

const isProduction = process.env.NODE_ENV === 'production';

export default {
  input: 'src/index.ts',
  output: {
    file: 'dist/webui-api.js',
    format: 'iife',
    name: 'WebUIApi',
    sourcemap: true,
    banner: '// WebUI Platform API v1.0.0\n// Auto-generated - do not edit directly'
  },
  plugins: [
    resolve(),
    typescript({
      tsconfig: './tsconfig.json',
      sourceMap: true,
      inlineSources: true
    }),
    ...(isProduction ? [terser()] : [])
  ]
};