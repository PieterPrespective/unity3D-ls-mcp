# PP13-106: Phase 4 - Testing Infrastructure

## Prompt Contents

| File | Description |
|------|-------------|
| [PP13-106.md](./PP13-106.md) | Main assignment prompt (~1900 lines) - comprehensive implementation guide |
| [CHECKLIST.md](./CHECKLIST.md) | Quick reference checklist for implementation tracking |

## Overview

This prompt guides the implementation of Phase 4 of the ULSM roadmap: Testing Infrastructure.

### Objective

Create a comprehensive testing infrastructure for ULSM that validates:
- Unity workspace loading (MSBuild and AdhocWorkspace fallback)
- Unity-specific analysis tools (analyzers, pattern detection, API migration)
- Integration with actual Unity project structures
- Cross-platform compatibility

### New Project Structure

```
tests/
├── ULSM.Tests/                          # NUnit 4.x test project
│   ├── ULSM.Tests.csproj
│   ├── GlobalUsings.cs
│   ├── TestHelpers/
│   │   └── TestPaths.cs                 # Path resolution helper
│   ├── Unit/
│   │   ├── UnityProjectDetectorTests.cs
│   │   ├── UnityWorkspaceLoaderTests.cs
│   │   ├── UnityAnalyzerLoaderTests.cs
│   │   ├── UnityPatternAnalyzerTests.cs
│   │   └── UnityApiMigrationDataTests.cs
│   └── Integration/
│       ├── UnityProjectIntegrationTests.cs
│       └── UnityAnalysisIntegrationTests.cs
│
└── UnityTestProject/                    # Minimal Unity-like structure
    ├── Assets/Scripts/
    │   ├── TestMonoBehaviour.cs
    │   ├── TestScriptableObject.cs
    │   └── TestPatterns.cs              # Intentional anti-patterns
    ├── ProjectSettings/
    │   └── ProjectVersion.txt
    ├── Assembly-CSharp.csproj
    └── UnityTestProject.sln
```

### Test Categories

| Category | Purpose | Test Count |
|----------|---------|------------|
| Unit | Fast, isolated component tests | 26+ |
| Integration | Full project loading tests | 6+ |
| Platform | OS-specific tests (Win/Unix) | 2-4 |

### Dependencies

- **PP13-104** (Phase 2 - Unity Workspace Loader) must be completed
- **PP13-105** (Phase 3 - Unity-Specific Analysis Tools) must be completed
- **NUnit 4.x** testing framework
- **coverlet** for code coverage

### Estimated Commits

1. `feat(tests): create ULSM.Tests project structure`
2. `feat(tests): add minimal Unity test project`
3. `feat(tests): add unit tests for Unity components`
4. `feat(tests): add integration tests for Unity analysis`
5. `feat(tests): integrate test project into solution`
6. `docs: add PP13-106 Phase 4 assignment prompt`

## Test Coverage Goals

### Components Under Test

| Component | Tests | Purpose |
|-----------|-------|---------|
| UnityProjectDetector | 6 | Unity project identification heuristics |
| UnityWorkspaceLoader | 4 | Framework path resolution, MSBuild config |
| UnityAnalyzerLoader | 5 | Analyzer loading, caching, categorization |
| UnityPatternAnalyzer | 6 | Custom pattern detection (ULSM0001-0004) |
| UnityApiMigrationData | 7 | Deprecation database, version filtering |
| Integration | 6 | End-to-end project loading and analysis |

### Test Execution

```bash
# Run all tests
dotnet test ULSM.sln

# Run unit tests only
dotnet test ULSM.sln --filter "Category=Unit"

# Run with coverage
dotnet test ULSM.sln --collect:"XPlat Code Coverage"
```

## Reference

- [DevRoadmap.md](../../Documentation/tdd/DevRoadmap.md) - Phase 4 specification
- [PP13-104](../PP13-104/) - Phase 2 (prerequisite)
- [PP13-105](../PP13-105/) - Phase 3 (prerequisite)
- [NUnit Documentation](https://docs.nunit.org/) - Testing framework
