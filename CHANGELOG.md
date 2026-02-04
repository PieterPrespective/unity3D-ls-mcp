# Changelog

All notable changes to ULSM are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-04

### Added

#### Unity Project Support
- **UnityProjectDetector**: Automatic detection of Unity projects by:
  - Presence of `Assembly-CSharp.csproj` or `Assembly-CSharp-Editor.csproj`
  - HintPath references to `UnityEngine.dll` or `UnityEditor.dll`
  - Unity folder structure (`Assets/`, `ProjectSettings/`)
  - `ProjectVersion.txt` parsing for Unity version extraction

- **UnityWorkspaceLoader**: MSBuildWorkspace configuration for Unity's legacy csproj format:
  - `ToolsVersion="4.0"` compatibility
  - Cross-platform .NET Framework 4.7.1 reference assembly resolution
  - Environment variable overrides (`ULSM_FRAMEWORK_PATH`, `UNITY_EDITOR_PATH`)

- **UnityAdhocWorkspaceBuilder**: Fallback workspace loader that parses csproj files directly when MSBuild fails

#### Unity Analysis Tools
- **`ulsm:unity_diagnostics`**: Run Microsoft.Unity.Analyzers on Unity projects
  - 50+ Unity-specific diagnostic rules (UNT0001-UNT0xxx)
  - Category filtering (Performance, Correctness, etc.)
  - File or project-level analysis

- **`ulsm:check_unity_patterns`**: Custom pattern analyzer for Unity anti-patterns:
  - ULSM0001: Expensive calls in hot paths (GetComponent in Update)
  - ULSM0002: String operations in hot paths
  - ULSM0003: Debug.Log in hot paths
  - ULSM0004: Camera.main in hot paths

- **`ulsm:api_migration`**: Unity API deprecation checker:
  - 30+ migration rules for Unity 6.x
  - Categories: Input, Networking, Rendering, XR, Physics
  - Version-filtered results
  - Fix suggestions

- **`ulsm:list_unity_rules`**: List all available Unity diagnostic rules

#### Testing Infrastructure
- NUnit 4.x test project with 56 tests
- Minimal Unity test project structure for integration testing
- Unit tests for all Unity-specific components
- Integration tests with graceful skip when Unity DLLs unavailable

### Changed

#### Rebranding
- Renamed from `RoslynMcp` to `ULSM` (Unity Language Server MCP)
- Namespace changed from `RoslynMcp` to `ULSM`
- Tool prefix changed from `roslyn:` to `ulsm:`
- Package ID changed from `dotnet-roslyn-mcp` to `ulsm`
- Environment variables renamed from `ROSLYN_*` to `ULSM_*`

#### Health Check Enhancements
- Added `isUnityProject` field to health check response
- Added `unityVersion` field when Unity project detected
- Added `unityAnalyzersLoaded` status

#### Project Structure
- Added `src/Unity/` folder for Unity-specific code
- Added `src/Unity/Analyzers/` for analyzer infrastructure
- Added `tests/` folder with ULSM.Tests and UnityTestProject

### Technical Details

#### Dependencies Added
- `Microsoft.Unity.Analyzers` v1.19.0

#### New Files
```
src/Unity/
+-- UnityProjectDetector.cs
+-- UnityWorkspaceLoader.cs
+-- UnityAdhocWorkspaceBuilder.cs
+-- UnityAnalysisService.cs
+-- Analyzers/
    +-- UnityAnalyzerLoader.cs
    +-- UnityPatternAnalyzer.cs
    +-- UnityApiMigrationData.cs

tests/
+-- ULSM.Tests/
|   +-- ULSM.Tests.csproj
|   +-- GlobalUsings.cs
|   +-- TestHelpers/
|   |   +-- TestPaths.cs
|   +-- Unit/
|   |   +-- UnityProjectDetectorTests.cs
|   |   +-- UnityWorkspaceLoaderTests.cs
|   |   +-- UnityAnalyzerLoaderTests.cs
|   |   +-- UnityPatternAnalyzerTests.cs
|   |   +-- UnityApiMigrationDataTests.cs
|   +-- Integration/
|       +-- UnityProjectIntegrationTests.cs
|       +-- UnityAnalysisIntegrationTests.cs
+-- UnityTestProject/
    +-- Assembly-CSharp.csproj
    +-- UnityTestProject.sln
    +-- Assets/Scripts/
    +-- ProjectSettings/
```

