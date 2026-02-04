using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace ULSM.Tests.Unit;

/// <summary>
/// Unit tests for UnityPatternAnalyzer class.
/// Tests custom pattern detection for Unity anti-patterns.
/// </summary>
[TestFixture]
[Category("Unit")]
public class UnityPatternAnalyzerTests
{
    private UnityPatternAnalyzer _analyzer = null!;

    [SetUp]
    public void Setup()
    {
        _analyzer = new UnityPatternAnalyzer();
    }

    /// <summary>
    /// Verifies that GetComponent in Update is detected as ULSM0001.
    /// </summary>
    [Test]
    public async Task AnalyzeDocument_GetComponentInUpdate_ReturnsULSM0001()
    {
        // Arrange
        var code = @"
using UnityEngine;
public class TestClass : MonoBehaviour {
    void Update() {
        var comp = GetComponent<Transform>();
    }
}";
        var (document, semanticModel) = await CreateDocumentWithSemanticModelAsync(code);

        // Act
        var issues = await _analyzer.AnalyzeDocumentAsync(document, semanticModel);

        // Assert
        Assert.That(issues, Has.Some.Matches<UnityPatternAnalyzer.PatternIssue>(
            i => i.Id == "ULSM0001" && i.CodeSnippet.Contains("GetComponent")),
            "Should detect GetComponent in Update as ULSM0001");
    }

    /// <summary>
    /// Verifies that Camera.main in Update is detected as ULSM0004.
    /// </summary>
    [Test]
    public async Task AnalyzeDocument_CameraMainInUpdate_ReturnsULSM0004()
    {
        // Arrange
        var code = @"
using UnityEngine;
public class TestClass : MonoBehaviour {
    void Update() {
        var cam = Camera.main;
    }
}";
        var (document, semanticModel) = await CreateDocumentWithSemanticModelAsync(code);

        // Act
        var issues = await _analyzer.AnalyzeDocumentAsync(document, semanticModel);

        // Assert
        Assert.That(issues, Has.Some.Matches<UnityPatternAnalyzer.PatternIssue>(
            i => i.Id == "ULSM0004" && i.Severity == "Warning"),
            "Should detect Camera.main in Update as ULSM0004 Warning");
    }

    /// <summary>
    /// Verifies that string interpolation in Update is detected as ULSM0002.
    /// </summary>
    [Test]
    public async Task AnalyzeDocument_StringInterpolationInUpdate_ReturnsULSM0002()
    {
        // Arrange
        var code = @"
using UnityEngine;
public class TestClass : MonoBehaviour {
    void Update() {
        var msg = $""Time: {Time.deltaTime}"";
    }
}";
        var (document, semanticModel) = await CreateDocumentWithSemanticModelAsync(code);

        // Act
        var issues = await _analyzer.AnalyzeDocumentAsync(document, semanticModel);

        // Assert
        Assert.That(issues, Has.Some.Matches<UnityPatternAnalyzer.PatternIssue>(
            i => i.Id == "ULSM0002"),
            "Should detect string interpolation in Update as ULSM0002");
    }

    /// <summary>
    /// Verifies that Debug.Log in Update is detected as ULSM0003.
    /// </summary>
    [Test]
    public async Task AnalyzeDocument_DebugLogInUpdate_ReturnsULSM0003()
    {
        // Arrange
        var code = @"
using UnityEngine;
public class TestClass : MonoBehaviour {
    void Update() {
        Debug.Log(""test"");
    }
}";
        var (document, semanticModel) = await CreateDocumentWithSemanticModelAsync(code);

        // Act
        var issues = await _analyzer.AnalyzeDocumentAsync(document, semanticModel);

        // Assert
        Assert.That(issues, Has.Some.Matches<UnityPatternAnalyzer.PatternIssue>(
            i => i.Id == "ULSM0003"),
            "Should detect Debug.Log in Update as ULSM0003");
    }

