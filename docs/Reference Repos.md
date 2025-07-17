## Hot Reload & File Watching Systems

**https://github.com/vitejs/vite**
- **Learn:** Modern HMR architecture, WebSocket-based live updates, module graph management
- **Key insights:** Efficient file watching, browser-server communication patterns, plugin system for different frameworks
- **Apply to your project:** WebSocket setup, client-side update handling, debouncing strategies

**https://github.com/webpack/webpack-dev-server**
- **Learn:** Mature hot reload implementation, multiple transport methods (WebSocket, SSE)
- **Key insights:** Resource serving during development, memory-based file system, overlay error handling
- **Apply to your project:** Error overlays, development server patterns, resource caching strategies

**https://github.com/sveltejs/kit**
- **Learn:** Svelte-specific hot reload, component state preservation during updates
- **Key insights:** How to handle Svelte component lifecycle during hot reload, CSS hot updates
- **Apply to your project:** Svelte compilation pipeline, component update strategies

**https://github.com/paulmillr/chokidar**
- **Learn:** Cross-platform file watching, performance optimization, edge case handling
- **Key insights:** Recursive watching, debouncing, handling symlinks and network drives
- **Apply to your project:** Robust file watching implementation, performance best practices

## ASP.NET Core & Blazor Hot Reload

**https://github.com/dotnet/aspnetcore**
- **Learn:** Enterprise-grade hot reload, SignalR for real-time updates, middleware patterns
- **Key insights:** How Microsoft implements hot reload at scale, error handling, security considerations
- **Apply to your project:** Middleware architecture, WebView2 integration patterns, production-ready error handling

**https://github.com/dotnet/sdk**
- **Learn:** Build system integration, MSBuild task creation, incremental compilation
- **Key insights:** How `dotnet watch` works internally, file change detection, build orchestration
- **Apply to your project:** MSBuild integration, watch mode implementation, incremental builds

## Custom MSBuild SDKs

**https://github.com/dotnet/sdk**
- **Learn:** Official SDK structure, target composition, property inheritance
- **Key insights:** How to create reusable build logic, packaging SDKs, versioning strategies
- **Apply to your project:** SDK architecture, target organization, NuGet packaging

**https://github.com/unoplatform/uno**
- **Learn:** Multi-platform build orchestration, conditional compilation, resource handling
- **Key insights:** Platform-specific builds, shared resource management, complex target dependencies
- **Apply to your project:** Multi-target builds, resource embedding, platform detection

**https://github.com/AvaloniaUI/Avalonia**
- **Learn:** XAML preprocessing, resource compilation, design-time support
- **Key insights:** Custom markup compilation, embedded resource strategies, IDE integration
- **Apply to your project:** Custom file processing, design-time builds, resource management

## WebView2 Integration

**https://github.com/MicrosoftEdge/WebView2Samples**
- **Learn:** Official WebView2 patterns, resource interception, JavaScript injection
- **Key insights:** Best practices for WebView2 apps, security considerations, debugging techniques
- **Apply to your project:** Resource handling, custom schemes, host-web communication

**https://github.com/ElectronNET/Electron.NET**
- **Learn:** Desktop web app architecture, process management, native-web bridging
- **Key insights:** Similar architectural patterns, process lifecycle, inter-process communication
- **Apply to your project:** Desktop app patterns, process management, native integration

## Source Generators & Build-Time Code Gen

**https://github.com/dotnet/roslyn**
- **Learn:** Official source generator patterns, MSBuild integration, incremental generation
- **Key insights:** Performance optimization, caching strategies, debugging generators
- **Apply to your project:** Build-time code generation, MSBuild hooks, performance optimization

**https://github.com/andrewlock/StronglyTypedId**
- **Learn:** Clean, focused source generator example, real-world usage patterns
- **Key insights:** Simple generator architecture, testing strategies, distribution
- **Apply to your project:** Generator structure, testing approaches, packaging

## Real-World Custom SDK Examples

**https://github.com/dotnet/maui**
- **Learn:** Modern mobile SDK architecture, hot reload for mobile, resource compilation
- **Key insights:** Complex build pipelines, platform abstraction, hot reload in constrained environments
- **Apply to your project:** Advanced build orchestration, resource handling, hot reload patterns

**https://github.com/xamarin/xamarin-android**
- **Learn:** Mobile build complexity, incremental builds, resource optimization
- **Key insights:** Managing large build systems, performance optimization, debugging build issues
- **Apply to your project:** Build performance, incremental compilation, complex dependencies

**https://github.com/microsoft/react-native-windows**
- **Learn:** JS + native integration, hot reload for hybrid apps, build system integration
- **Key insights:** Managing JS/native boundaries, hot reload across different runtimes
- **Apply to your project:** Hybrid app patterns, cross-runtime hot reload, build integration

## File System & Resource Management

**https://github.com/dotnet/sdk (dotnet watch)**
- **Learn:** Production-ready file watching, integration with MSBuild, performance considerations
- **Key insights:** Handling large projects, filtering strategies, memory management
- **Apply to your project:** Scalable file watching, MSBuild integration, production deployment

## What Each Teaches You

**Architecture Patterns:**
- Event-driven hot reload systems
- Plugin/middleware architectures
- Client-server communication patterns

**Performance Optimization:**
- Incremental compilation strategies
- Efficient file watching
- Memory management during development

**Integration Techniques:**
- MSBuild extensibility
- IDE integration
- WebView2 resource handling

**Production Considerations:**
- Error handling and recovery
- Security implications
- Deployment strategies

**Development Experience:**
- Error overlays and debugging
- State preservation during updates
- Build performance optimization

These repos collectively show you how to build a professional-grade development tool that integrates seamlessly with existing .NET tooling while providing a great developer experience.