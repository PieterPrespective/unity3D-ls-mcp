namespace ULSM.Tests.Unit;

/// <summary>
/// Unit tests for UnityProjectDetector class.
/// Tests the heuristics for identifying Unity projects.
/// </summary>
[TestFixture]
[Category("Unit")]
public class UnityProjectDetectorTests
{
    /// <summary>
    /// Verifies that a solution with Assembly-CSharp.csproj is detected as Unity.
    /// </summary>
    [Test]
    public void IsUnityProject_WithAssemblyCSharpProject_ReturnsTrue()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var solutionPath = TestPaths.UnityTestSolutionPath;

        // Act
        var result = UnityProjectDetector.IsUnityProject(solutionPath);

        // Assert
        Assert.That(result, Is.True,
            "Should detect Unity project by Assembly-CSharp.csproj presence");
    }

    /// <summary>
    /// Verifies that the main ULSM solution is NOT detected as Unity.
    /// </summary>
    [Test]
    public void IsUnityProject_WithStandardDotNetSolution_ReturnsFalse()
    {
        // Arrange
        var solutionPath = TestPaths.MainSolutionPath;

        // Act
        var result = UnityProjectDetector.IsUnityProject(solutionPath);

        // Assert
        Assert.That(result, Is.False,
            "Should not detect standard .NET project as Unity");
    }

    /// <summary>
    /// Verifies that Unity version is extracted from ProjectVersion.txt.
    /// </summary>
    [Test]
    public void GetUnityVersion_WithProjectVersionFile_ReturnsVersion()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var projectPath = TestPaths.UnityTestProjectPath;

        // Act
        var version = UnityProjectDetector.GetUnityVersion(projectPath);

        // Assert
        Assert.That(version, Is.Not.Null.And.Not.Empty,
            "Should extract Unity version from ProjectVersion.txt");
        Assert.That(version, Does.Contain("6000"),
            "Version should contain Unity 6 identifier");
    }

    /// <summary>
    /// Verifies that null is returned when no ProjectVersion.txt exists.
    /// </summary>
    [Test]
    public void GetUnityVersion_WithoutProjectVersionFile_ReturnsNull()
    {
        // Arrange
        var nonUnityPath = Path.GetDirectoryName(TestPaths.MainSolutionPath);

        // Act
        var version = UnityProjectDetector.GetUnityVersion(nonUnityPath!);

        // Assert
        Assert.That(version, Is.Null,
            "Should return null when ProjectVersion.txt is missing");
    }

    /// <summary>
    /// Verifies detection works with directory containing Assets and ProjectSettings.
    /// </summary>
    [Test]
    public void IsUnityProject_WithUnityFolderStructure_ReturnsTrue()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var solutionPath = TestPaths.UnityTestSolutionPath;

        // Act
        var result = UnityProjectDetector.IsUnityProject(solutionPath);

        // Assert
        Assert.That(result, Is.True,
            "Should detect Unity project by folder structure (Assets + ProjectSettings)");
    }

    /// <summary>
    /// Verifies graceful handling of non-existent paths.
    /// </summary>
    [Test]
    public void IsUnityProject_WithNonExistentPath_ReturnsFalse()
    {
        // Arrange
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "Fake.sln");

        // Act
        var result = UnityProjectDetector.IsUnityProject(fakePath);

        // Assert
        Assert.That(result, Is.False,
            "Should return false for non-existent paths without throwing");
    }

    /// <summary>
    /// Verifies that empty or null paths return false.
    /// </summary>
    [Test]
    public void IsUnityProject_WithNullOrEmptyPath_ReturnsFalse()
    {
        // Act & Assert
        Assert.That(UnityProjectDetector.IsUnityProject(null!), Is.False,
            "Should return false for null path");
        Assert.That(UnityProjectDetector.IsUnityProject(string.Empty), Is.False,
            "Should return false for empty path");
    }

    /// <summary>
    /// Verifies that HasUnityReferences detects Unity DLLs in csproj files.
    /// </summary>
    [Test]
    public void HasUnityReferences_WithUnityCsproj_ReturnsTrue()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var csprojPath = TestPaths.UnityAssemblyCSharpPath;

        // Act
        var result = UnityProjectDetector.HasUnityReferences(csprojPath);

        // Assert
        Assert.That(result, Is.True,
            "Should detect Unity references in Assembly-CSharp.csproj");
    }
}
