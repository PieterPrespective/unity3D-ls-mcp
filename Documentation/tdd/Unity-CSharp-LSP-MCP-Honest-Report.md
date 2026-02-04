# C# Language Server + MCP for Unity 6.x: The Honest Report

## The Problem

You want a C# language server running via MCP that Claude Code CLI can use for Unity 6.x code review. Three candidates exist (OmniSharp, Roslyn LS, csharp-ls), and the previous two reports contradicted each other. This report cuts through the noise.

---

## Executive Summary: None of Them Work Well for Unity via MCP

Let me be blunt: **no existing off-the-shelf LSP-via-MCP solution gives you reliable, production-quality Unity code analysis today.** Each option has a critical flaw that makes it unsuitable as-is. The honest recommendation is to **build a custom Roslyn-based MCP server** — and it's less work than you might think, because the hardest part (loading Unity projects into Roslyn) is already a solved problem.

Here's the three-way comparison, then the custom MCP server case.

---

## Option 1: OmniSharp (omnisharp-roslyn)

### What It Is
The original standalone C# language server. Built on Roslyn internally. Designed to work outside of Visual Studio/VS Code — the only LSP designed for standalone use.

### Unity Compatibility: ⭐⭐⭐ (Best of the three)
- **UnityEngine/UnityEditor resolution**: ✅ Works. OmniSharp has mature support for resolving Unity DLL references via HintPaths in .csproj files
- **Legacy .csproj (ToolsVersion 4.0)**: ✅ Handles it. OmniSharp has years of battle-testing with old-format project files
- **.asmdef handling**: ✅ Loads multiple .csproj files from solution
- **Preprocessor defines**: ✅ Reads them from the .csproj `<DefineConstants>` block
- **Project regeneration mid-session**: ⚠️ Problematic. Requires restart or manual reload notification

### The Fatal Flaw: Memory and Stability

OmniSharp has **documented, long-standing memory leak issues** that have never been fully resolved:

- GitHub issue #2423 (dotnet/vscode-csharp): OmniSharp growing from 350MB to 2GB+ within 30 minutes while idle, caught in an infinite loop of "queue update, load project, update, queue update, load project"
- GitHub issue #5378: Memory leaks that persist even after restarting VS Code
- GitHub issue #2623 (omnisharp-roslyn): Complete LSP failure on large projects (500k+ lines), memory fills up, requires killing the process
- GitHub issue #2418: Memory growing from 1GB to 4GB+ while editing a single line, spending all CPU in Roslyn's FindReferencesSearchEngine
- GitHub issue #723: 10GB memory usage when assemblies are present in the project folder (relevant because Unity projects have many DLLs in Library/)
- Issue #558 (omnisharp-vim): Memory usage **doubles** when used outside VS Code vs inside it

**For your use case** (headless MCP, long-running process, Unity project with many assemblies in Library/), OmniSharp will likely consume several GB of RAM and degrade over time. For a code review sub-agent that spins up, analyzes, and shuts down, this is manageable. For a persistent MCP server, it's a serious problem.

### MCP Integration Quality
- **mcp-language-server bridge**: Works. OmniSharp speaks standard LSP stdio, which the Go bridge handles
- **Feature coverage through bridge**: Go-to-definition, references, hover, diagnostics all pass through
- **What gets lost**: Semantic highlighting tokens, some completion details, workspace/symbol search can be slow

### Verdict: "Works but will hurt you"
OmniSharp is the **only option that actually resolves Unity namespaces out of the box.** But its memory behavior makes it unreliable for sustained use. The Neovim/Unity community (who face the exact same challenge) uses OmniSharp but with specific workarounds:
- Pin to older versions (v1.38.2 reported as most stable for Unity)
- Require Mono on macOS/Linux (the .NET version behaves differently)
- Accept periodic restarts as normal workflow

---

## Option 2: Roslyn Language Server (Microsoft.CodeAnalysis.LanguageServer)

### What It Is
Microsoft's newer, actively-developed C# language server. Powers VS Code's C# Dev Kit extension. Built directly from the Roslyn codebase.

