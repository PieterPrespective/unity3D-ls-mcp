namespace ULSM.Tests.Integration;

/// <summary>
/// Integration tests that load the actual Unity test project.
/// These tests verify end-to-end functionality of ULSM with Unity projects.
/// </summary>
[TestFixture]
[Category("Integration")]
public class UnityProjectIntegrationTests
{
    /// <summary>
    /// Verifies that the Unity test project is detected as a Unity project.
    /// </summary>
    [Test]
    [Order(1)]
    public void IsUnityProject_UnityTestProject_ReturnsTrue()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var solutionPath = TestPaths.UnityTestSolutionPath;

        // Act
        var isUnity = UnityProjectDetector.IsUnityProject(solutionPath);

        // Assert
        Assert.That(isUnity, Is.True,
            "Unity test project should be detected as Unity project");
    }

    /// <summary>
    /// Verifies that Unity version can be extracted from the test project.
    /// </summary>
    [Test]
    [Order(2)]
    public void GetUnityVersion_UnityTestProject_ReturnsVersion()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var projectPath = TestPaths.UnityTestProjectPath;

        // Act
        var version = UnityProjectDetector.GetUnityVersion(projectPath);

        // Assert
        Assert.That(version, Is.Not.Null.And.Not.Empty,
            "Should extract Unity version");
        Assert.That(version, Is.EqualTo("6000.0.0f1"),
            "Should extract correct version string");
    }

    /// <summary>
    /// Verifies that the csproj can be parsed for source files.
    /// </summary>
    [Test]
    [Order(3)]
    public void ParseCsprojFile_AssemblyCSharp_ReturnsSourceFiles()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var csprojPath = TestPaths.UnityAssemblyCSharpPath;

        // Act
        var (sourceFiles, references, defines) = UnityAdhocWorkspaceBuilder.ParseCsprojFile(csprojPath);

        // Assert
        Assert.That(sourceFiles, Has.Count.EqualTo(3),
            "Should find 3 source files in Assembly-CSharp.csproj");
        Assert.That(sourceFiles, Has.Some.Contains("TestMonoBehaviour.cs"),
            "Should include TestMonoBehaviour.cs");
        Assert.That(sourceFiles, Has.Some.Contains("TestPatterns.cs"),
            "Should include TestPatterns.cs");
    }

    /// <summary>
    /// Verifies that the csproj contains Unity references.
    /// </summary>
    [Test]
    [Order(4)]
    public void ParseCsprojFile_AssemblyCSharp_ReturnsUnityReferences()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var csprojPath = TestPaths.UnityAssemblyCSharpPath;

        // Act
        var (sourceFiles, references, defines) = UnityAdhocWorkspaceBuilder.ParseCsprojFile(csprojPath);

        // Assert
        Assert.That(references, Is.Not.Empty,
            "Should find Unity DLL references");
        Assert.That(references, Has.Some.Contains("UnityEngine"),
            "Should include UnityEngine reference");
    }

    /// <summary>
    /// Verifies that preprocessor defines are extracted from csproj.
    /// </summary>
    [Test]
    [Order(5)]
    public void ParseCsprojFile_AssemblyCSharp_ReturnsDefines()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var csprojPath = TestPaths.UnityAssemblyCSharpPath;

        // Act
        var (sourceFiles, references, defines) = UnityAdhocWorkspaceBuilder.ParseCsprojFile(csprojPath);
        var definesList = defines.ToList();

        // Assert
        Assert.That(definesList, Is.Not.Empty,
            "Should extract preprocessor defines");
        Assert.That(definesList, Has.Some.Contains("UNITY_6000"),
            "Should include UNITY_6000 define");
    }

    /// <summary>
    /// Verifies that main ULSM solution is NOT detected as Unity.
    /// </summary>
    [Test]
    [Order(6)]
    public void IsUnityProject_MainULSMSolution_ReturnsFalse()
    {
        // Arrange
        var solutionPath = TestPaths.MainSolutionPath;

        // Act
        var isUnity = UnityProjectDetector.IsUnityProject(solutionPath);

        // Assert
        Assert.That(isUnity, Is.False,
            "Main ULSM solution should NOT be detected as Unity project");
    }
}
