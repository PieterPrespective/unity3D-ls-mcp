using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ULSM.Unity.Analyzers;

namespace ULSM.Unity;

/// <summary>
/// Service that provides Unity-specific code analysis capabilities.
/// Integrates Microsoft.Unity.Analyzers and custom pattern detection.
/// </summary>
public class UnityAnalysisService
{
    private readonly RoslynService _roslynService;

    /// <summary>
    /// Creates a new UnityAnalysisService instance.
    /// </summary>
    /// <param name="roslynService">The Roslyn service for workspace access.</param>
    public UnityAnalysisService(RoslynService roslynService)
    {
        _roslynService = roslynService;
    }

    /// <summary>
    /// Gets Unity-specific diagnostics for a file, project, or entire solution.
    /// Uses Microsoft.Unity.Analyzers to detect Unity anti-patterns.
    /// </summary>
    /// <param name="filePath">Optional: specific file to analyze.</param>
    /// <param name="projectPath">Optional: specific project to analyze.</param>
    /// <param name="category">Optional: filter by diagnostic category (messages, nullchecking, performance, bestpractices).</param>
    /// <param name="includeSuppressions">Include diagnostics that have suppressions.</param>
    /// <param name="maxResults">Maximum diagnostics to return (default: 100).</param>
    /// <returns>Unity diagnostics with detailed information.</returns>
    public async Task<object> GetUnityDiagnosticsAsync(
        string? filePath = null,
        string? projectPath = null,
        string? category = null,
        bool includeSuppressions = false,
        int maxResults = 100)
    {
        var solution = _roslynService.GetSolution();
        if (solution == null)
        {
            return new
            {
                success = false,
                error = "No solution loaded. Call ulsm:load_solution first."
            };
        }

        // Load Unity analyzers
        var analyzerCategory = category?.ToLowerInvariant() switch
        {
            "messages" => UnityAnalyzerLoader.AnalyzerCategory.Messages,
            "nullchecking" or "null" => UnityAnalyzerLoader.AnalyzerCategory.NullChecking,
            "performance" => UnityAnalyzerLoader.AnalyzerCategory.Performance,
            "bestpractices" or "practices" => UnityAnalyzerLoader.AnalyzerCategory.BestPractices,
            _ => UnityAnalyzerLoader.AnalyzerCategory.All
        };

        var analyzers = UnityAnalyzerLoader.GetAnalyzersByCategory(analyzerCategory);

        if (analyzers.Length == 0)
        {
            return new
            {
                success = false,
                error = "No Unity analyzers loaded. Ensure Microsoft.Unity.Analyzers package is installed."
            };
        }

        var allDiagnostics = new List<object>();
        var projectsAnalyzed = 0;
        var filesAnalyzed = new HashSet<string>();

        // Determine which projects to analyze
        IEnumerable<Project> projects = solution.Projects;
        if (!string.IsNullOrEmpty(projectPath))
        {
            projects = projects.Where(p =>
                p.FilePath?.Equals(projectPath, StringComparison.OrdinalIgnoreCase) == true ||
                p.Name.Equals(Path.GetFileNameWithoutExtension(projectPath), StringComparison.OrdinalIgnoreCase));
        }

        foreach (var project in projects)
        {
            var compilation = await project.GetCompilationAsync();
            if (compilation == null)
                continue;

            projectsAnalyzed++;

            // Run analyzers
            var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);
            var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

            // Filter by file if specified
            if (!string.IsNullOrEmpty(filePath))
            {
                diagnostics = diagnostics.Where(d =>
                    d.Location.SourceTree?.FilePath?.Equals(filePath, StringComparison.OrdinalIgnoreCase) == true)
                    .ToImmutableArray();
            }

            // Filter Unity-specific diagnostics (UNT* and USP*)
            var unityDiagnostics = diagnostics
                .Where(d => d.Id.StartsWith("UNT", StringComparison.OrdinalIgnoreCase) ||
                           d.Id.StartsWith("USP", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var diagnostic in unityDiagnostics.Take(maxResults - allDiagnostics.Count))
            {
                var location = diagnostic.Location;
                var lineSpan = location.GetLineSpan();
                var sourceTree = location.SourceTree;

                if (sourceTree?.FilePath != null)
                    filesAnalyzed.Add(sourceTree.FilePath);

                allDiagnostics.Add(new
                {
                    id = diagnostic.Id,
                    severity = diagnostic.Severity.ToString(),
                    message = diagnostic.GetMessage(),
                    category = diagnostic.Descriptor.Category,
                    title = diagnostic.Descriptor.Title.ToString(),
                    description = diagnostic.Descriptor.Description.ToString(),
                    helpLink = diagnostic.Descriptor.HelpLinkUri,
                    file = sourceTree?.FilePath,
                    line = lineSpan.StartLinePosition.Line,
                    column = lineSpan.StartLinePosition.Character,
                    endLine = lineSpan.EndLinePosition.Line,
                    endColumn = lineSpan.EndLinePosition.Character,
                    project = project.Name
                });

                if (allDiagnostics.Count >= maxResults)
                    break;
            }

            if (allDiagnostics.Count >= maxResults)
                break;
        }

        // Group by ID for summary
        var summary = allDiagnostics
            .GroupBy(d => ((dynamic)d).id as string)
            .Select(g => new { id = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .ToList();

        return new
        {
            success = true,
            totalDiagnostics = allDiagnostics.Count,
            projectsAnalyzed,
            filesAnalyzed = filesAnalyzed.Count,
            analyzerCount = analyzers.Length,
            categoryFilter = category ?? "all",
            truncated = allDiagnostics.Count >= maxResults,
            summary,
            diagnostics = allDiagnostics
        };
    }

    /// <summary>
    /// Checks for Unity-specific performance patterns and anti-patterns.
    /// Goes beyond Microsoft.Unity.Analyzers with custom pattern detection.
    /// </summary>
    /// <param name="filePath">File to analyze (required).</param>
    /// <param name="includeInfo">Include informational patterns (default: true).</param>
    /// <param name="checkHotPathsOnly">Only check patterns in Update/FixedUpdate (default: false).</param>
    /// <returns>Pattern analysis results with suggestions.</returns>
    public async Task<object> CheckUnityPatternsAsync(
        string filePath,
        bool includeInfo = true,
        bool checkHotPathsOnly = false)
    {
        var solution = _roslynService.GetSolution();
        if (solution == null)
        {
            return new
            {
                success = false,
                error = "No solution loaded. Call ulsm:load_solution first."
            };
        }

        // Find the document
        Document? document = null;
        foreach (var project in solution.Projects)
        {
            document = project.Documents.FirstOrDefault(d =>
                d.FilePath?.Equals(filePath, StringComparison.OrdinalIgnoreCase) == true);
            if (document != null)
                break;
        }

        if (document == null)
        {
            return new
            {
                success = false,
                error = $"File not found in solution: {filePath}"
            };
        }

        var semanticModel = await document.GetSemanticModelAsync();
        if (semanticModel == null)
        {
            return new
            {
                success = false,
                error = "Could not get semantic model for file"
            };
        }

        // Run custom pattern analysis
        var analyzer = new UnityPatternAnalyzer();
        var issues = await analyzer.AnalyzeDocumentAsync(document, semanticModel);

        // Filter by severity if requested
        if (!includeInfo)
        {
            issues = issues.Where(i => i.Severity != "Info").ToList();
        }

        // Group by category
        var byCategory = issues
            .GroupBy(i => i.Category)
            .Select(g => new { category = g.Key, count = g.Count() })
            .ToList();

        // Group by ID
        var byId = issues
            .GroupBy(i => i.Id)
            .Select(g => new { id = g.Key, title = g.First().Title, count = g.Count() })
            .OrderByDescending(x => x.count)
            .ToList();

        return new
        {
            success = true,
            file = filePath,
            totalIssues = issues.Count,
            byCategory,
            byId,
            issues = issues.Select(i => new
            {
                i.Id,
                i.Title,
                i.Description,
                i.Severity,
                i.Category,
                i.Line,
                i.Column,
                i.CodeSnippet,
                i.Suggestion
            }).ToList()
        };
    }

    /// <summary>
    /// Checks for deprecated Unity API usage that should be migrated.
    /// </summary>
    /// <param name="filePath">Optional: specific file to check.</param>
    /// <param name="projectPath">Optional: specific project to check.</param>
    /// <param name="targetVersion">Unity version to check migrations for (default: 6000.0).</param>
    /// <param name="categoryFilter">Optional: filter by migration category.</param>
    /// <returns>API migration recommendations.</returns>
    public async Task<object> CheckApiMigrationAsync(
        string? filePath = null,
        string? projectPath = null,
        string targetVersion = "6000.0",
        string? categoryFilter = null)
    {
        var solution = _roslynService.GetSolution();
        if (solution == null)
        {
            return new
            {
                success = false,
                error = "No solution loaded. Call ulsm:load_solution first."
            };
        }

        // Get relevant migrations for target version
        var migrations = UnityApiMigrationData.GetMigrationsForVersion(targetVersion);
        if (!string.IsNullOrEmpty(categoryFilter))
        {
            migrations = migrations.Where(m =>
                m.Category.Equals(categoryFilter, StringComparison.OrdinalIgnoreCase));
        }

        var migrationList = migrations.ToList();
        var findings = new List<object>();
        var filesChecked = 0;
        var projectsChecked = 0;

        // Determine which projects to analyze
        IEnumerable<Project> projects = solution.Projects;
        if (!string.IsNullOrEmpty(projectPath))
        {
            projects = projects.Where(p =>
                p.FilePath?.Equals(projectPath, StringComparison.OrdinalIgnoreCase) == true ||
                p.Name.Equals(Path.GetFileNameWithoutExtension(projectPath), StringComparison.OrdinalIgnoreCase));
        }

        foreach (var project in projects)
        {
            projectsChecked++;

            IEnumerable<Document> documents = project.Documents;
            if (!string.IsNullOrEmpty(filePath))
            {
                documents = documents.Where(d =>
                    d.FilePath?.Equals(filePath, StringComparison.OrdinalIgnoreCase) == true);
            }

            foreach (var document in documents)
            {
                if (document.FilePath == null || !document.FilePath.EndsWith(".cs"))
                    continue;

                filesChecked++;
                var text = await document.GetTextAsync();
                var content = text.ToString();

                // Simple text-based search for deprecated APIs
                foreach (var migration in migrationList)
                {
                    var searchTerm = ExtractSearchTerm(migration.OldApi);
                    if (string.IsNullOrEmpty(searchTerm))
                        continue;

                    var index = 0;
                    while ((index = content.IndexOf(searchTerm, index, StringComparison.Ordinal)) >= 0)
                    {
                        // Get line number
                        var linePosition = text.Lines.GetLinePosition(index);
                        var line = text.Lines[linePosition.Line];

                        findings.Add(new
                        {
                            oldApi = migration.OldApi,
                            newApi = migration.NewApi,
                            category = migration.Category,
                            deprecatedIn = migration.MinVersion,
                            removedIn = migration.RemovedVersion,
                            notes = migration.Notes,
                            file = document.FilePath,
                            line = linePosition.Line,
                            column = linePosition.Character,
                            codeSnippet = line.ToString().Trim()
                        });

                        index += searchTerm.Length;
                    }
                }
            }
        }

        // Group findings by API
        var byApi = findings
            .GroupBy(f => ((dynamic)f).oldApi as string)
            .Select(g => new { api = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .ToList();

        // Group by category
        var byCategory = findings
            .GroupBy(f => ((dynamic)f).category as string)
            .Select(g => new { category = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .ToList();

        return new
        {
            success = true,
            targetVersion,
            migrationRulesChecked = migrationList.Count,
            projectsChecked,
            filesChecked,
            totalFindings = findings.Count,
            byApi,
            byCategory,
            findings
        };
    }

    /// <summary>
    /// Gets list of available Unity diagnostics for discovery.
    /// </summary>
    /// <returns>Information about available analyzers, custom patterns, and migration rules.</returns>
    public object GetAvailableDiagnostics()
    {
        var diagnostics = UnityAnalyzerLoader.GetAvailableDiagnostics().ToList();
        var migrations = UnityApiMigrationData.GetAllMigrations();

        return new
        {
            analyzerDiagnostics = diagnostics,
            analyzerCount = diagnostics.Count,
            customPatterns = new[]
            {
                new { id = "ULSM0001", title = "Expensive call in hot path", category = "Performance" },
                new { id = "ULSM0002", title = "String operation in hot path", category = "Performance" },
                new { id = "ULSM0003", title = "Debug logging in hot path", category = "Performance" },
                new { id = "ULSM0004", title = "Camera.main usage", category = "Performance" }
            },
            migrationRules = migrations.Count,
            migrationCategories = UnityApiMigrationData.GetCategories().ToList()
        };
    }

    /// <summary>
    /// Extracts a searchable term from an API specification.
    /// </summary>
    /// <param name="apiSpec">Full API specification (e.g., "UnityEngine.Input.GetAxis").</param>
    /// <returns>Search term for text matching (e.g., "Input.GetAxis").</returns>
    private static string ExtractSearchTerm(string apiSpec)
    {
        // Handle patterns like "UnityEngine.Input.GetAxis" -> "Input.GetAxis"
        // Or "Camera.main" -> "Camera.main"

        if (apiSpec.Contains('('))
            apiSpec = apiSpec.Substring(0, apiSpec.IndexOf('('));

        var parts = apiSpec.Split('.');

        // Return last two parts for better matching
        if (parts.Length >= 2)
            return $"{parts[^2]}.{parts[^1]}";

        return parts[^1];
    }
}