### Unity Compatibility: ⭐⭐ (Conditional)
- **UnityEngine/UnityEditor resolution**: ⚠️ Works IF project files have correct HintPaths AND you send the right initialization notifications
- **Legacy .csproj (ToolsVersion 4.0)**: ⚠️ Supports it, but expects specific initialization workflow
- **.asmdef handling**: ✅ Loads multiple projects from solution
- **Preprocessor defines**: ✅ Reads from .csproj
- **Project regeneration mid-session**: ❌ Worse than OmniSharp — no file watcher built in

### The Fatal Flaw: Not Designed for Standalone Use

Roslyn LS was built **exclusively** for VS Code's C# Dev Kit. It requires custom LSP notifications that no generic MCP bridge sends:

1. **`workspace/open`** — Must be sent after initialization to tell it which solution to load. Without this, Roslyn LS sits idle and analyzes nothing.
2. **`workspace/projectInitializationComplete`** — A notification Roslyn LS *sends* that clients must handle to know when analysis is ready.
3. **Custom initialization parameters** — VS Code passes specific extension data during `initialize` that Roslyn LS expects.

The Neovim community has solved this with dedicated plugins:
- **roslyn.nvim** (seblyng/roslyn.nvim) — A Neovim plugin that specifically handles Roslyn LS's custom protocol requirements
- **walcht/neovim-unity** — A complete guide showing the custom handlers needed, including handling `workspace/projectInitializationComplete` and manually requesting diagnostics after initialization

**The mcp-language-server bridge does NOT handle any of this.** It sends standard LSP initialization and expects standard LSP behavior. Roslyn LS will start, accept the connection, and then do nothing because it never received the `workspace/open` notification.

### Could You Fix the Bridge?
Yes, but you'd need to fork mcp-language-server (Go) and add:
- Custom `workspace/open` notification after `initialize` response
- Handler for `workspace/projectInitializationComplete`
- Possibly custom `_vs_*` methods that Roslyn LS expects

At that point, you're building a custom MCP server anyway — just with extra complexity of wrapping an LSP that wasn't designed for wrapping.

### Verdict: "Better engine, wrong interface"
Roslyn LS is technically superior to OmniSharp (active development, better Roslyn integration, lower memory footprint). But its VS Code coupling makes it impractical for MCP without significant custom work. The Neovim community's shift toward Roslyn LS validates its quality — but they had to write custom plugins to make it work.

---

## Option 3: csharp-ls (razzmatazz/csharp-language-server)

### What It Is
A lightweight, community-built C# language server. Designed for Neovim/Emacs users who want something simpler than OmniSharp.

### Unity Compatibility: ⭐ (Poor)
- **UnityEngine/UnityEditor resolution**: ❌ Fails. Cannot resolve absolute HintPaths to Unity installation directories
- **Legacy .csproj (ToolsVersion 4.0)**: ❌ Struggles with non-SDK-style projects
- **.asmdef handling**: ⚠️ Partial — often reports "No parent project could be resolved"
- **Preprocessor defines**: ⚠️ Misses many Unity-specific defines
- **Project regeneration**: ❌ No solution reloading

### The Fatal Flaw: Not Built for Unity's Project Structure

csharp-ls was built for standard .NET SDK-style projects. Unity's legacy .csproj format with absolute DLL paths, Mono-based runtime targeting, and non-standard project structure is fundamentally incompatible.

Common errors with Unity projects:
- "Could not find type or namespace UnityEngine"
- "No parent project could be resolved"
- Syntax-only analysis (no semantic understanding of Unity types)

### Verdict: "Don't bother for Unity"
csharp-ls is excellent for standard .NET projects. It is the wrong tool for Unity. Period.

---

## Option 4: Existing Roslyn MCP Servers (Bypass LSP Entirely)

Several projects already exist that use Roslyn directly as a library (not via LSP) to expose code analysis through MCP:

### carquiza/RoslynMCP
- Uses MSBuildWorkspace to load .sln files
- Exposes: symbol search, reference tracking, dependency analysis, complexity metrics
- **Unity compatibility**: Untested, but MSBuildWorkspace *can* load Unity .csproj files if MSBuild/Mono is configured correctly

