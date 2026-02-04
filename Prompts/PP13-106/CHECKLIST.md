# PP13-106: Phase 4 Checklist

Quick reference checklist for Phase 4 implementation.

## Pre-Flight Checks

- [ ] Dev-diary branch is `ULSM-PWS`
- [ ] Knowledge-agent branch is `ULSM-PWS`
- [ ] PP13-105 commits present (`git log --oneline -5`)
- [ ] `dotnet build ULSM.sln` succeeds
- [ ] Create registry entry in dev diary
- [ ] Log implementation plan in dev diary

## Task 1: Create Test Project Structure

- [ ] Create `tests/ULSM.Tests/` directory
- [ ] Create `tests/ULSM.Tests/TestHelpers/` directory
- [ ] Create `tests/ULSM.Tests/Unit/` directory
- [ ] Create `tests/ULSM.Tests/Integration/` directory
- [ ] Create `tests/UnityTestProject/` directory
- [ ] Create `tests/UnityTestProject/Assets/Scripts/` directory
- [ ] Create `tests/UnityTestProject/ProjectSettings/` directory

## Task 2: Create ULSM.Tests Project

File: `tests/ULSM.Tests/ULSM.Tests.csproj`

- [ ] Target `net8.0`
- [ ] Add NUnit 4.2.2 package reference
- [ ] Add NUnit3TestAdapter 4.6.0 package reference
- [ ] Add Microsoft.NET.Test.Sdk 17.11.1 package reference
- [ ] Add NUnit.Analyzers 4.3.0 package reference
- [ ] Add coverlet.collector 6.0.2 package reference
- [ ] Add project reference to `src/ULSM.csproj`
- [ ] Include UnityTestProject files as content

## Task 3: Create Global Usings

File: `tests/ULSM.Tests/GlobalUsings.cs`

- [ ] Add `global using NUnit.Framework;`
- [ ] Add `global using ULSM;`
- [ ] Add `global using ULSM.Unity;`
- [ ] Add `global using ULSM.Unity.Analyzers;`

## Task 4: Create Test Helpers

File: `tests/ULSM.Tests/TestHelpers/TestPaths.cs`

- [ ] Implement `TestProjectRoot` property
- [ ] Implement `UnityTestSolutionPath` property
- [ ] Implement `UnityTestProjectPath` property
- [ ] Implement `MainSolutionPath` property
- [ ] Implement `GetUnityScriptPath()` method
- [ ] Implement `UnityAssemblyCSharpPath` property
- [ ] Implement `UnityTestProjectExists` property
- [ ] Implement `SkipIfUnityTestProjectMissing()` method

## Task 5: Create Unity Test Project

### 5.1 Solution File

File: `tests/UnityTestProject/UnityTestProject.sln`

- [ ] Visual Studio Solution format
- [ ] Reference Assembly-CSharp project
- [ ] Debug and Release configurations

### 5.2 Project File

File: `tests/UnityTestProject/Assembly-CSharp.csproj`

- [ ] ToolsVersion 4.0 (Unity legacy format)
- [ ] TargetFrameworkVersion v4.7.1
- [ ] UNITY_6000 define constants
- [ ] Unity DLL references with HintPaths
- [ ] Compile includes for test scripts

### 5.3 Project Version

File: `tests/UnityTestProject/ProjectSettings/ProjectVersion.txt`

- [ ] `m_EditorVersion: 6000.0.0f1`
- [ ] `m_EditorVersionWithRevision` entry

### 5.4 Test Scripts

File: `tests/UnityTestProject/Assets/Scripts/TestMonoBehaviour.cs`

- [ ] Simulated Unity types (when not in Unity)
- [ ] Valid MonoBehaviour class
- [ ] SerializeField attributes
- [ ] Awake/Start/Update methods
- [ ] Public methods for testing references

File: `tests/UnityTestProject/Assets/Scripts/TestScriptableObject.cs`

- [ ] ScriptableObject implementation
- [ ] SerializeField properties

File: `tests/UnityTestProject/Assets/Scripts/TestPatterns.cs`

- [ ] UNT0002 tag comparison anti-pattern
- [ ] UNT0010 MonoBehaviour with new()
- [ ] ULSM0001 GetComponent in Update
- [ ] ULSM0002 string operation in Update
- [ ] ULSM0003 Debug.Log in Update
- [ ] ULSM0004 Camera.main in Update
- [ ] UNT0001 empty Unity message (LateUpdate)

