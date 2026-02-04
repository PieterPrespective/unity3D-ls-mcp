using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace ULSM.Unity;

/// <summary>
/// Builds an AdhocWorkspace by manually parsing Unity .csproj files.
/// Used as fallback when MSBuildWorkspace fails to load Unity projects.
/// </summary>
public static class UnityAdhocWorkspaceBuilder
{
    /// <summary>
    /// Builds an AdhocWorkspace from a Unity solution by parsing all referenced .csproj files.
    /// Parses the solution file to find project references, then parses each .csproj to extract
    /// source files, references, and preprocessor defines.
    /// </summary>
    /// <param name="solutionPath">Path to the .sln file.</param>
    /// <returns>Tuple of (AdhocWorkspace, Solution).</returns>
    public static async Task<(AdhocWorkspace workspace, Solution solution)> BuildFromSolutionAsync(string solutionPath)
    {
        var solutionDir = Path.GetDirectoryName(solutionPath)!;
        var workspace = new AdhocWorkspace();

        // Parse solution file to find project references
        var projectPaths = ParseSolutionProjects(solutionPath);

        // Build each project
        var projectIdMap = new Dictionary<string, ProjectId>();

        foreach (var projectPath in projectPaths)
        {
            var fullPath = Path.IsPathRooted(projectPath)
                ? projectPath
                : Path.Combine(solutionDir, projectPath);

            if (!File.Exists(fullPath) || !fullPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                continue;

            var projectId = ProjectId.CreateNewId();
            projectIdMap[fullPath] = projectId;
        }

        // Now create projects with references
        foreach (var (projectPath, projectId) in projectIdMap)
        {
            await AddProjectToWorkspaceAsync(workspace, projectPath, projectId, projectIdMap);
        }

        return (workspace, workspace.CurrentSolution);
    }

    /// <summary>
    /// Parses a .sln file to extract project paths.
    /// Looks for Project lines and extracts the .csproj path from the second comma-separated value.
    /// </summary>
    /// <param name="solutionPath">Path to the solution file.</param>
    /// <returns>List of project paths referenced in the solution.</returns>
    private static List<string> ParseSolutionProjects(string solutionPath)
    {
        var projects = new List<string>();
        var lines = File.ReadAllLines(solutionPath);

        foreach (var line in lines)
        {
            // Match: Project("{...}") = "Name", "path.csproj", "{...}"
            if (line.StartsWith("Project("))
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    var projectPath = parts[1].Trim().Trim('"');
                    if (projectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                    {
                        projects.Add(projectPath);
                    }
                }
            }
        }

        return projects;
    }

    /// <summary>
    /// Adds a single project to the workspace by parsing its .csproj file.
    /// Creates ProjectInfo with source files, metadata references, and compilation options.
    /// </summary>
    /// <param name="workspace">The AdhocWorkspace to add the project to.</param>
    /// <param name="csprojPath">Path to the .csproj file.</param>
    /// <param name="projectId">The ProjectId to use for this project.</param>
    /// <param name="projectIdMap">Map of project paths to IDs for resolving project references.</param>
    private static async Task AddProjectToWorkspaceAsync(
        AdhocWorkspace workspace,
        string csprojPath,
        ProjectId projectId,
        Dictionary<string, ProjectId> projectIdMap)
    {
        var projectDir = Path.GetDirectoryName(csprojPath)!;
        var projectName = Path.GetFileNameWithoutExtension(csprojPath);

        var (sourceFiles, references, defines) = ParseCsprojFile(csprojPath);

        // Create parse options with preprocessor defines
        var parseOptions = new CSharpParseOptions(
            LanguageVersion.CSharp9,
            DocumentationMode.Parse,
            SourceCodeKind.Regular,
            defines
        );

        // Create compilation options
        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            allowUnsafe: true
        );

        // Load metadata references
        var metadataReferences = new List<MetadataReference>();
        foreach (var refPath in references)
        {
            var fullRefPath = Path.IsPathRooted(refPath)
                ? refPath
                : Path.Combine(projectDir, refPath);

            if (File.Exists(fullRefPath))
            {
                try
                {
                    metadataReferences.Add(MetadataReference.CreateFromFile(fullRefPath));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[ULSM Warning] Failed to load reference {fullRefPath}: {ex.Message}");
                }
            }
        }

        // Create project info
        var projectInfo = ProjectInfo.Create(
            projectId,
            VersionStamp.Default,
            projectName,
            projectName,
            LanguageNames.CSharp,
            filePath: csprojPath,
            parseOptions: parseOptions,
            compilationOptions: compilationOptions,
            metadataReferences: metadataReferences
        );

        // Add project to workspace
        workspace.AddProject(projectInfo);

        // Add source documents
        foreach (var sourceFile in sourceFiles)
        {
            var fullSourcePath = Path.IsPathRooted(sourceFile)
                ? sourceFile
                : Path.Combine(projectDir, sourceFile);

            if (File.Exists(fullSourcePath))
            {
                try
                {
                    var sourceText = SourceText.From(await File.ReadAllTextAsync(fullSourcePath));
                    var documentId = DocumentId.CreateNewId(projectId);
                    var documentInfo = DocumentInfo.Create(
                        documentId,
                        Path.GetFileName(fullSourcePath),
                        filePath: fullSourcePath,
                        loader: TextLoader.From(TextAndVersion.Create(sourceText, VersionStamp.Default))
                    );
                    workspace.AddDocument(documentInfo);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[ULSM Warning] Failed to load source file {fullSourcePath}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Parses a Unity .csproj file to extract source files, references, and defines.
    /// Handles Unity's legacy ToolsVersion 4.0 format with Compile elements and HintPath references.
    /// </summary>
    /// <param name="csprojPath">Path to the .csproj file to parse.</param>
    /// <returns>Tuple of (sourceFiles, references, defines).</returns>
    public static (List<string> sourceFiles, List<string> references, IEnumerable<string> defines) ParseCsprojFile(string csprojPath)
    {
        var sourceFiles = new List<string>();
        var references = new List<string>();
        var defines = new List<string>();

        try
        {
            var xml = XDocument.Load(csprojPath);

            // Extract source files from <Compile Include="...">
            sourceFiles.AddRange(
                xml.Descendants()
                    .Where(e => e.Name.LocalName == "Compile")
                    .Select(e => e.Attribute("Include")?.Value)
                    .Where(v => !string.IsNullOrEmpty(v) && v.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))!
            );

            // Extract references from <HintPath>
            references.AddRange(
                xml.Descendants()
                    .Where(e => e.Name.LocalName == "HintPath")
                    .Select(e => e.Value)
                    .Where(v => !string.IsNullOrEmpty(v))
            );

            // Extract preprocessor defines from <DefineConstants>
            var defineConstants = xml.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "DefineConstants");
            if (defineConstants != null && !string.IsNullOrEmpty(defineConstants.Value))
            {
                defines.AddRange(defineConstants.Value.Split(';', StringSplitOptions.RemoveEmptyEntries));
            }

            // Add common Unity defines if not present
            if (!defines.Contains("UNITY_EDITOR"))
            {
                defines.AddRange(new[]
                {
                    "UNITY_EDITOR",
                    "UNITY_2022_3_OR_NEWER",
                    "UNITY_6000_0_OR_NEWER"
                });
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ULSM Warning] Failed to parse {csprojPath}: {ex.Message}");
        }

        return (sourceFiles, references, defines);
    }
}
