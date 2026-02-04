# PP13-105: Phase 3 - Unity-Specific Analysis Tools

## Prompt Contents

| File | Description |
|------|-------------|
| [PP13-105.md](./PP13-105.md) | Main assignment prompt (~1000 lines) - comprehensive implementation guide |
| [CHECKLIST.md](./CHECKLIST.md) | Quick reference checklist for implementation tracking |

## Overview

This prompt guides the implementation of Phase 3 of the ULSM roadmap: Unity-Specific Analysis Tools.

### Objective

Integrate Microsoft.Unity.Analyzers and implement custom pattern detection to provide Unity game developers with actionable insights about:
- Unity-specific anti-patterns (null coalescing, tag comparison, etc.)
- Performance issues in hot paths (Update, FixedUpdate)
- Deprecated API usage for Unity 6.x migration

### New MCP Tools

| Tool | Purpose |
|------|---------|
| `ulsm:unity_diagnostics` | Get diagnostics from Microsoft.Unity.Analyzers |
| `ulsm:check_unity_patterns` | Custom pattern detection for performance issues |
| `ulsm:api_migration` | Check for deprecated Unity API usage |
| `ulsm:list_unity_rules` | List all available Unity diagnostic rules |

### New Files

```
src/Unity/Analyzers/
├── UnityAnalyzerLoader.cs      # Loads Microsoft.Unity.Analyzers
├── UnityPatternAnalyzer.cs     # Custom pattern detection
└── UnityApiMigrationData.cs    # Unity 6.x deprecation database

src/Unity/
└── UnityAnalysisService.cs     # Main analysis orchestrator
```

### Dependencies

- **PP13-104** (Phase 2 - Unity Workspace Loader) must be completed
- **Microsoft.Unity.Analyzers** NuGet package (v1.19.0)

### Estimated Commits

1. `feat(unity): add Microsoft.Unity.Analyzers package`
2. `feat(unity): add Unity analyzer infrastructure`
3. `feat(unity): add UnityAnalysisService`
4. `feat(unity): register Unity analysis MCP tools`
5. `docs: add PP13-105 Phase 3 assignment prompt`

## Reference

- [DevRoadmap.md](../../Documentation/tdd/DevRoadmap.md) - Phase 3 specification
- [PP13-104](../PP13-104/) - Phase 2 (prerequisite)
- [Microsoft.Unity.Analyzers](https://github.com/microsoft/Microsoft.Unity.Analyzers) - Unity analyzer source
