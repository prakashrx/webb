# WebUI Platform Refactoring - Master Task List

## 🎯 Project Overview
Transform WebUI from simple browser window framework to VS Code-style extension platform for trading applications.

---

## **PHASE 1: PROJECT STRUCTURE & NAMING**

### **Task 1A: Rename Solution and Core Projects**
- [x] Rename `WebUI.sln` → `WebUI.Platform.sln`
- [x] Rename project folder `WebUI/` → `WebUI.Shell/`
- [x] Rename project folder `WebUI.Framework/` → `WebUI.Core/`
- [x] Rename project folder `WebUI.Components/` → `WebUI.UIComponents/`
- [x] Update solution file project references
- [x] Update .csproj file names:
  - [x] `WebUI.csproj` → `WebUI.Shell.csproj`
  - [x] `WebUI.Framework.csproj` → `WebUI.Core.csproj`
  - [x] Update WebUI.Components structure (no rename needed)
- [x] Update project references in each .csproj file
- [x] Test solution builds successfully
- [x] Update git ignore patterns if needed
- [x] **VALIDATE MEMORY DOCUMENTS:**
  - [x] Update `.cursor/memory/project-overview.md` with new naming
  - [x] Update `PROJECT_STRUCTURE.md` with new solution/project names
  - [x] Check all memory files for old references (`WebUI.sln`, `WebUI.Framework`, etc.)
  - [x] Update any code examples in memory with new namespaces

### **Task 1B: Restructure Folder Hierarchy**
- [x] Create new top-level folder structure:
  - [x] Create `Shell/` folder
  - [x] Create `Host/` folder (placeholder for future)
  - [x] Create `Core/` folder
  - [x] Create `Extensions/` folder (placeholder for future)
  - [x] Create `UIComponents/` folder
  - [x] Create `Tools/` folder (placeholder for future)
- [x] Move projects to new locations:
  - [x] Move `WebUI.Shell/` content to `Shell/` (direct, no nesting)
  - [x] Move `WebUI.Core/` content to `Core/` (direct, no nesting)
  - [x] Move `WebUI.UIComponents/` content to `UIComponents/` (direct, no nesting)
- [x] Update solution file with new project paths
- [x] Update relative paths in project references
- [x] Test solution builds successfully
- [x] **VALIDATE MEMORY DOCUMENTS:**
  - [x] Update memory files with new folder structure
  - [x] Update any diagrams or structure references
  - [x] Verify all file paths are correct in documentation

### **Task 1C: Update Project References and Dependencies**
- [ ] Update all ProjectReference paths in .csproj files
- [ ] Update using statements in C# files (if needed)
- [ ] Update any hardcoded paths in configuration files
- [ ] Test all projects build individually
- [ ] Test solution builds and runs successfully
- [ ] Update PROJECT_STRUCTURE.md with new structure
- [ ] Commit changes to git
- [ ] **VALIDATE MEMORY DOCUMENTS:**
  - [ ] Ensure all memory files reference correct project names
  - [ ] Update any build/deployment instructions
  - [ ] Verify code examples compile with new references

---

## **PHASE 2: NAMESPACE & API REFACTORING**

### **Task 2A: Refactor C# Namespaces**
- [ ] Rename `WebUI.Framework` namespace to `WebUI.Core.Windows` in:
  - [ ] `BrowserWindow.cs`
  - [ ] `WebViewHost.cs`
  - [ ] `IWebViewHost.cs`
  - [ ] `WebViewOptions.cs`
  - [ ] `WindowControls.cs`
  - [ ] `WindowHelper.cs`
- [ ] Update using statements in Shell project:
  - [ ] `MainForm.cs`
  - [ ] `Program.cs`
- [ ] Update GlobalUsings.cs if needed
- [ ] Test compilation after namespace changes
- [ ] Update examples and documentation with new namespaces
- [ ] **VALIDATE MEMORY DOCUMENTS:**
  - [ ] Update all memory files with new namespace references
  - [ ] Update API documentation with `WebUI.Core.Windows` namespace
  - [ ] Update code examples in memory files
  - [ ] Verify using statements are correct in all examples

### **Task 2B: Create New Namespace Structure**
- [ ] Create new namespace folders in Core project:
  - [ ] Create `WebUI.Core.IPC/` folder
  - [ ] Create `WebUI.Core.Extensions/` folder
  - [ ] Create `WebUI.Shared/` folder (new project)
- [ ] Create WebUI.Shared project:
  - [ ] Create `Shared/WebUI.Shared/WebUI.Shared.csproj`
  - [ ] Add shared contracts and models
  - [ ] Add to solution file
- [ ] Move appropriate classes to new namespaces:
  - [ ] Keep window management in `WebUI.Core.Windows`
  - [ ] Prepare structure for IPC classes
- [ ] Update project references to include WebUI.Shared
- [ ] Test all projects build successfully

