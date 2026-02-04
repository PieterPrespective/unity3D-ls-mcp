# PP13-104: Phase 2 - Unity Workspace Loader

## Quick Start

```
Please read and execute the prompt in 'Prompts/PP13-104/PP13-104.md'
```

## Contents

| File | Description |
|------|-------------|
| [PP13-104.md](./PP13-104.md) | **Main assignment prompt** - Complete implementation instructions |
| [CHECKLIST.md](./CHECKLIST.md) | Quick reference checklist for tracking progress |

## Context

- **Parent Issue:** PP13-102 (ULSM Development)
- **Depends On:** PP13-103 (Phase 1 - Project Restructuring) ✅
- **Phase:** 2 of 5
- **Roadmap:** [Documentation/tdd/DevRoadmap.md](../../Documentation/tdd/DevRoadmap.md)
- **Technical Analysis:** [Unity-CSharp-LSP-MCP-Honest-Report.md](../../Documentation/tdd/Unity-CSharp-LSP-MCP-Honest-Report.md)
- **Base Prompt:** [Prompts/BasePrompt.md](../BasePrompt.md)

## Objective

Enable ULSM to load Unity 6.x projects by creating a specialized workspace loader that handles:

1. **Unity's legacy .csproj format** (ToolsVersion 4.0)
2. **Absolute HintPath DLL references** to Unity installation
3. **Platform-specific framework path resolution** (Windows/macOS/Linux)
4. **AdhocWorkspace fallback** when MSBuildWorkspace fails

## Architecture

```
RoslynService.LoadSolutionAsync(solutionPath)
    │
    ├─► UnityProjectDetector.IsUnityProject()
    │
    ├─► IF Unity:
    │     └─► UnityWorkspaceLoader.LoadSolutionAsync()
    │           │
    │           ├─► Try MSBuildWorkspace with Unity properties
    │           │
    │           └─► FALLBACK: UnityAdhocWorkspaceBuilder
    │
    └─► ELSE:
          └─► Standard MSBuildWorkspace (existing)
```

## New Files

| File | Purpose |
|------|---------|
| `src/Unity/UnityProjectDetector.cs` | Detect Unity projects |
| `src/Unity/UnityWorkspaceLoader.cs` | MSBuildWorkspace configuration |
| `src/Unity/UnityAdhocWorkspaceBuilder.cs` | Fallback workspace builder |

## Success Criteria

- Unity projects auto-detected by folder structure, project files, or references
- MSBuildWorkspace configured with correct framework path
- Fallback to AdhocWorkspace works when MSBuild fails
- Health check reports `isUnityProject` and `unityVersion`
- `dotnet build ULSM.sln` succeeds

## Key Technical Challenges

| Challenge | Solution |
|-----------|----------|
| ToolsVersion 4.0 warnings | Set `MSBuildToolsVersion=Current` |
| Framework path resolution | Platform-specific search + env var override |
| MSBuild failure | AdhocWorkspace fallback with .csproj parsing |
| Unity DLL resolution | Parse HintPaths directly from .csproj XML |

## Environment Variables

| Variable | Purpose |
|----------|---------|
| `ULSM_FRAMEWORK_PATH` | Override framework reference assemblies path |
| `ULSM_FORCE_ADHOC` | Force AdhocWorkspace (bypass MSBuild) |
| `UNITY_EDITOR_PATH` | Unity Editor installation path |

## References

- [The Honest Report](../../Documentation/tdd/Unity-CSharp-LSP-MCP-Honest-Report.md) - Why custom loader is needed
- [DevRoadmap Phase 2](../../Documentation/tdd/DevRoadmap.md#phase-2-unity-workspace-loader) - Detailed design
- [MSBuildWorkspace docs](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.msbuild.msbuildworkspace)
