# PP13-103 Implementation Checklist

Quick reference checklist for Phase 1 implementation.

## Pre-Flight
- [ ] Read `Prompts/BasePrompt.md`
- [ ] Read `Documentation/tdd/DevRoadmap.md` (Phase 1)
- [ ] Verify psdd branch is `ULSM-PWS`
- [ ] Verify pskd branch is `ULSM-PWS`
- [ ] `dotnet build RoslynMcp.sln` passes
- [ ] Create dev diary registry entry for PP13-103
- [ ] Log plan entry in dev diary

## Task 1: File Renames
- [ ] `git mv RoslynMcp.sln ULSM.sln`
- [ ] `git mv src/RoslynMcp.csproj src/ULSM.csproj`
- [ ] Verify git tracks as rename

## Task 2: Solution File Updates
- [ ] Update project reference path in `ULSM.sln`
- [ ] Update project name in `ULSM.sln`
- [ ] `dotnet sln ULSM.sln list` shows correct path

## Task 3: Project File Updates
- [ ] Update `<RootNamespace>` to `ULSM`
- [ ] Update `<AssemblyName>` to `ULSM`
- [ ] Update `<ToolCommandName>` to `ulsm`
- [ ] Update `<PackageId>` to `ulsm`
- [ ] Update `<Authors>` to `Prespective`
- [ ] Update `<Description>` for Unity focus
- [ ] Update `<PackageTags>` with Unity tags
- [ ] `dotnet restore src/ULSM.csproj` passes
- [ ] `dotnet build src/ULSM.csproj` passes

## Task 4: Namespace Renames
- [ ] `src/Program.cs`: `using ULSM;`
- [ ] `src/McpServer.cs`: `namespace ULSM;`
- [ ] `src/McpServer.cs`: Server name = "ULSM - Unity Language Server MCP"
- [ ] `src/RoslynService.cs`: `namespace ULSM;`

## Task 5: Tool Prefix Renames
- [ ] Find: `"roslyn:` Replace: `"ulsm:` in `McpServer.cs`
- [ ] Verify ~46 replacements (23 tools × 2 locations)
- [ ] All tools in `HandleListToolsAsync` use `ulsm:` prefix
- [ ] All cases in `HandleToolCallAsync` switch use `ulsm:` prefix

## Task 6: Server.json Updates
- [ ] Update `name` to `ulsm`
- [ ] Update `displayName` to `ULSM - Unity Language Server MCP`
- [ ] Update `description` for Unity focus
- [ ] Update tool prefix to `ulsm`
- [ ] Update environment variables section

## Task 7: Environment Variable Renames
- [ ] `ROSLYN_MAX_DIAGNOSTICS` → `ULSM_MAX_DIAGNOSTICS`
- [ ] `ROSLYN_TIMEOUT_SECONDS` → `ULSM_TIMEOUT_SECONDS`
- [ ] `ROSLYN_ENABLE_SEMANTIC_CACHE` → `ULSM_ENABLE_SEMANTIC_CACHE`
- [ ] `ROSLYN_LOG_LEVEL` → `ULSM_LOG_LEVEL`

## Task 8: Log Message Updates
- [ ] Startup message: "ULSM (Unity Language Server MCP) starting..."
- [ ] Warning prefix: "[ULSM Warning]"

## Task 9: Health Check Updates
- [ ] Not ready message references `ulsm:load_solution`
- [ ] Ready message: "ULSM (Unity Language Server MCP) is operational"

## Validation
- [ ] `dotnet clean ULSM.sln`
- [ ] `dotnet restore ULSM.sln`
- [ ] `dotnet build ULSM.sln` - no errors
- [ ] `dotnet run --project src/ULSM.csproj` - starts successfully
- [ ] MCP `initialize` returns ULSM server info
- [ ] MCP `tools/list` shows `ulsm:` prefix on all tools
- [ ] MCP `ulsm:health_check` returns ULSM-branded response

## Git Commits
- [ ] Commit 1: File renames
- [ ] Commit 2: Namespace and branding updates

## Dev Diary Completion
- [ ] Log final work entry
- [ ] Check for learnings to offload
- [ ] Commit dev diary (`mcp__psdd__DoltCommit`)

## Grep Verification
```bash
# Should find only expected Roslyn references:
grep -ri "roslyn" src/ --include="*.cs" --include="*.json"
# Expected: Microsoft.CodeAnalysis.*, RoslynService class, technology comments
```
