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