## Task 6: Create Unit Tests

### 6.1 UnityProjectDetectorTests

File: `tests/ULSM.Tests/Unit/UnityProjectDetectorTests.cs`

- [ ] `IsUnityProject_WithAssemblyCSharpProject_ReturnsTrue`
- [ ] `IsUnityProject_WithStandardDotNetSolution_ReturnsFalse`
- [ ] `GetUnityVersion_WithProjectVersionFile_ReturnsVersion`
- [ ] `GetUnityVersion_WithoutProjectVersionFile_ReturnsNull`
- [ ] `IsUnityProject_WithUnityFolderStructure_ReturnsTrue`
- [ ] `IsUnityProject_WithNonExistentPath_ReturnsFalse`

### 6.2 UnityWorkspaceLoaderTests

File: `tests/ULSM.Tests/Unit/UnityWorkspaceLoaderTests.cs`

- [ ] `GetFrameworkPath_OnWindows_ReturnsValidPath` (Platform: Win)
- [ ] `GetFrameworkPath_OnUnix_ReturnsValidPathOrEmpty` (Platform: Unix)
- [ ] `GetFrameworkPath_WithEnvironmentOverride_UsesOverride`
- [ ] `GetMSBuildProperties_ReturnsRequiredProperties`

### 6.3 UnityAnalyzerLoaderTests

File: `tests/ULSM.Tests/Unit/UnityAnalyzerLoaderTests.cs`

- [ ] `LoadAllAnalyzers_WhenPackageAvailable_ReturnsAnalyzers`
- [ ] `LoadAllAnalyzers_CalledTwice_ReturnsCachedResult`
- [ ] `ClearCache_AfterLoad_AllowsReload`
- [ ] `GetAnalyzersByCategory_All_ReturnsAllAnalyzers`
- [ ] `GetAvailableDiagnostics_ReturnsFormattedDiagnosticInfo`

### 6.4 UnityPatternAnalyzerTests

File: `tests/ULSM.Tests/Unit/UnityPatternAnalyzerTests.cs`

- [ ] `AnalyzeDocument_GetComponentInUpdate_ReturnsULSM0001`
- [ ] `AnalyzeDocument_CameraMainInUpdate_ReturnsULSM0004`
- [ ] `AnalyzeDocument_StringInterpolationInUpdate_ReturnsULSM0002`
- [ ] `AnalyzeDocument_DebugLogInUpdate_ReturnsULSM0003`
- [ ] `AnalyzeDocument_GetComponentInStart_NotWarning`
- [ ] `AnalyzeDocument_NonMonoBehaviour_ReturnsNoIssues`

### 6.5 UnityApiMigrationDataTests

File: `tests/ULSM.Tests/Unit/UnityApiMigrationDataTests.cs`

- [ ] `GetAllMigrations_ReturnsNonEmptyList`
- [ ] `GetAllMigrations_AllEntriesHaveRequiredFields`
- [ ] `GetMigrationsForVersion_Unity6_ReturnsRelevantMigrations`
- [ ] `GetMigrationsForVersion_OlderVersion_ReturnsFewerMigrations`
- [ ] `SearchByOldApi_WithInputPattern_FindsInputMigrations`
- [ ] `GetCategories_ReturnsDistinctCategories`
- [ ] `GetMigrationsForVersion_VariousFormats_ParsesCorrectly` (TestCase)

## Task 7: Create Integration Tests

### 7.1 UnityProjectIntegrationTests

File: `tests/ULSM.Tests/Integration/UnityProjectIntegrationTests.cs`

- [ ] `LoadSolution_UnityTestProject_LoadsSuccessfully`
- [ ] `GetHealthCheck_UnityProject_ReportsUnityStatus`
- [ ] `LoadSolution_UnityTestProject_HasExpectedProjectCount`

### 7.2 UnityAnalysisIntegrationTests

File: `tests/ULSM.Tests/Integration/UnityAnalysisIntegrationTests.cs`

- [ ] `GetAvailableDiagnostics_ReturnsAnalyzerInfo`
- [ ] `CheckApiMigration_Unity6Target_ReturnsResults`
- [ ] `CheckUnityPatterns_TestPatternsFile_FindsIssues`

