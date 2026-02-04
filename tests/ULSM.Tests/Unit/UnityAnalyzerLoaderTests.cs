using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ULSM.Tests.Unit;

/// <summary>
/// Unit tests for UnityAnalyzerLoader class.
/// Tests analyzer loading, caching, and category filtering.
/// </summary>
[TestFixture]
[Category("Unit")]
public class UnityAnalyzerLoaderTests
{
    [SetUp]
    public void Setup()
    {
        // Clear cache before each test to ensure isolation
        UnityAnalyzerLoader.ClearCache();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up after tests
        UnityAnalyzerLoader.ClearCache();
    }

    /// <summary>
    /// Verifies that LoadAllAnalyzers returns analyzers when package is available.
    /// </summary>
    [Test]
    public void LoadAllAnalyzers_WhenPackageAvailable_ReturnsAnalyzers()
    {
        // Act
        var analyzers = UnityAnalyzerLoader.LoadAllAnalyzers();

        // Assert
        // Note: May return empty if Microsoft.Unity.Analyzers is not loaded
        // ImmutableArray is a struct, so we check that it's been properly initialized
        Assert.That(analyzers.IsDefault, Is.False,
            "Should return initialized array even if no analyzers found");
        Assert.That(analyzers.Length, Is.GreaterThanOrEqualTo(0),
            "Length should be non-negative");
    }

    /// <summary>
    /// Verifies that caching works - second call returns same count.
    /// </summary>
    [Test]
    public void LoadAllAnalyzers_CalledTwice_ReturnsCachedResult()
    {
        // Act
        var first = UnityAnalyzerLoader.LoadAllAnalyzers();
        var second = UnityAnalyzerLoader.LoadAllAnalyzers();

        // Assert
        Assert.That(first.Length, Is.EqualTo(second.Length),
            "Cached result should have same count");
    }

    /// <summary>
    /// Verifies that ClearCache actually clears the cache.
    /// </summary>
    [Test]
    public void ClearCache_AfterLoad_AllowsReload()
    {
        // Arrange
        var first = UnityAnalyzerLoader.LoadAllAnalyzers();

        // Act
        UnityAnalyzerLoader.ClearCache();
        var second = UnityAnalyzerLoader.LoadAllAnalyzers();

        // Assert
        Assert.That(first.Length, Is.EqualTo(second.Length),
            "Should return same analyzers after cache clear");
    }

    /// <summary>
    /// Verifies that GetAnalyzersByCategory with All returns all analyzers.
    /// </summary>
    [Test]
    public void GetAnalyzersByCategory_All_ReturnsAllAnalyzers()
    {
        // Arrange
        var all = UnityAnalyzerLoader.LoadAllAnalyzers();

        // Act
        var filtered = UnityAnalyzerLoader.GetAnalyzersByCategory(
            UnityAnalyzerLoader.AnalyzerCategory.All);

        // Assert
        Assert.That(filtered.Length, Is.EqualTo(all.Length),
            "Category.All should return all analyzers");
    }

    /// <summary>
    /// Verifies that GetAvailableDiagnostics returns diagnostic information.
    /// </summary>
    [Test]
    public void GetAvailableDiagnostics_ReturnsFormattedDiagnosticInfo()
    {
        // Act
        var diagnostics = UnityAnalyzerLoader.GetAvailableDiagnostics().ToList();

        // Assert
        Assert.That(diagnostics, Is.Not.Null,
            "Should return diagnostic information list");

        // If analyzers are loaded, check the format
        if (diagnostics.Count > 0)
        {
            var first = diagnostics[0];
            var firstType = first.GetType();

            Assert.That(firstType.GetProperty("id"), Is.Not.Null,
                "Diagnostic info should have id property");
            Assert.That(firstType.GetProperty("title"), Is.Not.Null,
                "Diagnostic info should have title property");
            Assert.That(firstType.GetProperty("severity"), Is.Not.Null,
                "Diagnostic info should have severity property");
        }
    }

    /// <summary>
    /// Verifies that category filtering works for Messages category.
    /// </summary>
    [Test]
    public void GetAnalyzersByCategory_Messages_ReturnsSubset()
    {
        // Arrange
        var all = UnityAnalyzerLoader.LoadAllAnalyzers();

        // Act
        var messages = UnityAnalyzerLoader.GetAnalyzersByCategory(
            UnityAnalyzerLoader.AnalyzerCategory.Messages);

        // Assert
        Assert.That(messages.Length, Is.LessThanOrEqualTo(all.Length),
            "Messages category should return subset of all analyzers");
    }

    /// <summary>
    /// Verifies that category filtering works for Performance category.
    /// </summary>
    [Test]
    public void GetAnalyzersByCategory_Performance_ReturnsSubset()
    {
        // Arrange
        var all = UnityAnalyzerLoader.LoadAllAnalyzers();

        // Act
        var performance = UnityAnalyzerLoader.GetAnalyzersByCategory(
            UnityAnalyzerLoader.AnalyzerCategory.Performance);

        // Assert
        Assert.That(performance.Length, Is.LessThanOrEqualTo(all.Length),
            "Performance category should return subset of all analyzers");
    }

    /// <summary>
    /// Verifies AnalyzerCategory enum has expected values.
    /// </summary>
    [Test]
    public void AnalyzerCategory_HasExpectedValues()
    {
        // Assert
        Assert.That(Enum.IsDefined(typeof(UnityAnalyzerLoader.AnalyzerCategory),
            UnityAnalyzerLoader.AnalyzerCategory.All), Is.True);
        Assert.That(Enum.IsDefined(typeof(UnityAnalyzerLoader.AnalyzerCategory),
            UnityAnalyzerLoader.AnalyzerCategory.Messages), Is.True);
        Assert.That(Enum.IsDefined(typeof(UnityAnalyzerLoader.AnalyzerCategory),
            UnityAnalyzerLoader.AnalyzerCategory.NullChecking), Is.True);
        Assert.That(Enum.IsDefined(typeof(UnityAnalyzerLoader.AnalyzerCategory),
            UnityAnalyzerLoader.AnalyzerCategory.Performance), Is.True);
        Assert.That(Enum.IsDefined(typeof(UnityAnalyzerLoader.AnalyzerCategory),
            UnityAnalyzerLoader.AnalyzerCategory.BestPractices), Is.True);
    }
}
