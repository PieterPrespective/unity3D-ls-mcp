using System.Text.Json;

namespace ULSM.Tests.Integration;

/// <summary>
/// Integration tests for solution loading and health check functionality.
/// Verifies that ULSM correctly handles both MSBuildWorkspace and AdhocWorkspace scenarios,
/// and that the health check and load responses are accurate.
/// </summary>
[TestFixture]
[Category("Integration")]
public class SolutionLoadingTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Converts an anonymous object to a JsonElement for property access.
    /// </summary>
    private static JsonElement ToJson(object obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        return JsonDocument.Parse(json).RootElement;
    }

    /// <summary>
    /// Verifies that LoadSolutionAsync returns the correct solutionPath in the response.
    /// This tests the fix for Issue PP13-108 where AdhocWorkspace solutions returned null FilePath.
    /// </summary>
    [Test]
    public async Task LoadSolution_ReturnsCorrectSolutionPath()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var service = new RoslynService();
        var expectedPath = TestPaths.UnityTestSolutionPath;

        // Act
        var result = ToJson(await service.LoadSolutionAsync(expectedPath));

        // Assert
        Assert.That(result.GetProperty("success").GetBoolean(), Is.True,
            "Load should succeed");
        Assert.That(result.GetProperty("solutionPath").GetString(), Is.EqualTo(expectedPath),
            "solutionPath in response should match the input path");
    }

    /// <summary>
    /// Verifies that the health check returns "Ready" after a successful Unity project load.
    /// This tests the fix for Issue PP13-108 where AdhocWorkspace fallback caused "Not Ready" status.
    /// </summary>
    [Test]
    public async Task HealthCheck_AfterUnityProjectLoad_ReturnsReady()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var service = new RoslynService();
        var solutionPath = TestPaths.UnityTestSolutionPath;

        // Act
        await service.LoadSolutionAsync(solutionPath);
        var health = ToJson(await service.GetHealthCheckAsync());

        // Assert
        Assert.That(health.GetProperty("status").GetString(), Is.EqualTo("Ready"),
            "Health check should return 'Ready' after successful load");
        Assert.That(health.GetProperty("solution").GetProperty("loaded").GetBoolean(), Is.True,
            "Solution should be marked as loaded");
    }

    /// <summary>
    /// Verifies that the health check includes workspace type information.
    /// </summary>
    [Test]
    public async Task HealthCheck_IncludesWorkspaceType()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var service = new RoslynService();
        var solutionPath = TestPaths.UnityTestSolutionPath;

        // Act
        await service.LoadSolutionAsync(solutionPath);
        var health = ToJson(await service.GetHealthCheckAsync());

        // Assert
        var workspaceType = health.GetProperty("workspace").GetProperty("type").GetString();
        Assert.That(workspaceType, Is.Not.Null.And.Not.Empty,
            "Workspace type should be included in health check");
        Assert.That(workspaceType,
            Is.EqualTo("MSBuildWorkspace").Or.EqualTo("AdhocWorkspace"),
            "Workspace type should be MSBuildWorkspace or AdhocWorkspace");
    }

    /// <summary>
    /// Verifies that the health check solution path matches the loaded path.
    /// </summary>
    [Test]
    public async Task HealthCheck_SolutionPath_MatchesLoadedPath()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var service = new RoslynService();
        var expectedPath = TestPaths.UnityTestSolutionPath;

        // Act
        await service.LoadSolutionAsync(expectedPath);
        var health = ToJson(await service.GetHealthCheckAsync());

        // Assert
        Assert.That(health.GetProperty("solution").GetProperty("path").GetString(), Is.EqualTo(expectedPath),
            "Health check solution path should match the loaded path");
    }

    /// <summary>
    /// Verifies that the health check returns "Not Ready" when no solution is loaded.
    /// </summary>
    [Test]
    public async Task HealthCheck_NoSolutionLoaded_ReturnsNotReady()
    {
        // Arrange
        var service = new RoslynService();

        // Act
        var health = ToJson(await service.GetHealthCheckAsync());

        // Assert
        Assert.That(health.GetProperty("status").GetString(), Is.EqualTo("Not Ready"),
            "Health check should return 'Not Ready' when no solution loaded");
        Assert.That(health.GetProperty("solution").ValueKind, Is.EqualTo(JsonValueKind.Null),
            "Solution object should be null when not loaded");
    }

    /// <summary>
    /// Verifies that LoadSolutionAsync reports usedFallback correctly for Unity projects.
    /// </summary>
    [Test]
    public async Task LoadSolution_UnityProject_ReportsUsedFallback()
    {
        // Arrange
        TestPaths.SkipIfUnityTestProjectMissing();
        var service = new RoslynService();
        var solutionPath = TestPaths.UnityTestSolutionPath;

        // Act
        var result = ToJson(await service.LoadSolutionAsync(solutionPath));

        // Assert
        Assert.That(result.GetProperty("success").GetBoolean(), Is.True,
            "Load should succeed");
        Assert.That(result.GetProperty("isUnityProject").GetBoolean(), Is.True,
            "Should detect as Unity project");
        // usedFallback can be true or false depending on MSBuild availability
        Assert.That(result.GetProperty("usedFallback").ValueKind, Is.EqualTo(JsonValueKind.True).Or.EqualTo(JsonValueKind.False),
            "usedFallback should be a boolean");
    }

    /// <summary>
    /// Verifies that loading a non-Unity solution works correctly.
    /// </summary>
    [Test]
    public async Task LoadSolution_NonUnitySolution_LoadsSuccessfully()
    {
        // Arrange
        var service = new RoslynService();
        var solutionPath = TestPaths.MainSolutionPath;

        // Act
        var result = ToJson(await service.LoadSolutionAsync(solutionPath));

        // Assert
        Assert.That(result.GetProperty("success").GetBoolean(), Is.True,
            "Load should succeed for non-Unity solution");
        Assert.That(result.GetProperty("isUnityProject").GetBoolean(), Is.False,
            "Should not detect as Unity project");
        Assert.That(result.GetProperty("solutionPath").GetString(), Is.EqualTo(solutionPath),
            "solutionPath should match input");
    }

    /// <summary>
    /// Verifies health check after loading a non-Unity solution.
    /// </summary>
    [Test]
    public async Task HealthCheck_AfterNonUnityLoad_ReturnsReady()
    {
        // Arrange
        var service = new RoslynService();
        var solutionPath = TestPaths.MainSolutionPath;

        // Act
        await service.LoadSolutionAsync(solutionPath);
        var health = ToJson(await service.GetHealthCheckAsync());

        // Assert
        Assert.That(health.GetProperty("status").GetString(), Is.EqualTo("Ready"),
            "Health check should return 'Ready' for non-Unity solution");
        Assert.That(health.GetProperty("workspace").GetProperty("type").GetString(), Is.EqualTo("MSBuildWorkspace"),
            "Non-Unity solutions should use MSBuildWorkspace");
    }
}
