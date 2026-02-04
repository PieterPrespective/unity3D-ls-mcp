namespace ULSM.Tests.TestHelpers;

/// <summary>
/// Provides consistent paths to test resources across all test classes.
/// Handles the complexity of finding test files relative to the test execution directory.
/// </summary>
public static class TestPaths
{
    private static string? _testProjectRoot;

    /// <summary>
    /// Gets the root directory of the test project.
    /// </summary>
    public static string TestProjectRoot
    {
        get
        {
            if (_testProjectRoot != null)
                return _testProjectRoot;

            // Start from test execution directory and navigate up to find tests folder
            var currentDir = TestContext.CurrentContext.TestDirectory;

            // Navigate up until we find the tests folder or ULSM.sln
            while (currentDir != null)
            {
                var testsDir = Path.Combine(currentDir, "tests");
                var slnFile = Path.Combine(currentDir, "ULSM.sln");

                if (Directory.Exists(testsDir) || File.Exists(slnFile))
                {
                    _testProjectRoot = currentDir;
                    return _testProjectRoot;
                }

                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            // Fallback: use test directory
            _testProjectRoot = TestContext.CurrentContext.TestDirectory;
            return _testProjectRoot;
        }
    }

    /// <summary>
    /// Gets the path to the Unity test project solution file.
    /// </summary>
    public static string UnityTestSolutionPath =>
        Path.Combine(TestProjectRoot, "tests", "UnityTestProject", "UnityTestProject.sln");

    /// <summary>
    /// Gets the path to the Unity test project folder.
    /// </summary>
    public static string UnityTestProjectPath =>
        Path.Combine(TestProjectRoot, "tests", "UnityTestProject");

    /// <summary>
    /// Gets the path to the main ULSM solution file.
    /// </summary>
    public static string MainSolutionPath =>
        Path.Combine(TestProjectRoot, "ULSM.sln");

    /// <summary>
    /// Gets the path to a specific test script in the Unity test project.
    /// </summary>
    /// <param name="scriptName">Name of the script file (e.g., "TestPatterns.cs").</param>
    /// <returns>Full path to the script file.</returns>
    public static string GetUnityScriptPath(string scriptName) =>
        Path.Combine(UnityTestProjectPath, "Assets", "Scripts", scriptName);

    /// <summary>
    /// Gets the path to the Assembly-CSharp.csproj in the Unity test project.
    /// </summary>
    public static string UnityAssemblyCSharpPath =>
        Path.Combine(UnityTestProjectPath, "Assembly-CSharp.csproj");

    /// <summary>
    /// Checks if the Unity test project exists and is properly set up.
    /// </summary>
    public static bool UnityTestProjectExists =>
        File.Exists(UnityTestSolutionPath) &&
        File.Exists(UnityAssemblyCSharpPath) &&
        Directory.Exists(Path.Combine(UnityTestProjectPath, "Assets"));

    /// <summary>
    /// Skips the current test if the Unity test project is not available.
    /// </summary>
    public static void SkipIfUnityTestProjectMissing()
    {
        if (!UnityTestProjectExists)
        {
            Assert.Ignore("Unity test project not found. Skipping integration test.");
        }
    }

    /// <summary>
    /// Resets the cached test project root. Useful for testing TestPaths itself.
    /// </summary>
    internal static void ResetCache()
    {
        _testProjectRoot = null;
    }
}