### egorpavlikhin/roslyn-mcp
- ValidateFile: Runs Roslyn diagnostics on individual files within project context
- FindUsages: Symbol reference finding
- **Simpler scope** — focused on validation and usage finding

### dotnet-roslyn-mcp (NuGet tool)
- 24+ tools including impact analysis, dead code detection, safe refactoring
- Most feature-complete of the existing options
- Uses MSBuildWorkspace internally

### The Problem with All Three
**None of them were built with Unity in mind.** They all use `MSBuildWorkspace.Create()` with default settings, which means:

1. They expect SDK-style .csproj files (Unity uses legacy ToolsVersion="4.0")
2. They don't configure the MSBuild properties Unity needs (framework paths, reference assembly locations)
3. They don't handle Unity's absolute HintPath DLL references
4. They will likely fail with "The imported project Microsoft.CSharp.targets was not found" or similar MSBuild resolution errors

**However** — and this is the key insight — **these servers prove that the architecture works.** The pattern of "load solution → get compilation → expose analysis via MCP tools" is sound. The gap is Unity-specific MSBuild configuration.

---

## The Honest Recommendation: Build a Custom Unity-Aware Roslyn MCP Server

### Why This Is the Right Answer

1. **The hard problem is already solved.** Microsoft's own Microsoft.Unity.Analyzers project proves that Roslyn can analyze Unity code — it's a set of Roslyn analyzers that understand Unity-specific patterns (MonoBehaviour lifecycle, SerializeField, CompareTag, etc.). These analyzers run inside Roslyn's compilation pipeline.

2. **Loading Unity projects into Roslyn is a solved problem.** MSBuildWorkspace can load Unity's legacy .csproj files. The trick is providing the right MSBuild properties so it can find the .NET Framework reference assemblies and Unity's DLLs.

3. **Existing Roslyn MCP servers give you 80% of the code.** Fork carquiza/RoslynMCP or dotnet-roslyn-mcp and add Unity-specific workspace configuration.

4. **You control the tool surface.** Instead of hoping an LSP-to-MCP bridge passes through the right features, you define exactly what tools Claude Code gets: diagnostics, references, hover info, complexity analysis, Unity API migration checks.

### What You'd Need to Build

```
┌─────────────────────────────────┐
│     Claude Code CLI (MCP)       │
└──────────────┬──────────────────┘
               │ stdio (JSON-RPC)
┌──────────────▼──────────────────┐
│   Unity Roslyn MCP Server       │
│                                 │
│  ┌───────────────────────────┐  │
│  │  MCP Protocol Handler     │  │
│  │  (Tool registration,      │  │
│  │   request routing)        │  │
│  └────────────┬──────────────┘  │
│               │                 │
│  ┌────────────▼──────────────┐  │
│  │  Unity Workspace Loader   │  │
│  │  - Configure MSBuild      │  │
│  │  - Set FrameworkPathOverride│ │
│  │  - Handle HintPaths       │  │
│  │  - Load .sln/.csproj      │  │
│  └────────────┬──────────────┘  │
│               │                 │
│  ┌────────────▼──────────────┐  │
│  │  Analysis Engine          │  │
│  │  - Roslyn Compilation     │  │
│  │  - SemanticModel queries  │  │
│  │  - Microsoft.Unity.       │  │
│  │    Analyzers integration  │  │
│  │  - Custom Unity 6000.x    │  │
│  │    migration rules        │  │
│  └───────────────────────────┘  │
└─────────────────────────────────┘
```

### Key Implementation Details

**1. Unity Workspace Loader (the critical piece)**

The core challenge is configuring MSBuildWorkspace so it can process Unity's .csproj files:

```csharp
// Pseudocode — the key MSBuild properties Unity needs
var properties = new Dictionary<string, string>
{
    // Tell MSBuild where to find .NET Framework reference assemblies
    {"FrameworkPathOverride", "/path/to/mono/lib/mono/4.7.1-api"},
    
    // Or on systems with .NET Framework targeting pack:
    {"TargetFrameworkRootPath", "/usr/lib/mono/xbuild-frameworks"},
    
    // Suppress ToolsVersion warnings for legacy format
    {"MSBuildToolsVersion", "Current"},
};

var workspace = MSBuildWorkspace.Create(properties);
workspace.SkipUnrecognizedProjects = true;

// Unity .sln files reference all assembly .csproj files
var solution = await workspace.OpenSolutionAsync(solutionPath);

// Now you have full Roslyn Compilation for each project
foreach (var project in solution.Projects)
{
    var compilation = await project.GetCompilationAsync();
    // compilation.GetDiagnostics() — compiler errors/warnings
    // compilation.GetSemanticModel(tree) — type info, references
}
```

