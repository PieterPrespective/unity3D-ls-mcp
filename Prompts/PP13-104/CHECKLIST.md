# PP13-104 Implementation Checklist

Quick reference checklist for Phase 2 implementation.

## Pre-Flight

- [ ] Read `Prompts/BasePrompt.md`
- [ ] Read `Documentation/tdd/DevRoadmap.md` (Phase 2)
- [ ] Read `Documentation/tdd/Unity-CSharp-LSP-MCP-Honest-Report.md`
- [ ] Verify psdd branch is `ULSM-PWS`
- [ ] Verify pskd branch is `ULSM-PWS`
- [ ] Verify PP13-103 commits exist (`git log --oneline -3`)
- [ ] `dotnet build ULSM.sln` passes
- [ ] Create dev diary registry entry for PP13-104
- [ ] Log plan entry in dev diary

## Task 1: Create Folder Structure

- [ ] Create `src/Unity/` folder

## Task 2: UnityProjectDetector

- [ ] Create `src/Unity/UnityProjectDetector.cs`
- [ ] Implement `IsUnityProject(solutionPath)`
- [ ] Implement `HasUnityFolderStructure(directory)`
- [ ] Implement `HasAssemblyCSharpProject(directory)`
- [ ] Implement `HasUnityReferencesInAnyProject(directory)`
- [ ] Implement `HasUnityReferences(csprojPath)`
- [ ] Implement `GetUnityVersion(solutionDir)`
- [ ] Add XML documentation to all public members

## Task 3: UnityWorkspaceLoader

- [ ] Create `src/Unity/UnityWorkspaceLoader.cs`
- [ ] Implement `CreateWorkspace(workspaceFailed)`
- [ ] Implement `GetUnityMSBuildProperties()`
- [ ] Implement `FindFrameworkPath()` with platform support
- [ ] Implement `GetPlatformFrameworkPaths()` for Win/Mac/Linux
- [ ] Implement `LoadSolutionAsync(solutionPath, workspaceFailed)`
- [ ] Support `ULSM_FRAMEWORK_PATH` environment variable
- [ ] Add XML documentation to all public members

## Task 4: UnityAdhocWorkspaceBuilder

- [ ] Create `src/Unity/UnityAdhocWorkspaceBuilder.cs`
- [ ] Implement `BuildFromSolutionAsync(solutionPath)`
- [ ] Implement `ParseSolutionProjects(solutionPath)`
- [ ] Implement `AddProjectToWorkspaceAsync(...)`
- [ ] Implement `ParseCsprojFile(csprojPath)`
- [ ] Extract source files from `<Compile>` elements
- [ ] Extract references from `<HintPath>` elements
- [ ] Extract defines from `<DefineConstants>`
- [ ] Add XML documentation to all public members

## Task 5: Integration into RoslynService

- [ ] Add `using ULSM.Unity;` to imports
- [ ] Modify `LoadSolutionAsync` to detect Unity projects
- [ ] Call `UnityWorkspaceLoader.LoadSolutionAsync` for Unity
- [ ] Handle AdhocWorkspace fallback
- [ ] Return `isUnityProject` and `unityVersion` in response
- [ ] Return `usedFallback` indicator

## Task 6: Update Health Check

- [ ] Add `isUnityProject` to health check response
- [ ] Add `unityVersion` to health check response
- [ ] Add `frameworkPath` to workspace info

## Task 7: Environment Variables

- [ ] Add `ULSM_FRAMEWORK_PATH` to server.json
- [ ] Add `ULSM_FORCE_ADHOC` to server.json
- [ ] Add `UNITY_EDITOR_PATH` to server.json

## Build Validation

- [ ] `dotnet clean ULSM.sln`
- [ ] `dotnet restore ULSM.sln`
- [ ] `dotnet build ULSM.sln` - no errors
- [ ] No new warnings related to Unity code

## Manual Testing (if Unity project available)

- [ ] Set `DOTNET_SOLUTION_PATH` to Unity solution
- [ ] Run ULSM
- [ ] MCP `initialize` succeeds
- [ ] MCP `ulsm:health_check` shows `isUnityProject: true`
- [ ] Unity version detected (if ProjectVersion.txt exists)

## Git Commits

- [ ] Commit 1: Unity detection (`UnityProjectDetector.cs`)
- [ ] Commit 2: Workspace loaders (`UnityWorkspaceLoader.cs`, `UnityAdhocWorkspaceBuilder.cs`)
- [ ] Commit 3: Integration (`RoslynService.cs`, `server.json`)

## Dev Diary Completion

- [ ] Log final work entry
- [ ] Check for learnings to offload
- [ ] Commit dev diary (`mcp__psdd__DoltCommit`)

## Code Quality Checks

- [ ] All public classes have XML summary comments
- [ ] All public methods have XML documentation
- [ ] All parameters have descriptions
- [ ] Static utility pattern followed (no instance state in utility classes)
- [ ] Consistent error handling with `[ULSM Warning]` prefix

## Platform Support Verification

| Platform | Framework Path Found | Notes |
|----------|---------------------|-------|
| Windows | [ ] | Check .NET Framework Targeting Pack |
| macOS | [ ] | Check Mono installation |
| Linux | [ ] | Check Mono or dotnet targeting packs |
