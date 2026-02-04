using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace ULSM.Unity;

/// <summary>
/// Creates MSBuildWorkspace instances configured for Unity project loading.
/// Handles Unity's legacy ToolsVersion 4.0 .csproj format and platform-specific framework paths.
/// </summary>
public static class UnityWorkspaceLoader
{
    /// <summary>
    /// Creates an MSBuildWorkspace configured for Unity project loading.
    /// Sets up MSBuild properties to handle Unity's legacy project format.
    /// </summary>
    /// <param name="workspaceFailed">Optional callback for workspace failure events.</param>
    /// <returns>Configured MSBuildWorkspace instance.</returns>
    public static MSBuildWorkspace CreateWorkspace(Action<WorkspaceDiagnosticEventArgs>? workspaceFailed = null)
    {
        var properties = GetUnityMSBuildProperties();
        var workspace = MSBuildWorkspace.Create(properties);

        workspace.SkipUnrecognizedProjects = true;

        if (workspaceFailed != null)
        {
            workspace.WorkspaceFailed += (sender, args) => workspaceFailed(args);
        }

        return workspace;
    }

    /// <summary>
    /// Gets the MSBuild properties required for Unity project loading.
    /// These properties suppress warnings and configure framework targeting for Unity's legacy format.
    /// </summary>
    /// <returns>Dictionary of MSBuild property name-value pairs.</returns>
    public static Dictionary<string, string> GetUnityMSBuildProperties()
    {
        var frameworkPath = FindFrameworkPath();

        var properties = new Dictionary<string, string>
        {
            // Suppress ToolsVersion warnings for legacy format
            ["MSBuildToolsVersion"] = "Current",

            // Prevent MSBuild from trying to restore packages
            ["RestorePackages"] = "false",

            // Skip targets that may fail outside Unity
            ["SkipCopyBuildProduct"] = "true",
            ["SkipCopyFilesToOutputDirectory"] = "true",
        };

        // Add framework path if found
        if (!string.IsNullOrEmpty(frameworkPath))
        {
            properties["FrameworkPathOverride"] = frameworkPath;
        }

        // Check for environment variable override
        var envFrameworkPath = Environment.GetEnvironmentVariable("ULSM_FRAMEWORK_PATH");
        if (!string.IsNullOrEmpty(envFrameworkPath))
        {
            properties["FrameworkPathOverride"] = envFrameworkPath;
        }

        return properties;
    }

    /// <summary>
    /// Finds the appropriate .NET Framework reference assemblies path for the current platform.
    /// Searches common installation locations and returns the first valid path found.
    /// Unity targets .NET Framework 4.7.1, so we look for that or compatible versions.
    /// </summary>
    /// <returns>Path to framework reference assemblies, or empty string if not found.</returns>
    public static string FindFrameworkPath()
    {
        // Check environment variable first
        var envPath = Environment.GetEnvironmentVariable("ULSM_FRAMEWORK_PATH");
        if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
            return envPath;

        var searchPaths = GetPlatformFrameworkPaths();

        foreach (var path in searchPaths)
        {
            if (Directory.Exists(path))
            {
                // Verify it contains expected assemblies
                var mscorlibPath = Path.Combine(path, "mscorlib.dll");
                if (File.Exists(mscorlibPath))
                    return path;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the list of framework paths to search based on the current platform.
    /// Returns paths in order of preference (most specific/recent first).
    /// </summary>
    /// <returns>Array of potential framework paths to search.</returns>
    private static string[] GetPlatformFrameworkPaths()
    {
        if (OperatingSystem.IsWindows())
        {
            return new[]
            {
                @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.1",
                @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8",
                @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.1",
                @"C:\Windows\Microsoft.NET\Framework\v4.0.30319",
            };
        }
        else if (OperatingSystem.IsMacOS())
        {
            return new[]
            {
                "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.7.1-api",
                "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.8-api",
                "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.5",
            };
        }
        else // Linux
        {
            return new[]
            {
                "/usr/lib/mono/4.7.1-api",
                "/usr/lib/mono/4.8-api",
                "/usr/lib/mono/4.5",
                "/usr/share/dotnet/packs/Microsoft.NETFramework.ReferenceAssemblies.net471/1.0.0/build/.NETFramework/v4.7.1",
            };
        }
    }

    /// <summary>
    /// Loads a Unity solution with appropriate workspace configuration.
    /// Falls back to AdhocWorkspace if MSBuildWorkspace fails to load the solution.
    /// </summary>
    /// <param name="solutionPath">Path to the Unity .sln file.</param>
    /// <param name="workspaceFailed">Optional callback for workspace failure events.</param>
    /// <returns>Tuple of (Workspace, Solution, usedFallback).</returns>
    public static async Task<(Workspace workspace, Solution solution, bool usedFallback)> LoadSolutionAsync(
        string solutionPath,
        Action<WorkspaceDiagnosticEventArgs>? workspaceFailed = null)
    {
        var forceAdhoc = Environment.GetEnvironmentVariable("ULSM_FORCE_ADHOC") == "true";

        // Try MSBuildWorkspace first (unless forced to use AdhocWorkspace)
        if (!forceAdhoc)
        {
            try
            {
                var workspace = CreateWorkspace(workspaceFailed);
                var solution = await workspace.OpenSolutionAsync(solutionPath);

                // Verify we actually loaded something
                if (solution.ProjectIds.Count > 0)
                {
                    return (workspace, solution, false);
                }

                // MSBuildWorkspace loaded but no projects - dispose and try fallback
                Console.Error.WriteLine("[ULSM Warning] MSBuildWorkspace loaded 0 projects, trying fallback");
                workspace.Dispose();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ULSM Warning] MSBuildWorkspace failed: {ex.Message}");
            }
        }

        // Fallback to AdhocWorkspace
        Console.Error.WriteLine("[ULSM] Falling back to AdhocWorkspace");
        var (adhocWorkspace, adhocSolution) = await UnityAdhocWorkspaceBuilder.BuildFromSolutionAsync(solutionPath);
        return (adhocWorkspace, adhocSolution, true);
    }
}