**2. MCP Tools to Expose**

For a code review sub-agent, you need these tools:

| Tool | Roslyn API | Purpose |
|------|-----------|---------|
| `validate_file` | `Compilation.GetDiagnostics()` | Compiler errors & warnings for a file |
| `get_symbol_info` | `SemanticModel.GetSymbolInfo()` | Type/method information at a position |
| `find_references` | `SymbolFinder.FindReferencesAsync()` | All usages of a symbol across solution |
| `find_definition` | `SymbolFinder.FindSourceDefinitionAsync()` | Jump to where something is defined |
| `get_type_hierarchy` | `SymbolFinder.FindDerivedClassesAsync()` | MonoBehaviour inheritance chains |
| `analyze_unity_patterns` | Microsoft.Unity.Analyzers | Unity-specific code quality checks |
| `check_api_migration` | Custom analyzer | Flag deprecated Unity 6000.x APIs |
| `get_complexity` | SyntaxWalker | Cyclomatic complexity per method |

**3. Microsoft.Unity.Analyzers Integration**

This is the secret weapon. Microsoft maintains 25+ Unity-specific analyzers that catch real Unity bugs:

- UNT0001: Empty Unity message (empty Update/Start methods)
- UNT0002: Inefficient tag comparison (use CompareTag)
- UNT0006: Incorrect message signature
- UNT0010: MonoBehaviour created with `new` instead of AddComponent
- UNT0014: Invalid method for GetComponent
- UNT0022: Unity objects should not use null coalescing
- UNT0024: Prefer float math over double
- USP0004-USP0023: Suppressors that prevent false positive C# warnings on Unity patterns

You can load these analyzers into your Roslyn compilation:

```csharp
var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
    // Load from the Microsoft.Unity.Analyzers NuGet package DLL
    Assembly.LoadFrom("Microsoft.Unity.Analyzers.dll")
        .GetTypes()
        .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t))
        .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
        .ToArray()
);

var results = await compilation
    .WithAnalyzers(analyzers)
    .GetAllDiagnosticsAsync();
```

### Effort Estimate

| Component | Effort | Notes |
|-----------|--------|-------|
| MCP server scaffold | 1-2 days | Fork dotnet-roslyn-mcp, it has the MCP protocol handling done |
| Unity workspace loader | 2-3 days | The hardest part — getting MSBuild properties right for Unity's legacy format. Test across platforms. |
| Core analysis tools | 1-2 days | Most are thin wrappers around Roslyn APIs |
| Unity analyzer integration | 1 day | Load Microsoft.Unity.Analyzers DLL, wire into compilation |
| Custom Unity 6000.x migration rules | 2-3 days | Write Roslyn DiagnosticAnalyzers for your specific API migration concerns |
| Testing & edge cases | 2-3 days | Assembly definitions, editor vs runtime assemblies, platform defines |
| **Total** | **~2 weeks** | For a solid, Unity-aware code analysis MCP server |

### Risks and Mitigations

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| MSBuildWorkspace fails to load Unity .csproj | Medium | Fall back to AdhocWorkspace with manually parsed references from .csproj XML |
| Roslyn version mismatch with Unity's compiler | Low | Unity uses Roslyn 3.8 internally, but analyzers run externally — version is flexible |
| High memory on large Unity projects | Medium | Load only the projects/assemblies relevant to the diff being reviewed |
| MSBuild not finding .NET Framework targets on Linux | High | Ship a minimal reference assemblies package or use Mono's targeting packs |

### The AdhocWorkspace Fallback

If MSBuildWorkspace proves too fragile with Unity's legacy .csproj files, you can bypass MSBuild entirely:

