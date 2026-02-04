# PP13-105: Phase 3 Checklist

Quick reference checklist for Phase 3 implementation.

## Pre-Flight Checks

- [ ] Dev-diary branch is `ULSM-PWS`
- [ ] Knowledge-agent branch is `ULSM-PWS`
- [ ] PP13-104 commits present (`git log --oneline -5`)
- [ ] `dotnet build ULSM.sln` succeeds
- [ ] Create registry entry in dev diary
- [ ] Log implementation plan in dev diary

## Task 1: NuGet Package

- [ ] Add `Microsoft.Unity.Analyzers v1.19.0` to `src/ULSM.csproj`
- [ ] `dotnet restore ULSM.sln` succeeds
- [ ] `dotnet build ULSM.sln` succeeds

## Task 2: UnityAnalyzerLoader

File: `src/Unity/Analyzers/UnityAnalyzerLoader.cs`

- [ ] Create `src/Unity/Analyzers/` directory
- [ ] Implement `LoadAllAnalyzers()` method
- [ ] Implement `GetAnalyzersByCategory()` method
- [ ] Implement analyzer caching
- [ ] Implement `GetAvailableDiagnostics()` method
- [ ] Graceful handling when assembly not found

## Task 3: UnityPatternAnalyzer

File: `src/Unity/Analyzers/UnityPatternAnalyzer.cs`

- [ ] Implement `AnalyzeDocumentAsync()` method
- [ ] Detect GetComponent in hot paths
- [ ] Detect string concatenation in hot paths
- [ ] Detect Camera.main usage
- [ ] Detect Debug.Log in hot paths
- [ ] Provide actionable suggestions for each pattern

## Task 4: UnityApiMigrationData

File: `src/Unity/Analyzers/UnityApiMigrationData.cs`

- [ ] Define `ApiMigration` record
- [ ] Implement `GetAllMigrations()` method
- [ ] Implement `GetMigrationsForVersion()` method
- [ ] Implement `SearchByOldApi()` method
- [ ] Add migration data for:
  - [ ] Input System
  - [ ] Networking (UNet)
  - [ ] Rendering
  - [ ] UI
  - [ ] XR
  - [ ] Other categories

## Task 5: UnityAnalysisService

File: `src/Unity/UnityAnalysisService.cs`

- [ ] Implement `GetUnityDiagnosticsAsync()` method
- [ ] Implement `CheckUnityPatternsAsync()` method
- [ ] Implement `CheckApiMigrationAsync()` method
- [ ] Implement `GetAvailableDiagnostics()` method
- [ ] Add `GetSolution()` to `RoslynService`

## Task 6: MCP Tool Registration

File: `src/McpServer.cs`

- [ ] Add `using ULSM.Unity;`
- [ ] Add `UnityAnalysisService` field and initialization
- [ ] Add `ulsm:unity_diagnostics` tool definition
- [ ] Add `ulsm:check_unity_patterns` tool definition
- [ ] Add `ulsm:api_migration` tool definition
- [ ] Add `ulsm:list_unity_rules` tool definition
- [ ] Add tool handlers in switch expression

## Task 7: Documentation

File: `src/server.json`

- [ ] Document `ulsm:unity_diagnostics`
- [ ] Document `ulsm:check_unity_patterns`
- [ ] Document `ulsm:api_migration`
- [ ] Document `ulsm:list_unity_rules`

## Build Validation

- [ ] `dotnet clean ULSM.sln`
- [ ] `dotnet restore ULSM.sln`
- [ ] `dotnet build ULSM.sln` - no new errors/warnings

## Git Commits

- [ ] Commit 1: `feat(unity): add Microsoft.Unity.Analyzers package`
- [ ] Commit 2: `feat(unity): add Unity analyzer infrastructure`
- [ ] Commit 3: `feat(unity): add UnityAnalysisService`
- [ ] Commit 4: `feat(unity): register Unity analysis MCP tools`
- [ ] Commit 5: `docs: add PP13-105 Phase 3 assignment prompt`

## Testing (if Unity project available)

- [ ] `ulsm:health_check` returns success
- [ ] `ulsm:list_unity_rules` returns diagnostic list
- [ ] `ulsm:unity_diagnostics` returns Unity analyzer results
- [ ] `ulsm:check_unity_patterns` detects patterns in test file
- [ ] `ulsm:api_migration` finds deprecated API usage

## Dev Diary Completion

- [ ] Log work completion with summary
- [ ] Note any deviations from plan
- [ ] Record lessons learned

## New Files Created

| File | Status |
|------|--------|
| `src/Unity/Analyzers/UnityAnalyzerLoader.cs` | [ ] |
| `src/Unity/Analyzers/UnityPatternAnalyzer.cs` | [ ] |
| `src/Unity/Analyzers/UnityApiMigrationData.cs` | [ ] |
| `src/Unity/UnityAnalysisService.cs` | [ ] |

## Files Modified

| File | Status |
|------|--------|
| `src/ULSM.csproj` | [ ] |
| `src/RoslynService.cs` | [ ] |
| `src/McpServer.cs` | [ ] |
| `src/server.json` | [ ] |

## New MCP Tools

| Tool | Status |
|------|--------|
| `ulsm:unity_diagnostics` | [ ] |
| `ulsm:check_unity_patterns` | [ ] |
| `ulsm:api_migration` | [ ] |
| `ulsm:list_unity_rules` | [ ] |
