<script lang="ts">
  import { invoke } from '@webui/api';
  import { TitleBar } from '@webui/components';
  import { onMount } from 'svelte';
  import * as d3 from 'd3';

  let container: HTMLDivElement;
  let svg: d3.Selection<SVGSVGElement, unknown, null, undefined>;
  let width = 0;
  let height = 0;

  let gravity = 0.5;
  let friction = 0.99;
  let showConnections = true;
  let balls: any[] = [];
  let ballCount = 0;

  function createBalls(n: number) {
    const newBalls = d3.range(n).map(() => {
      const r = 10 + Math.random() * 20;
      return {
        x: r + Math.random() * (width - 2 * r),
        y: r + Math.random() * (height / 2 - 2 * r),
        vx: (Math.random() - 0.5) * 8,
        vy: (Math.random() - 0.5) * 4,
        r: r,
        color: d3.interpolateRainbow(Math.random())
      };
    });
    ballCount = newBalls.length;
    return newBalls;
  }

  function setupSVG() {
    width = container.clientWidth;
    height = container.clientHeight;

    svg = d3.select(container)
      .html('') // clear existing content
      .append('svg')
      .attr('width', width)
      .attr('height', height)
      .style('background', 'linear-gradient(to bottom, #1e293b, #0f172a)')
      .style('border-radius', '12px')
      .style('cursor', 'crosshair')
      .style('display', 'block');

    // Add glow filter
    const defs = svg.append('defs');
    const filter = defs.append('filter')
      .attr('id', 'glow');
    filter.append('feGaussianBlur')
      .attr('stdDeviation', '3')
      .attr('result', 'coloredBlur');
    const feMerge = filter.append('feMerge');
    feMerge.append('feMergeNode')
      .attr('in', 'coloredBlur');
    feMerge.append('feMergeNode')
      .attr('in', 'SourceGraphic');
  }

  function renderBalls() {
    // Render connections first (behind balls)
    if (showConnections) {
      const lines = [];
      for (let i = 0; i < balls.length; i++) {
        for (let j = i + 1; j < balls.length; j++) {
          const dx = balls[i].x - balls[j].x;
          const dy = balls[i].y - balls[j].y;
          const distance = Math.sqrt(dx * dx + dy * dy);
          if (distance < 150) {
            lines.push({
              x1: balls[i].x,
              y1: balls[i].y,
              x2: balls[j].x,
              y2: balls[j].y,
              opacity: 1 - distance / 150
            });
          }
        }
      }

      const connections = svg.selectAll('line')
        .data(lines);

      connections.enter()
        .append('line')
        .merge(connections)
        .attr('x1', d => d.x1)
        .attr('y1', d => d.y1)
        .attr('x2', d => d.x2)
        .attr('y2', d => d.y2)
        .attr('stroke', '#60a5fa')
        .attr('stroke-width', 1)
        .attr('opacity', d => d.opacity * 0.3);

      connections.exit().remove();
    } else {
      svg.selectAll('line').remove();
    }

    // Render balls
    const circles = svg.selectAll('circle')
      .data(balls);

    circles.enter()
      .append('circle')
      .attr('r', d => d.r)
      .attr('fill', d => d.color)
      .attr('stroke', d => d3.color(d.color).brighter(1))
      .attr('stroke-width', 2)
      .style('filter', 'url(#glow)')
      .style('cursor', 'pointer')
      .on('click', function(event, d) {
        event.stopPropagation();
        const index = balls.indexOf(d);
        if (index > -1 && d.r > 15) {
          balls.splice(index, 1);
          for (let i = 0; i < 3; i++) {
            balls.push({
              x: d.x,
              y: d.y,
              vx: (Math.random() - 0.5) * 10,
              vy: (Math.random() - 0.5) * 10,
              r: d.r / 2,
              color: d3.interpolateRainbow(Math.random())
            });
          }
          ballCount = balls.length;
        }
      })
      .merge(circles)
      .attr('cx', d => d.x)
      .attr('cy', d => d.y);

    circles.exit().remove();
  }

  function updatePhysics() {
    for (const ball of balls) {
      // Apply gravity
      ball.vy += gravity;
      
      // Apply friction
      ball.vx *= friction;
      ball.vy *= friction;

      // Update position
      ball.x += ball.vx;
      ball.y += ball.vy;

      // Bounce off walls
      if (ball.x - ball.r <= 0 || ball.x + ball.r >= width) {
        ball.vx *= -0.9;
        ball.x = Math.max(ball.r, Math.min(width - ball.r, ball.x));
      }
      if (ball.y - ball.r <= 0 || ball.y + ball.r >= height) {
        ball.vy *= -0.9;
        ball.y = Math.max(ball.r, Math.min(height - ball.r, ball.y));
      }

      // Ball collision
      for (const other of balls) {
        if (ball === other) continue;
        const dx = ball.x - other.x;
        const dy = ball.y - other.y;
        const distance = Math.sqrt(dx * dx + dy * dy);
        const minDistance = ball.r + other.r;
        
        if (distance < minDistance && distance > 0) {
          const angle = Math.atan2(dy, dx);
          const targetX = other.x + Math.cos(angle) * minDistance;
          const targetY = other.y + Math.sin(angle) * minDistance;
          const ax = (targetX - ball.x) * 0.5;
          const ay = (targetY - ball.y) * 0.5;
          ball.vx += ax;
          ball.vy += ay;
          other.vx -= ax;
          other.vy -= ay;
        }
      }
    }
  }

  function resize() {
    const newWidth = container.clientWidth;
    const newHeight = container.clientHeight;
    
    if (newWidth !== width || newHeight !== height) {
      width = newWidth;
      height = newHeight;
      
      if (svg) {
        svg.attr('width', width).attr('height', height);
      }
      
      // Keep balls in bounds
      balls.forEach(ball => {
        ball.x = Math.max(ball.r, Math.min(width - ball.r, ball.x));
        ball.y = Math.max(ball.r, Math.min(height - ball.r, ball.y));
      });
    }
  }

  onMount(() => {
    setupSVG();
    balls = createBalls(15);
    renderBalls();

    // Use ResizeObserver for better resize detection
    const resizeObserver = new ResizeObserver(() => resize());
    resizeObserver.observe(container);

    // Click to add ball
    svg.on('click', (event: MouseEvent) => {
      const [x, y] = d3.pointer(event);
      balls.push({
        x,
        y,
        vx: (Math.random() - 0.5) * 8,
        vy: -10,
        r: 10 + Math.random() * 20,
        color: d3.interpolateRainbow(Math.random())
      });
      ballCount = balls.length;
      renderBalls();
    });

    // Mouse repulsion
    svg.on('mousemove', (event: MouseEvent) => {
      const [mouseX, mouseY] = d3.pointer(event);
      for (const ball of balls) {
        const dx = ball.x - mouseX;
        const dy = ball.y - mouseY;
        const distance = Math.sqrt(dx * dx + dy * dy);
        if (distance < 100 && distance > 0) {
          const force = (100 - distance) / 100;
          ball.vx += (dx / distance) * force * 2;
          ball.vy += (dy / distance) * force * 2;
        }
      }
    });

    // Animation loop
    const timer = d3.timer(() => {
      updatePhysics();
      renderBalls();
    });

    // Cleanup
    return () => {
      timer.stop();
      resizeObserver.disconnect();
    };
  });
