Here you go — a clean, no-fluff article explaining how to build a **Svelte component library** (not an app!) that outputs **individually loadable Web Components**, perfect for embedding into any host like a C# WebView.

# 🔧 Building a Modular Svelte Component Library with Rollup

> A minimal setup to write reusable Svelte components, compile each to its own JS file, and load them dynamically in any web environment — including C# WebView apps.

---

## 🎯 Why This Setup?

* You want **Svelte components**, not a full app.
* You want each component to be **independently compiled**.
* You want to **load them dynamically** in environments like WebView2, without needing a full SPA or Svelte runtime.
* You want a **lightweight dev server with hot reload**, but no bloat like SvelteKit.

---

## 📁 Project Structure

```
svelte-components/
├── public/              # Static test page, bundled output lives here
│   └── index.html
├── src/                 # Your Svelte components
│   ├── HelloWorld.svelte
│   └── MyButton.svelte
├── rollup.config.js
├── package.json
└── README.md
```

---

## ⚙️ Step 1: Install Dependencies

```bash
npm init -y
npm install --save-dev \
  svelte \
  rollup \
  rollup-plugin-svelte \
  rollup-plugin-css-only \
  @rollup/plugin-node-resolve \
  @rollup/plugin-commonjs \
  rollup-plugin-livereload \
  rollup-plugin-serve \
  rollup-plugin-terser
```

---

## 🧠 Step 2: Svelte as Web Components

Make each Svelte file self-contained by compiling as a **Custom Element**:

```svelte
<!-- src/HelloWorld.svelte -->
<svelte:options tag="hello-world" />

<script>
  export let name = "World";
</script>

<p>Hello {name}!</p>
```

---

## ⚙️ Step 3: Rollup Config

```js
// rollup.config.js
import svelte from 'rollup-plugin-svelte';
import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import css from 'rollup-plugin-css-only';
import livereload from 'rollup-plugin-livereload';
import serve from 'rollup-plugin-serve';
import { terser } from 'rollup-plugin-terser';
import fs from 'fs';
import path from 'path';

const production = !process.env.ROLLUP_WATCH;

// Dynamically grab all .svelte files in /src
const componentFiles = fs.readdirSync('src').filter(f => f.endsWith('.svelte'));

const configs = componentFiles.map(file => {
  const name = path.parse(file).name;
  const inputPath = `src/${file}`;
  const outputPath = `public/${name}.js`;

  return {
    input: inputPath,
    output: {
      file: outputPath,
      format: 'esm',
      sourcemap: true,
    },
    plugins: [
      svelte({
        compilerOptions: {
          customElement: true
        }
      }),
      css({ output: `${name}.css` }),
      resolve(),
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
```

---

## 🖼️ Step 4: HTML to Test It

```html
<!-- public/index.html -->
<!DOCTYPE html>
<html>
<head>
  <title>Svelte Component Demo</title>
  <script type="module" src="HelloWorld.js"></script>
</head>
<body>
  <h2>Using HelloWorld</h2>
  <hello-world name="Team"></hello-world>
</body>
</html>
```

> 📦 Components load on demand using `<script type="module">`, no extra bootstrapping needed.

---

## 🧪 Step 5: Run Dev Mode

```bash
ROLLUP_WATCH=true npx rollup -c -w
```

* Auto-reloads on file change.
* Serves at [http://localhost:5000](http://localhost:5000)
* Each `.svelte` file gets compiled into its own `.js` file.

---

## 🧲 Integration in C# WebView2

In your C# app, load the component like this:

```html
<script type="module" src="HelloWorld.js"></script>
<hello-world name="From C# WebView"></hello-world>
```

✅ That’s it. No Svelte runtime, no Vite, no SPA headaches.

---

## ✅ Why This Rocks

* **Independent bundles** = lazy-load, cache, and use what you need.
* **Web components** = universal. Works in any frontend or host app.
* **Pure Svelte** = no framework bloat.
* **Hot reload** = dev-friendly.

---

## 🧰 Bonus Tips

* Want to namespace your components? Use prefixes in `tag`:

  ```svelte
  <svelte:options tag="ui-button" />
  ```
---

## 🚀 Summary

This setup gives you a clean, minimal Svelte component system:

* Write once (`.svelte`)
* Build individually
* Load anywhere (including inside C#)
* Customize or theme with props
* No framework lock-in