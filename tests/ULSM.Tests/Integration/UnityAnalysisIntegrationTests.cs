using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace ULSM.Tests.Integration;

/// <summary>
/// Integration tests for Unity-specific analysis tools.
/// Tests the full pipeline from loading to analysis.
/// </summary>
[TestFixture]
[Category("Integration")]
public class UnityAnalysisIntegrationTests
{
    /// <summary>
    /// Verifies that GetAllMigrations returns substantial data.
    /// </summary>
    [Test]
    public void UnityApiMigrationData_GetAllMigrations_ReturnsSubstantialData()
    {
        // Act
        var migrations = UnityApiMigrationData.GetAllMigrations();

        // Assert
        Assert.That(migrations.Count, Is.GreaterThanOrEqualTo(20),
            "Should have at least 20 API migration entries");
    }

    /// <summary>
    /// Verifies that migration data covers major Unity systems.
    /// </summary>
    [Test]
    public void UnityApiMigrationData_CoversMajorSystems()
    {
        // Act
        var categories = UnityApiMigrationData.GetCategories().ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(categories, Does.Contain("Input"), "Should cover Input system");
            Assert.That(categories, Does.Contain("Networking"), "Should cover Networking");
            Assert.That(categories, Does.Contain("Rendering"), "Should cover Rendering");
            Assert.That(categories, Does.Contain("XR"), "Should cover XR");
            Assert.That(categories, Does.Contain("UI"), "Should cover UI");
        });
    }

    /// <summary>
    /// Verifies that AdhocWorkspace can be built from test project.
    /// </summary>
    [Test]
    public async Task BuildFromSolutionAsync_UnityTestProject_CreatesWorkspace()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var solutionPath = TestPaths.UnityTestSolutionPath;

        // Act
        var (workspace, solution) = await UnityAdhocWorkspaceBuilder.BuildFromSolutionAsync(solutionPath);

        try
        {
            // Assert
            Assert.That(workspace, Is.Not.Null, "Should create workspace");
            Assert.That(solution, Is.Not.Null, "Should create solution");
            Assert.That(solution.ProjectIds.Count, Is.GreaterThanOrEqualTo(1),
                "Should have at least one project");
        }
        finally
        {
            workspace.Dispose();
        }
    }

    /// <summary>
    /// Verifies that pattern analyzer can analyze code with anti-patterns.
    /// </summary>
    [Test]
    public async Task UnityPatternAnalyzer_AnalyzeAntiPatterns_DetectsIssues()
    {
        // Arrange
        var analyzer = new UnityPatternAnalyzer();
        var code = @"
using UnityEngine;
public class BadCode : MonoBehaviour {
    void Update() {
        var rb = GetComponent<Transform>();
        var cam = Camera.main;
        Debug.Log(""Frame"");
        var msg = $""Value: {Time.deltaTime}"";
    }
}";
        var (document, semanticModel) = await CreateDocumentWithSemanticModelAsync(code);

        // Act
        var issues = await analyzer.AnalyzeDocumentAsync(document, semanticModel);

        // Assert
        Assert.That(issues, Is.Not.Empty, "Should detect anti-patterns");
        var distinctIssueTypes = issues.Select(i => i.Id).Distinct().ToList();
        Assert.That(distinctIssueTypes, Has.Count.GreaterThanOrEqualTo(3),
            "Should detect multiple different issue types");
    }

    /// <summary>
    /// Verifies migration data can filter by Unity 6 version.
    /// </summary>
    [Test]
    public void GetMigrationsForVersion_Unity6_IncludesRemovedApis()
    {
        // Act
        var migrations = UnityApiMigrationData.GetMigrationsForVersion("6000.0").ToList();
        var removedInUnity6 = migrations.Where(m => m.RemovedVersion == "6000.0").ToList();

        // Assert
        Assert.That(removedInUnity6, Is.Not.Empty,
            "Should include APIs removed in Unity 6");
        Assert.That(removedInUnity6, Has.Some.Matches<UnityApiMigrationData.ApiMigration>(
            m => m.Category == "Networking"),
            "Should include removed networking APIs (UNet)");
    }

    /// <summary>
    /// Verifies that workspace loader can create workspace with Unity properties.
    /// </summary>
    [Test]
    public void UnityWorkspaceLoader_CreateWorkspace_Succeeds()
    {
        // Act
        using var workspace = UnityWorkspaceLoader.CreateWorkspace();

        // Assert
        Assert.That(workspace, Is.Not.Null, "Should create workspace");
    }

    /// <summary>
    /// Verifies full analysis pipeline on test code.
    /// </summary>
    [Test]
    public async Task FullAnalysisPipeline_TestCode_CompletesSuccessfully()
    {
        // Arrange
        var analyzer = new UnityPatternAnalyzer();
        var testCode = @"
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Camera _mainCamera;

    void Awake()
    {
        // Good: caching in Awake
        _mainCamera = Camera.main;
    }

    void Update()
    {
        // Bad: should use cached reference
        var cam = Camera.main;
    }
}";
        var (document, semanticModel) = await CreateDocumentWithSemanticModelAsync(testCode);

        // Act
        var issues = await analyzer.AnalyzeDocumentAsync(document, semanticModel);

        // Assert
        // Should find Camera.main in Update (Warning) but not flag Awake one as Warning
        var updateIssues = issues.Where(i => i.Severity == "Warning").ToList();
        var infoIssues = issues.Where(i => i.Severity == "Info").ToList();

        Assert.That(updateIssues, Has.Count.EqualTo(1),
            "Should have exactly one Warning (Camera.main in Update)");
        Assert.That(infoIssues, Has.Count.EqualTo(1),
            "Should have exactly one Info (Camera.main in Awake)");
    }

    /// <summary>
    /// Helper method to create a document with semantic model for testing.
    /// </summary>
    private static async Task<(Document document, SemanticModel semanticModel)> CreateDocumentWithSemanticModelAsync(string code)
    {
        var unityStubs = @"
namespace UnityEngine {
    public class Object { }
    public class Component : Object { }
    public class Behaviour : Component { }
    public class MonoBehaviour : Behaviour {
        public T GetComponent<T>() where T : Component => default!;
        public GameObject gameObject => null!;
        public Transform transform => null!;
    }
    public class GameObject : Object { }
    public class Transform : Component { }
    public class Camera : Behaviour {
        public static Camera main => null!;
    }
    public static class Time { public static float deltaTime => 0f; }
    public static class Debug {
        public static void Log(object msg) { }
        public static void LogWarning(object msg) { }
        public static void LogError(object msg) { }
    }
}
";
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var projectInfo = ProjectInfo.Create(
            projectId,
            VersionStamp.Default,
            "TestProject",
            "TestProject",
            LanguageNames.CSharp,
            parseOptions: new CSharpParseOptions(LanguageVersion.CSharp11));

        var project = workspace.AddProject(projectInfo);
        var stubDoc = project.AddDocument("UnityStubs.cs", SourceText.From(unityStubs));
        project = stubDoc.Project;
        var document = project.AddDocument("TestCode.cs", SourceText.From(code));
        var semanticModel = await document.GetSemanticModelAsync();

        return (document, semanticModel!);
    }
}
