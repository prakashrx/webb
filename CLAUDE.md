# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 🚀 IMPORTANT: Experimental SDK Transition

**We are transitioning to a new architecture!** The `experiments/` directory contains our revolutionary WebUI.Desktop framework that will replace the current implementation. This new approach provides a WinForms-like development experience with modern web technologies, distributed as a single NuGet package.

### Experimental Framework Highlights
- **Simple API**: Just `WebUI.Run("MainWindow")` - no complex setup
- **Automatic Compilation**: Svelte files compile transparently during build
- **Integrated Tailwind**: CSS framework built-in with zero configuration  
- **Hot Reload**: Edit Svelte files and see changes instantly without app restart
- **Single NuGet Package**: Complete framework with runtime and build tools in one package

### Migration Strategy
We are gradually extracting the best parts of the reference implementation (current `src/` directory) and rebuilding them in the cleaner experimental architecture. The goal is to maintain the powerful features while drastically simplifying the developer experience.

## Project Evolution

### Phase 1: Reference Implementation (Current `src/` directory)
The original WebUI Platform - a VS Code-style extensible desktop shell with:
- Complex panel system with IPC communication
- Full windowing management with docking
- Extensive JavaScript API
- Trading application focus

### Phase 2: Experimental Framework (New `experiments/` directory) ⭐
Our new direction - a general-purpose desktop framework that's as easy as WinForms:
- **Zero-configuration** Svelte compilation
- **Automatic** Tailwind CSS integration
- **Built-in** hot reload for development
- **Simple** API surface - just panels and IPC
- **Standard** .NET tooling throughout

### Phase 3: Convergence (Coming Soon)
Bringing the best of both worlds:
- Migrate IPC system from reference → experimental
- Port WebUI JavaScript API incrementally  
- Add advanced features (docking, persistence) to new framework
- Deprecate old implementation once feature-complete

## Working with the Experimental Framework

### Quick Start
```xml
<!-- HelloWorld.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="WebUI.Desktop" Version="1.0.0" />
  </ItemGroup>
  <ItemGroup>
    <Panel Include="MainWindow.svelte" />
  </ItemGroup>
</Project>
```

```csharp
// Program.cs
using WebUI;
WebUI.WebUI.Run("MainWindow");
```

### Development Workflow
```bash
# Terminal 1: Watch and compile Svelte files
cd experiments/samples/HelloWorld
dotnet watch msbuild /t:watch

# Terminal 2: Run the app with hot reload
dotnet run
```

### Key Improvements Over Reference
1. **No manual build steps** - MSBuild handles everything
2. **No npm scripts to run** - Integrated into .NET build
3. **No complex setup** - Just reference the NuGet package
4. **Standard tooling** - Uses dotnet CLI throughout
5. **Instant feedback** - Hot reload without app restart

## Current Development Focus

### ✅ Completed in Experimental Framework
- Single NuGet package with integrated MSBuild targets for Svelte compilation
- Rollup-based build pipeline with ES modules  
- Automatic Tailwind CSS integration
- Hot reload using dotnet watch + FileSystemWatcher
- Virtual host serving (http://webui.local/)
- Simple panel loading system
- **Dynamic command discovery** - Convention-based `*Commands` classes
- **TypeScript WebUI API** - Tree-shakable ES modules with `invoke()`
- **Window management** - Minimize, maximize, title, state commands
- **Clean separation** - Framework vs app commands
- **Automated API build** - MSBuild handles WebUI.Api compilation

### 🔄 In Progress
- **Multi-panel support** - Refactoring for multiple windows
- **Builder pattern API** - `WebUI.CreateBuilder()` for configuration
- **Message bus** - Inter-panel communication system

### 📋 Next Steps
1. **Multi-Panel Architecture**: Support multiple windows/panels
2. **Panel Navigation API**: `webui.panel.open()`, `webui.panel.close()`
3. **Message Bus**: Pub/sub for panel communication
4. **TypeScript Generation**: Auto-generate types from C# commands
5. **Dialog API**: File open/save, message boxes
6. **File System API**: Read/write files with permissions

## Reference Implementation (Original)

The current `src/` directory contains the reference implementation with full features:

### Build Commands
```bash
# Build entire solution
cd src
dotnet build WebUI.Platform.sln

# Run main application  
cd src/Workbench
dotnet run

# Build UI components
cd src/UI/workbench
npm install
npm run build
```

### Architecture Overview
- **Workbench**: Main application with frameless toolbar
- **Core Libraries**: WebView2, window management, panel APIs
- **Panel System**: Complex but powerful panel architecture
- **IPC Router**: Sophisticated message routing system
- **WebUI API**: Full-featured JavaScript API

### Key Files for Migration Reference
- `src/Platform/Core/Communication/` - IPC system to adapt
- `src/Platform/Core/Api/` - JavaScript API to port
- `src/UI/api/` - TypeScript definitions to reuse
- `src/Platform/Core/Windows/` - Window management ideas

## Development Guidelines

### When Working on Experimental Framework
1. **Keep it simple** - Avoid over-engineering
2. **Use standard .NET patterns** - No custom abstractions
3. **Maintain compatibility** - Think about migration path
4. **Document changes** - Update EXPERIMENT_STATUS.md
5. **Test hot reload** - Ensure dev experience stays smooth

### When Referencing Original Code
1. **Extract concepts, not code** - Rewrite for simplicity
2. **Question every abstraction** - Do we really need it?
3. **Prioritize developer experience** - Easy > Powerful
4. **Maintain feature parity** - But with cleaner implementation

## Important Notes

### For Experimental Development
- Always test hot reload after changes
- Keep the MSBuild targets simple and maintainable
- Ensure Tailwind CSS continues to work automatically
- Don't break the "zero configuration" promise

### For Migration Work
- Study the reference implementation for ideas
- But implement fresh in the experimental framework
- Focus on the 80% use case, not edge cases
- Make the common case trivial, the complex case possible

## Summary

We are building the future of .NET desktop development with web UI. The experimental framework in `experiments/` is our north star - simple, powerful, and delightful to use. Distributed as a single NuGet package, it provides everything needed to build desktop apps with web technologies. The reference implementation in `src/` provides the feature roadmap, but we're reimagining everything with a focus on developer experience.

**Remember**: If it's not as easy as WinForms, we're not done yet! 🚀