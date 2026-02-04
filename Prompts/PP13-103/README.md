# PP13-103: Phase 1 - ULSM Project Restructuring

## Quick Start

```
Please read and execute the prompt in 'Prompts/PP13-103/PP13-103.md'
```

## Contents

| File | Description |
|------|-------------|
| [PP13-103.md](./PP13-103.md) | **Main assignment prompt** - Complete implementation instructions |
| [CHECKLIST.md](./CHECKLIST.md) | Quick reference checklist for tracking progress |
| [TOOL_MAPPING.md](./TOOL_MAPPING.md) | Tool name mapping reference (roslyn: → ulsm:) |

## Context

- **Parent Issue:** PP13-102 (ULSM Development)
- **Phase:** 1 of 5
- **Roadmap:** [Documentation/tdd/DevRoadmap.md](../../Documentation/tdd/DevRoadmap.md)
- **Base Prompt:** [Prompts/BasePrompt.md](../BasePrompt.md)

## Objective

Transform the forked `dotnet-roslyn-mcp` project into `ULSM` (Unity Language Server MCP) by:

1. Renaming files (`RoslynMcp.*` → `ULSM.*`)
2. Updating namespaces (`RoslynMcp` → `ULSM`)
3. Changing tool prefixes (`roslyn:*` → `ulsm:*`)
4. Updating environment variables (`ROSLYN_*` → `ULSM_*`)
5. Applying ULSM branding throughout

## Estimated Effort

~1 day (as per DevRoadmap.md)

## Prerequisites

- Dev diary branch: `ULSM-PWS`
- Knowledge agent branch: `ULSM-PWS`
- Current build passes: `dotnet build RoslynMcp.sln`

## Success Criteria

- `dotnet build ULSM.sln` passes
- Server identifies as "ULSM - Unity Language Server MCP"
- All 23 tools use `ulsm:` prefix
- No remaining `roslyn:` tool references
