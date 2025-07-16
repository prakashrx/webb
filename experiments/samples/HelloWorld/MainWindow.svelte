<script>
  let count = 0;
  
  function increment() {
    count += 1;
  }
</script>

<main>
  <h1>Hello WebUI! 🚀</h1>
  <p>This is a Svelte component.</p>
  <button on:click={increment}>
    Clicked {count} {count === 1 ? 'time' : 'times'}
  </button>
</main>

<style>
  main {
    text-align: center;
    padding: 2em;
    max-width: 600px;
    margin: 0 auto;
  }
  
  h1 {
    color: #ff3e00;
    font-size: 3em;
    font-weight: 100;
  }
  
  button {
    font-size: 1.2em;
    padding: 0.5em 1em;
    border: 2px solid #ff3e00;
    background: white;
    color: #ff3e00;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.2s;
  }
  
  button:hover {
    background: #ff3e00;
    color: white;
  }
</style>