```csharp
// Parse the .csproj XML directly
var csproj = XDocument.Load("Assembly-CSharp.csproj");
var sourceFiles = csproj.Descendants("Compile")
    .Select(e => e.Attribute("Include")?.Value)
    .Where(v => v != null);
var references = csproj.Descendants("HintPath")
    .Select(e => e.Value);

// Build a Roslyn workspace manually
var workspace = new AdhocWorkspace();
var projectInfo = ProjectInfo.Create(
    ProjectId.CreateNewId(),
    VersionStamp.Default,
    "Assembly-CSharp",
    "Assembly-CSharp",
    LanguageNames.CSharp,
    parseOptions: new CSharpParseOptions(
        LanguageVersion.CSharp9,
        preprocessorSymbols: new[] { "UNITY_EDITOR", "UNITY_6000_0_OR_NEWER", /* ... */ }
    ),
    metadataReferences: references.Select(r => 
        MetadataReference.CreateFromFile(r))
);
var project = workspace.AddProject(projectInfo);
foreach (var file in sourceFiles)
{
    workspace.AddDocument(project.Id, Path.GetFileName(file), 
        SourceText.From(File.ReadAllText(file)));
}
```

This is more work but gives you complete control and zero MSBuild dependencies. It's what you'd do if MSBuildWorkspace proves unreliable.

---

## Final Decision Matrix

| Factor | OmniSharp via MCP | Roslyn LS via MCP | csharp-ls via MCP | Custom Roslyn MCP |
|--------|-------------------|-------------------|-------------------|-------------------|
| Unity namespace resolution | ✅ Works | ⚠️ Needs custom init | ❌ Fails | ✅ Full control |
| Setup effort | 1-2 hours | Broken without custom work | 30 minutes (then fails) | ~2 weeks |
| Memory stability | ❌ Leaks over time | ✅ Better than OmniSharp | ✅ Lightweight | ✅ You control it |
| Long-running reliability | ❌ Degrades | ❌ Needs custom protocol | ❌ Wrong tool | ✅ Purpose-built |
| Unity-specific analysis | ❌ Generic C# only | ❌ Generic C# only | ❌ Generic C# only | ✅ Microsoft.Unity.Analyzers |
| Unity 6000.x migration checks | ❌ None | ❌ None | ❌ None | ✅ Custom rules |
| Active development | ❌ Maintenance mode | ✅ Active | ⚠️ Community | ✅ Yours |
| MCP tool surface | Limited by LSP bridge | Limited by LSP bridge | Limited by LSP bridge | ✅ Exactly what you need |
| Code review agent fit | ⚠️ Generic | ⚠️ Generic | ❌ Poor | ✅ Purpose-built for it |

---

## What I'd Actually Do

**Phase 1 (Today):** Get OmniSharp working via mcp-language-server as a "good enough" stopgap. Accept the memory issues. Restart the MCP server between review sessions. This gives you basic diagnostics, go-to-definition, and references while you build the real thing.

**Phase 2 (2 weeks):** Fork dotnet-roslyn-mcp. Add the Unity workspace loader. Integrate Microsoft.Unity.Analyzers. Build the MCP tools your code review sub-agent actually needs (validate file, find references, check Unity patterns, flag deprecated APIs). This becomes your permanent solution.

**Phase 3 (Ongoing):** Add custom Roslyn DiagnosticAnalyzers for your team's specific Unity coding standards and Unity 6000.x migration requirements. This is where the custom MCP server really pays off — you can encode your team's knowledge into automated checks that run on every code review.

---

## Appendix: What the Neovim Community Actually Does

The Neovim Unity developer community (who face the *exact same* "non-VS Code LSP" challenge) has converged on two approaches:

1. **OmniSharp with workarounds** (legacy, still common): Pin to v1.38.2, require Mono, accept periodic restarts. Multiple blog posts and guides document this workflow, all noting it's painful but functional.

2. **Roslyn LS with custom plugins** (modern, growing): Using roslyn.nvim or walcht/neovim-unity, which handle the custom protocol requirements. This is where the community is moving, with multiple Medium articles and GitHub guides from late 2024/2025 recommending this switch.

Neither community has solved the MCP angle — you'd be among the first to build a proper Unity-aware Roslyn MCP server.
