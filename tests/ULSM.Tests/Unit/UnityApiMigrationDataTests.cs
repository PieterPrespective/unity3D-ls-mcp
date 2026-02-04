namespace ULSM.Tests.Unit;

/// <summary>
/// Unit tests for UnityApiMigrationData class.
/// Tests the API deprecation database and version filtering.
/// </summary>
[TestFixture]
[Category("Unit")]
public class UnityApiMigrationDataTests
{
    /// <summary>
    /// Verifies that GetAllMigrations returns a non-empty list.
    /// </summary>
    [Test]
    public void GetAllMigrations_ReturnsNonEmptyList()
    {
        // Act
        var migrations = UnityApiMigrationData.GetAllMigrations();

        // Assert
        Assert.That(migrations, Is.Not.Null.And.Not.Empty,
            "Should return a list of API migrations");
        Assert.That(migrations.Count, Is.GreaterThan(10),
            "Should have substantial number of migration rules");
    }

    /// <summary>
    /// Verifies that each migration has required fields populated.
    /// </summary>
    [Test]
    public void GetAllMigrations_AllEntriesHaveRequiredFields()
    {
        // Act
        var migrations = UnityApiMigrationData.GetAllMigrations();

        // Assert
        foreach (var migration in migrations)
        {
            Assert.Multiple(() =>
            {
                Assert.That(migration.OldApi, Is.Not.Null.And.Not.Empty,
                    $"Migration should have OldApi");
                Assert.That(migration.NewApi, Is.Not.Null.And.Not.Empty,
                    $"Migration {migration.OldApi} should have NewApi");
                Assert.That(migration.Category, Is.Not.Null.And.Not.Empty,
                    $"Migration {migration.OldApi} should have Category");
                Assert.That(migration.MinVersion, Is.Not.Null.And.Not.Empty,
                    $"Migration {migration.OldApi} should have MinVersion");
            });
        }
    }

    /// <summary>
    /// Verifies that GetMigrationsForVersion filters correctly for Unity 6.
    /// </summary>
    [Test]
    public void GetMigrationsForVersion_Unity6_ReturnsRelevantMigrations()
    {
        // Act
        var migrations = UnityApiMigrationData.GetMigrationsForVersion("6000.0").ToList();

        // Assert
        Assert.That(migrations, Is.Not.Empty,
            "Should return migrations for Unity 6");
        Assert.That(migrations, Has.Some.Matches<UnityApiMigrationData.ApiMigration>(
            m => m.Category == "Networking"),
            "Should include Networking (UNet) deprecations for Unity 6");
    }

    /// <summary>
    /// Verifies that older version filters return fewer migrations.
    /// </summary>
    [Test]
    public void GetMigrationsForVersion_OlderVersion_ReturnsFewerMigrations()
    {
        // Arrange
        var unity6Migrations = UnityApiMigrationData.GetMigrationsForVersion("6000.0").ToList();
        var unity2019Migrations = UnityApiMigrationData.GetMigrationsForVersion("2019.4").ToList();

        // Assert
        Assert.That(unity2019Migrations.Count, Is.LessThanOrEqualTo(unity6Migrations.Count),
            "Older Unity version should have fewer or equal migrations");
    }

    /// <summary>
    /// Verifies that SearchByOldApi finds relevant migrations.
    /// </summary>
    [Test]
    public void SearchByOldApi_WithInputPattern_FindsInputMigrations()
    {
        // Act
        var inputMigrations = UnityApiMigrationData.SearchByOldApi("Input").ToList();

        // Assert
        Assert.That(inputMigrations, Is.Not.Empty,
            "Should find migrations related to Input");
        Assert.That(inputMigrations, Has.All.Matches<UnityApiMigrationData.ApiMigration>(
            m => m.OldApi.Contains("Input", StringComparison.OrdinalIgnoreCase)),
            "All results should contain 'Input' in OldApi");
    }

    /// <summary>
    /// Verifies that GetCategories returns distinct categories.
    /// </summary>
    [Test]
    public void GetCategories_ReturnsDistinctCategories()
    {
        // Act
        var categories = UnityApiMigrationData.GetCategories().ToList();

        // Assert
        Assert.That(categories, Is.Not.Empty,
            "Should return migration categories");
        Assert.That(categories, Is.Unique,
            "Categories should be distinct");
        Assert.That(categories, Does.Contain("Input"),
            "Should include Input category");
        Assert.That(categories, Does.Contain("Networking"),
            "Should include Networking category");
    }

    /// <summary>
    /// Verifies that version parsing handles various formats.
    /// </summary>
    [TestCase("6000.0", ExpectedResult = true)]
    [TestCase("6000.0.0", ExpectedResult = true)]
    [TestCase("2022.3", ExpectedResult = true)]
    [TestCase("2022.3.1f1", ExpectedResult = true)]
    [TestCase("2019.4.0", ExpectedResult = true)]
    public bool GetMigrationsForVersion_VariousFormats_ParsesCorrectly(string version)
    {
        // Act
        var migrations = UnityApiMigrationData.GetMigrationsForVersion(version);

        // Assert - Should not throw and should return something
        return migrations.Any();
    }

    /// <summary>
    /// Verifies that GetByCategory returns only migrations in the specified category.
    /// </summary>
    [Test]
    public void GetByCategory_Input_ReturnsOnlyInputMigrations()
    {
        // Act
        var inputMigrations = UnityApiMigrationData.GetByCategory("Input").ToList();

        // Assert
        Assert.That(inputMigrations, Is.Not.Empty,
            "Should return Input migrations");
        Assert.That(inputMigrations, Has.All.Matches<UnityApiMigrationData.ApiMigration>(
            m => m.Category.Equals("Input", StringComparison.OrdinalIgnoreCase)),
            "All migrations should be in Input category");
    }

    /// <summary>
    /// Verifies that ApiMigration record has expected properties.
    /// </summary>
    [Test]
    public void ApiMigration_HasExpectedProperties()
    {
        // Arrange & Act
        var migration = new UnityApiMigrationData.ApiMigration(
            OldApi: "OldMethod",
            NewApi: "NewMethod",
            MinVersion: "2022.3",
            RemovedVersion: "6000.0",
            Category: "Test",
            Notes: "Test notes"
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(migration.OldApi, Is.EqualTo("OldMethod"));
            Assert.That(migration.NewApi, Is.EqualTo("NewMethod"));
            Assert.That(migration.MinVersion, Is.EqualTo("2022.3"));
            Assert.That(migration.RemovedVersion, Is.EqualTo("6000.0"));
            Assert.That(migration.Category, Is.EqualTo("Test"));
            Assert.That(migration.Notes, Is.EqualTo("Test notes"));
        });
    }

    /// <summary>
    /// Verifies that category search is case-insensitive.
    /// </summary>
    [Test]
    public void GetByCategory_CaseInsensitive_ReturnsResults()
    {
        // Act
        var lowerCase = UnityApiMigrationData.GetByCategory("input").ToList();
        var upperCase = UnityApiMigrationData.GetByCategory("INPUT").ToList();
        var mixedCase = UnityApiMigrationData.GetByCategory("Input").ToList();

        // Assert
        Assert.That(lowerCase.Count, Is.EqualTo(upperCase.Count),
            "Category search should be case-insensitive");
        Assert.That(lowerCase.Count, Is.EqualTo(mixedCase.Count),
            "Category search should be case-insensitive");
    }
}