### **Task 2C: Rename API Surface**
- [ ] Plan JavaScript API name changes:
  - [ ] `window.host` → `window.api`
  - [ ] `acquireHostApi()` → `getAPI()`
  - [ ] `host.commands.executeCommand()` → `api.commands.run()`
- [ ] Update WebView2 host object injection:
  - [ ] Change host object name in BrowserWindow.cs
  - [ ] Update JavaScript examples in MainForm.cs
- [ ] Update documentation with new API names
- [ ] Test JavaScript API calls work with new names
- [ ] **VALIDATE MEMORY DOCUMENTS:**
  - [ ] Update all JavaScript examples with `window.api` instead of `window.host`
  - [ ] Update API reference documentation
  - [ ] Update tutorial examples with new API names
  - [ ] Verify all code snippets use correct API surface

---

## **PHASE 3: CORE API ARCHITECTURE**

### **Task 3A: Create HostApiBridge Class Structure**
- [ ] Create `WebUI.Core.IPC/HostApiBridge.cs`:
  - [ ] Define main HostApiBridge class
  - [ ] Add CommandsApi property
  - [ ] Add EventsApi property  
  - [ ] Add WindowApi property
  - [ ] Add ServicesApi property
  - [ ] Add proper COM visibility attributes
- [ ] Create base API interface:
  - [ ] Create `IHostApi.cs` interface
  - [ ] Define common API contract
- [ ] Create placeholder API classes:
  - [ ] `CommandsApi.cs`
  - [ ] `EventsApi.cs`
  - [ ] `WindowApi.cs`
  - [ ] `ServicesApi.cs`
- [ ] Test HostApiBridge instantiation

### **Task 3B: Implement Modular API Components**
- [ ] Implement CommandsApi:
  - [ ] `RegisterCommand(string id, Action handler)`
  - [ ] `ExecuteCommand(string id, object args)`
  - [ ] `UnregisterCommand(string id)`
  - [ ] Command registry dictionary
- [ ] Implement EventsApi:
  - [ ] `Subscribe(string eventName, Action<object> handler)`
  - [ ] `Unsubscribe(string eventName, string handlerId)`
  - [ ] `Emit(string eventName, object data)`
  - [ ] Event subscription management
- [ ] Implement WindowApi:
  - [ ] `CreatePanel(string id, string title, string url)`
  - [ ] `ClosePanel(string id)`
  - [ ] `SetPanelTitle(string id, string title)`
  - [ ] Panel management dictionary
- [ ] Implement ServicesApi:
  - [ ] `GetService<T>(string serviceName)`
  - [ ] Service registry pattern
  - [ ] Storage service interface
- [ ] Test each API component individually

### **Task 3C: Update WebView2 Injection Mechanism**
- [ ] Update BrowserWindow.cs injection:
  - [ ] Replace direct WindowControls injection
  - [ ] Inject HostApiBridge instead
  - [ ] Update initialization timing
- [ ] Update MainForm.cs JavaScript:
  - [ ] Replace `window.chrome.webview.hostObjects.windowControls`
  - [ ] Use `window.api.window` instead
  - [ ] Update all API calls in demo HTML
- [ ] Create JavaScript API wrapper:
  - [ ] Create client-side API abstraction
  - [ ] Handle async COM calls properly
  - [ ] Add error handling
- [ ] Test new API injection works end-to-end

---

## **PHASE 4: EXTENSION FOUNDATION**

### **Task 4A: Create Extension Manifest Schema**
- [ ] Define manifest.json schema:
  - [ ] Basic extension metadata (id, name, version)
  - [ ] Entry point definition (main file)
  - [ ] Contribution points (commands, panels, events)
  - [ ] Dependencies and permissions
- [ ] Create JSON schema file:
  - [ ] `Shared/extension-manifest.schema.json`
  - [ ] Validation rules and constraints
- [ ] Create example extension manifests:
  - [ ] Simple hello-world extension
  - [ ] Market data extension example
  - [ ] Toolbar extension example
- [ ] Create C# manifest model classes:
  - [ ] `ExtensionManifest.cs`
  - [ ] `ContributionPoint.cs`
  - [ ] JSON deserialization attributes

### **Task 4B: Implement Basic Extension Loader**
- [ ] Create `WebUI.Core.Extensions/ExtensionLoader.cs`:
  - [ ] `LoadExtensionAsync(string extensionPath)`
  - [ ] `GetExtensionMetadata(string id)`
  - [ ] `UnloadExtension(string id)`
  - [ ] Extension lifecycle management
- [ ] Create extension context:
  - [ ] `ExtensionContext.cs`
  - [ ] API access for extensions
  - [ ] Extension-specific state management
- [ ] Create extension registry:
  - [ ] `ExtensionRegistry.cs`
  - [ ] Track loaded extensions
  - [ ] Handle extension dependencies
- [ ] Implement manifest validation:
  - [ ] JSON schema validation
  - [ ] Security checks
  - [ ] Version compatibility
- [ ] Test extension discovery and loading

