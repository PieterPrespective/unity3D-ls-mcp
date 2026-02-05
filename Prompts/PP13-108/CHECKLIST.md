# PP13-108 Quick Checklist

## Pre-Implementation

- [ ] Read BasePrompt.md for project conventions
- [ ] Verify psdd/pskd on work branch (ULSM-PWS)
- [ ] `dotnet build ULSM.sln` succeeds
- [ ] `dotnet test ULSM.sln` passes
- [ ] Create registry entry for PP13-108
- [ ] Log implementation plan

## Implementation

### Task 1: McpServer.cs - Auto-Load Logging
- [ ] Log DOTNET_SOLUTION_PATH value at startup
- [ ] Resolve relative paths and log result
- [ ] Log error when File.Exists returns false
- [ ] Log current working directory for debugging

### Task 2: RoslynService.cs - Health Check Fix
- [ ] Change condition from `_solution == null || _workspace == null` to `_solution == null`
- [ ] Add workspace type to health check response

### Task 3: RoslynService.cs - Store Solution Path
- [ ] Add `_solutionPath` field
- [ ] Store path in LoadSolutionAsync
- [ ] Use stored path in response and health check

### Task 4: Integration Tests
- [ ] Test health check after AdhocWorkspace load
- [ ] Test solutionPath in load response

## Validation

- [ ] `dotnet build ULSM.sln` succeeds
- [ ] `dotnet test ULSM.sln` all tests pass
- [ ] Manual test with relative path
- [ ] Manual test with absolute path
- [ ] Health check returns "Ready" after fallback load

## Git Commits

1. [ ] `fix(health): check only _solution for ready state`
2. [ ] `fix(load): return original solutionPath in response`
3. [ ] `fix(autoload): improve path resolution and error logging`
4. [ ] `test(load): add solution loading integration tests`
5. [ ] `docs: add PP13-108 assignment prompt`

## Dev Diary

- [ ] Create registry entry
- [ ] Log plan entry
- [ ] Log work completion entry
- [ ] Commit psdd database
