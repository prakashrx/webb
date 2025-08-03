import { readFileSync, writeFileSync } from 'fs';
import { resolve } from 'path';

// Get command line arguments
const [buildToolsPath, apiPath, outputPath, ...userPackages] = process.argv.slice(2);

if (!buildToolsPath || !apiPath || !outputPath) {
  console.error('Usage: node merge-package-json.js <build-tools-path> <api-path> <output-path> [user-packages...]');
  process.exit(1);
}

// Read the base package.json files
const buildTools = JSON.parse(readFileSync(resolve(buildToolsPath, 'package.json'), 'utf8'));
const api = JSON.parse(readFileSync(resolve(apiPath, 'package.json'), 'utf8'));

// Create merged package.json
const merged = {
  name: 'webui-project',
  version: '1.0.0',
  private: true,
  type: 'module',
  dependencies: {},
  devDependencies: {}
};

// Merge devDependencies from build tools
Object.assign(merged.devDependencies, buildTools.devDependencies || {});

// Merge devDependencies from API (for TypeScript compilation)
Object.assign(merged.devDependencies, api.devDependencies || {});

// Add user packages to dependencies
// Format: "package@version" or "package|version"
userPackages.forEach(pkg => {
  if (pkg) {
    const [name, version] = pkg.includes('@') ? pkg.split('@') : pkg.split('|');
    if (name && version) {
      merged.dependencies[name] = version;
    }
  }
});

// Write the merged package.json
writeFileSync(outputPath, JSON.stringify(merged, null, 2));
console.log(`✅ Merged package.json written to ${outputPath}`);
console.log(`📦 Dependencies: ${Object.keys(merged.dependencies).length}`);
console.log(`🔧 DevDependencies: ${Object.keys(merged.devDependencies).length}`);