## Attribution

ULSM is a fork of [dotnet-roslyn-mcp](https://github.com/brendankowitz/dotnet-roslyn-mcp) (MIT License) by Brendan Kowitz.

Unity-specific enhancements by Prespective.


<a name="1.0.1"></a>
## [1.0.1](https://www.github.com/PieterPrespective/unity3D-ls-mcp/releases/tag/v1.0.1) (2026-02-04)

### Bug Fixes

* improve MSBuild detection for packaged tool installation ([bd79800](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/bd798002d07ec2997c6484755ef8841f5803db1b))

<a name="1.0.0"></a>
## [1.0.0](https://www.github.com/PieterPrespective/unity3D-ls-mcp/releases/tag/v1.0.0) (2026-02-04)

### Features

* **unity:** add Microsoft.Unity.Analyzers package ([f98991c](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/f98991cee1f44f452254dd8f8e2bad540e6faa2a))
* **unity:** add Unity analyzer infrastructure ([620ad0f](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/620ad0fa925d35bba7419059d7b738fa44f4252b))
* **unity:** add Unity project detection ([5a092bf](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/5a092bf8ad59735e060f00210e1e130701c567f5))
* **unity:** add Unity workspace loaders ([1c9687f](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/1c9687f2a7a09403cd8faf4a90d4812e6df74c24))
* **unity:** add UnityAnalysisService ([df7a2f1](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/df7a2f1bbf7a4756192a934cd7472a5a20ed6be5))
* **unity:** integrate Unity workspace loading into RoslynService ([fa2a4e7](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/fa2a4e7ee022e79a536990bc40d6bdf07f31f66b))
* **unity:** register Unity analysis MCP tools ([944e618](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/944e618b43ae90570a470d6db809af4afecd2a9a))

### Documentation

* add CHANGELOG documenting fork changes ([ddbdc25](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/ddbdc2568c21755c6bc582fba98036c85189f931))
* add CONTRIBUTING guide and User Guide ([0d2bc0f](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/0d2bc0fc93874868dae3be4644b560e33f392360))
* add PP13-104 Phase 2 assignment prompt ([078ccbb](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/078ccbba02566c73bb169b33b314b6405ae24470))
* add PP13-105 Phase 3 assignment prompt ([eb38deb](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/eb38deb6f262eefec840a89c27ff482c140f771f))
* add PP13-106 Phase 4 assignment prompt ([2e5d2fe](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/2e5d2feaee96d8a4c34f626039bcb3961e4a28e6))
* add PP13-107 Phase 5 assignment prompt ([4464b37](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/4464b3777e910f0ae1145fb0680735e3c377c9e6))
* add ULSM development roadmap and Phase 1 assignment prompt ([9c11b5a](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/9c11b5a7ade1ccbd44f982f011659a55a2354efa))
* clarify path resolution for DOTNET_SOLUTION_PATH ([0939ef0](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/0939ef059e63c39f4038a5a969ec0f8182ff0041))
* rewrite README for ULSM ([394e9d4](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/394e9d4332afd83929a926baf61ecb6f98543b25))
* update LICENSE with dual attribution ([db27f47](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/db27f47636757b2ec88a75607ec029fa4f25fa67))

### Tests

* **unity:** add testing infrastructure with 56 tests ([b438729](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/b438729719b42440b5397141ae9fa2e0649e2a4b))

### Maintenance

* setup project for agent supported development ([8cd5b8a](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/8cd5b8a3e753d161d730b6692cfe009d16fc770b))
* setup versionize and update GitHub workflows ([caeccd4](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/caeccd47aec9a8728de822cbbf741297e0c4bdad))
* update NuGet package metadata for release ([b064bd7](https://www.github.com/PieterPrespective/unity3D-ls-mcp/commit/b064bd72be755c082c002b6b0f22de9205787968))

