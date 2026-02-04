using Microsoft.Build.Locator;
using ULSM;

// Register MSBuild before any Roslyn code runs
RegisterMSBuild();

// Create and run the MCP server
var server = new McpServer();
await server.RunAsync();

/// <summary>
/// Registers MSBuild with improved detection for various installation scenarios.
/// Handles both Visual Studio and .NET SDK installations.
/// </summary>
static void RegisterMSBuild()
{
    if (MSBuildLocator.IsRegistered)
        return;

    var instances = MSBuildLocator.QueryVisualStudioInstances().ToList();

    if (instances.Count > 0)
    {
        // Prefer .NET SDK instances over Visual Studio
        var sdkInstance = instances
            .Where(i => i.DiscoveryType == DiscoveryType.DotNetSdk)
            .OrderByDescending(i => i.Version)
            .FirstOrDefault();

        var selectedInstance = sdkInstance ?? instances
            .OrderByDescending(i => i.Version)
            .First();

        Console.Error.WriteLine($"[ULSM] Using MSBuild from: {selectedInstance.Name} {selectedInstance.Version}");
        MSBuildLocator.RegisterInstance(selectedInstance);
    }
    else
    {
        // Try to find .NET SDK manually
        var dotnetPath = FindDotNetSdk();
        if (dotnetPath != null)
        {
            Console.Error.WriteLine($"[ULSM] Using MSBuild from SDK at: {dotnetPath}");
            MSBuildLocator.RegisterMSBuildPath(dotnetPath);
        }
        else
        {
            throw new InvalidOperationException(
                "No MSBuild instance found. Please ensure .NET SDK or Visual Studio is installed.\n" +
                "- Install .NET SDK: https://dotnet.microsoft.com/download\n" +
                "- Or install Visual Studio with .NET workload");
        }
    }
}

/// <summary>
/// Attempts to find the .NET SDK MSBuild path manually.
/// </summary>
static string? FindDotNetSdk()
{
    // Common SDK locations
    var possiblePaths = new List<string>();

    if (OperatingSystem.IsWindows())
    {
        possiblePaths.Add(@"C:\Program Files\dotnet\sdk");
        possiblePaths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "sdk"));
    }
    else if (OperatingSystem.IsMacOS())
    {
        possiblePaths.Add("/usr/local/share/dotnet/sdk");
        possiblePaths.Add("/opt/homebrew/opt/dotnet/libexec/sdk");
    }
    else // Linux
    {
        possiblePaths.Add("/usr/share/dotnet/sdk");
        possiblePaths.Add("/usr/lib/dotnet/sdk");
    }

    foreach (var basePath in possiblePaths)
    {
        if (!Directory.Exists(basePath))
            continue;

        // Find the latest SDK version
        var sdkVersions = Directory.GetDirectories(basePath)
            .Select(d => new DirectoryInfo(d))
            .Where(d => File.Exists(Path.Combine(d.FullName, "MSBuild.dll")))
            .OrderByDescending(d => d.Name)
            .FirstOrDefault();

        if (sdkVersions != null)
        {
            return sdkVersions.FullName;
        }
    }

    return null;
}
