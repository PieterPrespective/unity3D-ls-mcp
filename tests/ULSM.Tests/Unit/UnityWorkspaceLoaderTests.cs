namespace ULSM.Tests.Unit;

/// <summary>
/// Unit tests for UnityWorkspaceLoader class.
/// Tests MSBuild configuration and framework path resolution.
/// </summary>
[TestFixture]
[Category("Unit")]
public class UnityWorkspaceLoaderTests
{
    private string? _originalFrameworkPath;

    [SetUp]
    public void Setup()
    {
        // Save original environment variable
        _originalFrameworkPath = Environment.GetEnvironmentVariable("ULSM_FRAMEWORK_PATH");
    }

    [TearDown]
    public void TearDown()
    {
        // Restore original environment variable
        Environment.SetEnvironmentVariable("ULSM_FRAMEWORK_PATH", _originalFrameworkPath);
    }

    /// <summary>
    /// Verifies that framework path resolution finds a valid path on Windows.
    /// </summary>
    [Test]
    [Platform("Win")]
    [Category("Platform")]
    public void FindFrameworkPath_OnWindows_ReturnsValidPath()
    {
        // Arrange - clear any override
        Environment.SetEnvironmentVariable("ULSM_FRAMEWORK_PATH", null);

        // Act
        var frameworkPath = UnityWorkspaceLoader.FindFrameworkPath();

        // Assert
        if (string.IsNullOrEmpty(frameworkPath))
        {
            Assert.Ignore(".NET Framework reference assemblies not installed on this system");
        }

        Assert.That(Directory.Exists(frameworkPath), Is.True,
            "Framework path should exist on disk");
        Assert.That(File.Exists(Path.Combine(frameworkPath, "mscorlib.dll")), Is.True,
            "Framework path should contain mscorlib.dll");
    }

    /// <summary>
    /// Verifies that framework path resolution finds a valid path on macOS/Linux.
    /// </summary>
    [Test]
    [Platform("Unix")]
    [Category("Platform")]
    public void FindFrameworkPath_OnUnix_ReturnsValidPathOrEmpty()
    {
        // Arrange - clear any override
        Environment.SetEnvironmentVariable("ULSM_FRAMEWORK_PATH", null);

        // Act
        var frameworkPath = UnityWorkspaceLoader.FindFrameworkPath();

        // Assert - On Unix, Mono may not be installed
        if (string.IsNullOrEmpty(frameworkPath))
        {
            Assert.Pass("Mono framework not installed - acceptable for CI without Unity");
        }

        Assert.That(Directory.Exists(frameworkPath), Is.True,
            "If framework path is returned, it should exist");
    }

    /// <summary>
    /// Verifies that environment variable override for framework path is respected.
    /// </summary>
    [Test]
    public void FindFrameworkPath_WithEnvironmentOverride_UsesOverride()
    {
        // Arrange
        var customPath = Path.Combine(Path.GetTempPath(), $"ULSM_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(customPath);

        try
        {
            Environment.SetEnvironmentVariable("ULSM_FRAMEWORK_PATH", customPath);

            // Act
            var frameworkPath = UnityWorkspaceLoader.FindFrameworkPath();

            // Assert
            Assert.That(frameworkPath, Is.EqualTo(customPath),
                "Should use environment variable override");
        }
        finally
        {
            // Cleanup
            Directory.Delete(customPath);
        }
    }

    /// <summary>
    /// Verifies that GetUnityMSBuildProperties returns required Unity properties.
    /// </summary>
    [Test]
    public void GetUnityMSBuildProperties_ReturnsRequiredProperties()
    {
        // Act
        var properties = UnityWorkspaceLoader.GetUnityMSBuildProperties();

        // Assert
        Assert.That(properties, Does.ContainKey("MSBuildToolsVersion"),
            "Should include MSBuildToolsVersion property");
        Assert.That(properties, Does.ContainKey("RestorePackages"),
            "Should include RestorePackages property");
        Assert.That(properties["RestorePackages"], Is.EqualTo("false"),
            "RestorePackages should be disabled for Unity projects");
        Assert.That(properties, Does.ContainKey("SkipCopyBuildProduct"),
            "Should include SkipCopyBuildProduct property");
        Assert.That(properties, Does.ContainKey("SkipCopyFilesToOutputDirectory"),
            "Should include SkipCopyFilesToOutputDirectory property");
    }

    /// <summary>
    /// Verifies that MSBuildToolsVersion is set to Current.
    /// </summary>
    [Test]
    public void GetUnityMSBuildProperties_MSBuildToolsVersion_IsCurrent()
    {
        // Act
        var properties = UnityWorkspaceLoader.GetUnityMSBuildProperties();

        // Assert
        Assert.That(properties["MSBuildToolsVersion"], Is.EqualTo("Current"),
            "MSBuildToolsVersion should be 'Current' to suppress ToolsVersion warnings");
    }

    /// <summary>
    /// Verifies that CreateWorkspace returns a configured workspace.
    /// </summary>
    [Test]
    public void CreateWorkspace_ReturnsWorkspaceInstance()
    {
        // Act
        using var workspace = UnityWorkspaceLoader.CreateWorkspace();

        // Assert
        Assert.That(workspace, Is.Not.Null,
            "Should create a workspace instance");
        Assert.That(workspace.SkipUnrecognizedProjects, Is.True,
            "Should have SkipUnrecognizedProjects set to true");
    }
}