</script>

<div class="flex flex-col h-screen bg-gray-900 overflow-hidden">
  <TitleBar title="Interactive Physics Playground" />
  
  <main class="flex-1 flex flex-col p-4 md:p-6 overflow-hidden">
    <div class="flex flex-col h-full">
      <div class="text-center mb-4 flex-shrink-0">
        <h1 class="text-2xl md:text-4xl font-bold text-white mb-1">Bouncing Balls Physics</h1>
        <p class="text-sm md:text-lg text-gray-400">Click to add balls • Move mouse to repel • Click balls to split them</p>
      </div>
      
      <!-- Controls -->
      <div class="flex flex-wrap justify-center gap-4 mb-4 flex-shrink-0">
        <div class="flex items-center gap-2">
          <label class="text-gray-300 text-sm">Gravity:</label>
          <input 
            type="range" 
            min="0" 
            max="2" 
            step="0.1" 
            bind:value={gravity}
            class="w-24"
          />
          <span class="text-gray-400 text-sm w-8">{gravity}</span>
        </div>
        
        <div class="flex items-center gap-2">
          <label class="text-gray-300 text-sm">Friction:</label>
          <input 
            type="range" 
            min="0.9" 
            max="1" 
            step="0.01" 
            bind:value={friction}
            class="w-24"
          />
          <span class="text-gray-400 text-sm w-10">{friction.toFixed(2)}</span>
        </div>
        
        <label class="flex items-center gap-2 text-gray-300 cursor-pointer">
          <input type="checkbox" bind:checked={showConnections} class="w-4 h-4" />
          <span class="text-sm">Show Connections</span>
        </label>
        
        <div class="text-gray-400 text-sm">
          Balls: <span class="font-bold text-white">{ballCount}</span>
        </div>
      </div>
      
      <!-- Chart Container -->
      <div bind:this={container} class="chart-container flex-1"></div>
    </div>
  </main>
</div>

<style>
  .chart-container {
    width: 100%;
    border-radius: 12px;
    overflow: hidden;
    box-shadow: 0 20px 40px rgba(0,0,0,0.5);
  }
  
  input[type="range"] {
    @apply bg-gray-700 rounded-lg appearance-none cursor-pointer;
  }
  
  input[type="range"]::-webkit-slider-thumb {
    @apply appearance-none w-4 h-4 bg-blue-500 rounded-full cursor-pointer;
  }
  
  input[type="checkbox"] {
    @apply text-blue-500 bg-gray-700 border-gray-600 rounded;
  }
</style>