### **Task 4C: Create Extension Registration System**
- [ ] Create extension activation pattern:
  - [ ] `activate(context)` function contract
  - [ ] Context API provision
  - [ ] Extension cleanup on deactivation
- [ ] Implement command registration:
  - [ ] Extension commands in manifest
  - [ ] Runtime command registration
  - [ ] Command namespace isolation
- [ ] Implement panel registration:
  - [ ] Panel contribution points
  - [ ] Dynamic panel creation
  - [ ] Panel lifecycle management
- [ ] Create extension communication:
  - [ ] Inter-extension message passing
  - [ ] Event system for extensions
  - [ ] Shared state management
- [ ] Test end-to-end extension registration

---

## **PHASE 5: DEVELOPER EXPERIENCE**

### **Task 5A: Update Documentation and Examples**
- [ ] Update PROJECT_STRUCTURE.md:
  - [ ] New folder structure
  - [ ] New namespace organization
  - [ ] Updated API references
- [ ] Create extension development guide:
  - [ ] Extension manifest format
  - [ ] API usage examples
  - [ ] Best practices
- [ ] Create API documentation:
  - [ ] CommandsApi reference
  - [ ] EventsApi reference
  - [ ] WindowApi reference
  - [ ] ServicesApi reference
- [ ] Create tutorial examples:
  - [ ] Simple extension tutorial
  - [ ] Panel creation tutorial
  - [ ] Command registration tutorial
- [ ] Update inline code comments

### **Task 5B: Create Extension Scaffolding Tools**
- [ ] Create CLI tool project:
  - [ ] `Tools/WebUI.CLI/WebUI.CLI.csproj`
  - [ ] Command-line argument parsing
  - [ ] Template generation system
- [ ] Implement `create-extension` command:
  - [ ] Generate extension folder structure
  - [ ] Create manifest.json template
  - [ ] Generate starter Svelte component
  - [ ] Set up build configuration
- [ ] Create extension templates:
  - [ ] Basic panel template
  - [ ] Command palette extension
  - [ ] Data visualization panel
  - [ ] Settings panel template
- [ ] Implement debugging tools:
  - [ ] Extension validation tool
  - [ ] Development server for extensions
  - [ ] Hot reload support
- [ ] Test scaffolding tools generate working extensions

### **Task 5C: Validate End-to-End Extension Workflow**
- [ ] Create sample extension using scaffolding:
  - [ ] Generate extension with CLI tool
  - [ ] Implement basic functionality
  - [ ] Test in WebUI host
- [ ] Test extension lifecycle:
  - [ ] Extension loading and activation
  - [ ] Panel creation and management
  - [ ] Command registration and execution
  - [ ] Event subscription and handling
- [ ] Test developer workflow:
  - [ ] Extension development setup
  - [ ] Build and test process
  - [ ] Deployment and installation
- [ ] Performance validation:
  - [ ] Extension loading time
  - [ ] Memory usage per extension
  - [ ] IPC communication overhead
- [ ] Create comprehensive test suite:
  - [ ] Unit tests for core APIs
  - [ ] Integration tests for extensions
  - [ ] End-to-end workflow tests

---

---

## **MEMORY DOCUMENT VALIDATION CHECKLIST**

### **After Each Major Phase - Validate ALL Memory Documents**
- [ ] **Phase 1 Complete**: All memory docs reflect new project/solution names
- [ ] **Phase 2 Complete**: All memory docs use new namespaces and API names  
- [ ] **Phase 3 Complete**: All memory docs show new API architecture
- [ ] **Phase 4 Complete**: All memory docs include extension concepts
- [ ] **Phase 5 Complete**: All memory docs show final developer experience

### **Memory Documents to Check After Each Phase**
- [ ] `.cursor/memory/project-overview.md`
- [ ] `.cursor/memory/api-ideas.md`
- [ ] `.cursor/memory/component-library.md`
- [ ] `PROJECT_STRUCTURE.md`
- [ ] Any other memory files with code examples or references

---

## **COMPLETION CRITERIA**

### **Definition of Done**
- [ ] All tasks completed and checked off
- [ ] Solution builds without errors or warnings
- [ ] All tests pass
- [ ] Documentation updated and comprehensive
- [ ] Sample extensions work end-to-end
- [ ] Performance benchmarks meet requirements
- [ ] Code review completed
- [ ] Ready for junior developer onboarding

### **Success Metrics**
- [ ] Extension development time < 30 minutes for simple panels
- [ ] API learning curve acceptable for junior developers
- [ ] Extension loading time < 500ms
- [ ] Memory overhead < 50MB per extension
- [ ] Zero breaking changes to existing BrowserWindow API

---

## **CURRENT STATUS: PHASE 1 - TASK 1C**
**Next Action**: Update Project References and Dependencies

**Current Progress**: 2/15 tasks completed (13.3%)

**CRITICAL REMINDER**: After each task completion, ALWAYS validate and update memory documents to maintain consistency throughout the refactoring process. 