    /// <summary>
    /// Verifies that patterns in non-hot-path methods are not flagged as warnings.
    /// </summary>
    [Test]
    public async Task AnalyzeDocument_GetComponentInStart_NotFlagged()
    {
        // Arrange
        var code = @"
using UnityEngine;
public class TestClass : MonoBehaviour {
    void Start() {
        var comp = GetComponent<Transform>();
    }
}";
        var (document, semanticModel) = await CreateDocumentWithSemanticModelAsync(code);

        // Act
        var issues = await _analyzer.AnalyzeDocumentAsync(document, semanticModel);

        // Assert
        var getComponentIssues = issues.Where(i =>
            i.Id == "ULSM0001" && i.CodeSnippet.Contains("GetComponent"));
        Assert.That(getComponentIssues, Is.Empty,
            "Should not flag GetComponent in Start as it's not a hot path");
    }

    /// <summary>
    /// Verifies that non-MonoBehaviour classes are not analyzed.
    /// </summary>
    [Test]
    public async Task AnalyzeDocument_NonMonoBehaviour_ReturnsNoIssues()
    {
        // Arrange
        var code = @"
public class TestClass {
    void Update() {
        // This Update is not a Unity message
    }
}";
        var (document, semanticModel) = await CreateDocumentWithSemanticModelAsync(code);

        // Act
        var issues = await _analyzer.AnalyzeDocumentAsync(document, semanticModel);

        // Assert
        Assert.That(issues, Is.Empty,
            "Should not analyze non-MonoBehaviour classes");
    }

    /// <summary>
    /// Verifies that Camera.main in non-hot-path returns Info severity.
    /// </summary>
    [Test]
    public async Task AnalyzeDocument_CameraMainInAwake_ReturnsInfoSeverity()
    {
        // Arrange
        var code = @"
using UnityEngine;
public class TestClass : MonoBehaviour {
    void Awake() {
        var cam = Camera.main;
    }
}";
        var (document, semanticModel) = await CreateDocumentWithSemanticModelAsync(code);

        // Act
        var issues = await _analyzer.AnalyzeDocumentAsync(document, semanticModel);

        // Assert
        var cameraIssues = issues.Where(i => i.Id == "ULSM0004").ToList();
        Assert.That(cameraIssues, Has.Count.EqualTo(1),
            "Should detect Camera.main in Awake");
        Assert.That(cameraIssues[0].Severity, Is.EqualTo("Info"),
            "Camera.main in Awake should be Info severity, not Warning");
    }

    /// <summary>
    /// Verifies PatternIssue record has all expected properties.
    /// </summary>
    [Test]
    public void PatternIssue_HasExpectedProperties()
    {
        // Arrange & Act
        var issue = new UnityPatternAnalyzer.PatternIssue(
            Id: "TEST001",
            Title: "Test Title",
            Description: "Test Description",
            Severity: "Warning",
            Category: "Test",
            FilePath: "test.cs",
            Line: 10,
            Column: 5,
            CodeSnippet: "var x = 1;",
            Suggestion: "Do something else"
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(issue.Id, Is.EqualTo("TEST001"));
            Assert.That(issue.Title, Is.EqualTo("Test Title"));
            Assert.That(issue.Description, Is.EqualTo("Test Description"));
            Assert.That(issue.Severity, Is.EqualTo("Warning"));
            Assert.That(issue.Category, Is.EqualTo("Test"));
            Assert.That(issue.FilePath, Is.EqualTo("test.cs"));
            Assert.That(issue.Line, Is.EqualTo(10));
            Assert.That(issue.Column, Is.EqualTo(5));
            Assert.That(issue.CodeSnippet, Is.EqualTo("var x = 1;"));
            Assert.That(issue.Suggestion, Is.EqualTo("Do something else"));
        });
    }

    /// <summary>
    /// Helper method to create a document with semantic model for testing.
    /// </summary>
    private static async Task<(Document document, SemanticModel semanticModel)> CreateDocumentWithSemanticModelAsync(string code)
    {
        // Add Unity stub types
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

        // Add Unity stubs
        var stubDoc = project.AddDocument("UnityStubs.cs", SourceText.From(unityStubs));
        project = stubDoc.Project;

        // Add test code
        var document = project.AddDocument("TestCode.cs", SourceText.From(code));
        var semanticModel = await document.GetSemanticModelAsync();

        return (document, semanticModel!);
    }
}
