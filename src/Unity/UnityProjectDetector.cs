using System.Xml.Linq;

namespace ULSM.Unity;

/// <summary>
/// Detects whether a solution or project is a Unity-generated project.
/// Uses multiple heuristics including project naming, reference patterns, and folder structure.
/// </summary>
public static class UnityProjectDetector
{
    /// <summary>
    /// Determines if the given solution path points to a Unity project.
    /// </summary>
    /// <param name="solutionPath">Absolute path to the .sln file.</param>
    /// <returns>True if this is a Unity project, false otherwise.</returns>
    public static bool IsUnityProject(string solutionPath)
    {
        if (string.IsNullOrEmpty(solutionPath) || !File.Exists(solutionPath))
            return false;

        var solutionDir = Path.GetDirectoryName(solutionPath);
        if (string.IsNullOrEmpty(solutionDir))
            return false;

        // Check 1: Unity folder structure (Assets + ProjectSettings)
        if (HasUnityFolderStructure(solutionDir))
            return true;

        // Check 2: Assembly-CSharp.csproj presence
        if (HasAssemblyCSharpProject(solutionDir))
            return true;

        // Check 3: Any .csproj with Unity references
        if (HasUnityReferencesInAnyProject(solutionDir))
            return true;

        return false;
    }

    /// <summary>
    /// Checks for Unity's characteristic folder structure.
    /// Unity projects always have Assets/ and ProjectSettings/ directories.
    /// </summary>
    /// <param name="directory">Directory to check.</param>
    /// <returns>True if Unity folder structure is detected.</returns>
    private static bool HasUnityFolderStructure(string directory)
    {
        var assetsPath = Path.Combine(directory, "Assets");
        var projectSettingsPath = Path.Combine(directory, "ProjectSettings");
        return Directory.Exists(assetsPath) && Directory.Exists(projectSettingsPath);
    }

    /// <summary>
    /// Checks for Assembly-CSharp.csproj which Unity always generates for runtime scripts.
    /// Also checks for Assembly-CSharp-Editor.csproj for editor scripts.
    /// </summary>
    /// <param name="directory">Directory to check.</param>
    /// <returns>True if Unity's standard project files are found.</returns>
    private static bool HasAssemblyCSharpProject(string directory)
    {
        var assemblyCSharpPath = Path.Combine(directory, "Assembly-CSharp.csproj");
        var assemblyCSharpEditorPath = Path.Combine(directory, "Assembly-CSharp-Editor.csproj");
        return File.Exists(assemblyCSharpPath) || File.Exists(assemblyCSharpEditorPath);
    }

    /// <summary>
    /// Scans .csproj files for Unity DLL references in HintPath elements.
    /// This catches custom assembly definition projects that reference Unity DLLs.
    /// </summary>
    /// <param name="directory">Directory to search for .csproj files.</param>
    /// <returns>True if any project contains Unity references.</returns>
    private static bool HasUnityReferencesInAnyProject(string directory)
    {
        try
        {
            var csprojFiles = Directory.GetFiles(directory, "*.csproj", SearchOption.TopDirectoryOnly);

            foreach (var csprojPath in csprojFiles)
            {
                if (HasUnityReferences(csprojPath))
                    return true;
            }
        }
        catch
        {
            // Ignore directory access errors
        }

        return false;
    }

    /// <summary>
    /// Checks a single .csproj file for Unity DLL references.
    /// Looks for HintPath elements containing UnityEngine.dll or UnityEditor.dll.
    /// </summary>
    /// <param name="csprojPath">Path to the .csproj file.</param>
    /// <returns>True if Unity references are found.</returns>
    public static bool HasUnityReferences(string csprojPath)
    {
        try
        {
            var xml = XDocument.Load(csprojPath);
            var hintPaths = xml.Descendants()
                .Where(e => e.Name.LocalName == "HintPath")
                .Select(e => e.Value);

            return hintPaths.Any(path =>
                path.Contains("UnityEngine.dll", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("UnityEditor.dll", StringComparison.OrdinalIgnoreCase) ||
                (path.Contains("Unity", StringComparison.OrdinalIgnoreCase) &&
                 path.Contains("Managed", StringComparison.OrdinalIgnoreCase)));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts Unity version from ProjectSettings/ProjectVersion.txt if present.
    /// Unity stores the editor version in this file with format "m_EditorVersion: 6000.0.0f1".
    /// </summary>
    /// <param name="solutionDir">Directory containing the Unity project.</param>
    /// <returns>Unity version string (e.g., "6000.0.0f1") or null if not found.</returns>
    public static string? GetUnityVersion(string solutionDir)
    {
        var versionFilePath = Path.Combine(solutionDir, "ProjectSettings", "ProjectVersion.txt");
        if (!File.Exists(versionFilePath))
            return null;

        try
        {
            var lines = File.ReadAllLines(versionFilePath);
            var editorVersionLine = lines.FirstOrDefault(l => l.StartsWith("m_EditorVersion:"));
            if (editorVersionLine != null)
            {
                return editorVersionLine.Replace("m_EditorVersion:", "").Trim();
            }
        }
        catch
        {
            // Ignore errors reading version file
        }

        return null;
    }
}