## Task 8: Update Solution File

File: `ULSM.sln`

- [ ] Add ULSM.Tests project reference
- [ ] Add "tests" solution folder
- [ ] Configure Debug|Any CPU for test project
- [ ] Configure Release|Any CPU for test project
- [ ] Nest test project in tests folder

## Task 9: API Additions (if needed)

File: `src/Unity/UnityWorkspaceLoader.cs`

- [ ] Make `GetFrameworkPath()` public
- [ ] Add `GetMSBuildProperties()` public method

File: `src/Unity/Analyzers/UnityAnalyzerLoader.cs`

- [ ] Add `ClearCache()` method (for test isolation)

## Build Validation

- [ ] `dotnet clean ULSM.sln`
- [ ] `dotnet restore ULSM.sln`
- [ ] `dotnet build ULSM.sln` - no new errors/warnings
- [ ] `dotnet test ULSM.sln` - runs without errors
- [ ] `dotnet test ULSM.sln --filter "Category=Unit"` - all pass
- [ ] `dotnet test ULSM.sln --filter "Category=Integration"` - pass or skip

## Git Commits

- [ ] Commit 1: `feat(tests): create ULSM.Tests project structure`
- [ ] Commit 2: `feat(tests): add minimal Unity test project`
- [ ] Commit 3: `feat(tests): add unit tests for Unity components`
- [ ] Commit 4: `feat(tests): add integration tests for Unity analysis`
- [ ] Commit 5: `feat(tests): integrate test project into solution`
- [ ] Commit 6: `docs: add PP13-106 Phase 4 assignment prompt`

## Dev Diary Completion

- [ ] Log work completion with summary
- [ ] Note any deviations from plan
- [ ] Record lessons learned

## New Files Created

| File | Status |
|------|--------|
| `tests/ULSM.Tests/ULSM.Tests.csproj` | [ ] |
| `tests/ULSM.Tests/GlobalUsings.cs` | [ ] |
| `tests/ULSM.Tests/TestHelpers/TestPaths.cs` | [ ] |
| `tests/ULSM.Tests/Unit/UnityProjectDetectorTests.cs` | [ ] |
| `tests/ULSM.Tests/Unit/UnityWorkspaceLoaderTests.cs` | [ ] |
| `tests/ULSM.Tests/Unit/UnityAnalyzerLoaderTests.cs` | [ ] |
| `tests/ULSM.Tests/Unit/UnityPatternAnalyzerTests.cs` | [ ] |
| `tests/ULSM.Tests/Unit/UnityApiMigrationDataTests.cs` | [ ] |
| `tests/ULSM.Tests/Integration/UnityProjectIntegrationTests.cs` | [ ] |
| `tests/ULSM.Tests/Integration/UnityAnalysisIntegrationTests.cs` | [ ] |
| `tests/UnityTestProject/UnityTestProject.sln` | [ ] |
| `tests/UnityTestProject/Assembly-CSharp.csproj` | [ ] |
| `tests/UnityTestProject/ProjectSettings/ProjectVersion.txt` | [ ] |
| `tests/UnityTestProject/Assets/Scripts/TestMonoBehaviour.cs` | [ ] |
| `tests/UnityTestProject/Assets/Scripts/TestScriptableObject.cs` | [ ] |
| `tests/UnityTestProject/Assets/Scripts/TestPatterns.cs` | [ ] |

## Files Modified

| File | Status |
|------|--------|
| `ULSM.sln` | [ ] |
| `src/Unity/UnityWorkspaceLoader.cs` (maybe) | [ ] |
| `src/Unity/Analyzers/UnityAnalyzerLoader.cs` (maybe) | [ ] |

## Test Summary

| Category | Test Count | Target |
|----------|------------|--------|
| Unit | 26+ | All pass |
| Integration | 6+ | Pass or skip |
| Platform | 2-4 | Platform-specific |

## Test Execution Commands

```bash
# Run all tests
dotnet test ULSM.sln

# Run unit tests only
dotnet test ULSM.sln --filter "Category=Unit"

# Run integration tests only
dotnet test ULSM.sln --filter "Category=Integration"

# Run with coverage
dotnet test ULSM.sln --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test ULSM.sln --filter "FullyQualifiedName~UnityProjectDetectorTests"
```
