using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ULSM.Unity.Analyzers;

/// <summary>
/// Loads and manages Microsoft.Unity.Analyzers DiagnosticAnalyzer instances.
/// Provides filtered access to Unity-specific analyzers by category.
/// </summary>
public static class UnityAnalyzerLoader
{
    private static ImmutableArray<DiagnosticAnalyzer>? _cachedAnalyzers;
    private static readonly object _loadLock = new();

    /// <summary>
    /// Category filters for Unity analyzers based on diagnostic ID ranges.
    /// </summary>
    public enum AnalyzerCategory
    {
        /// <summary>All Unity analyzers.</summary>
        All,
        /// <summary>UNT0001-UNT0015: Unity message issues (empty messages, incorrect signatures).</summary>
        Messages,
        /// <summary>UNT0016-UNT0025: Null handling issues (null coalescing, null propagation).</summary>
        NullChecking,
        /// <summary>UNT0026-UNT0035: Performance issues (tag comparison, GetComponent in loops).</summary>
        Performance,
        /// <summary>UNT0036+: General best practices.</summary>
        BestPractices
    }

    /// <summary>
    /// Loads all Unity analyzers from the Microsoft.Unity.Analyzers assembly.
    /// Results are cached for subsequent calls.
    /// </summary>
    /// <returns>Immutable array of DiagnosticAnalyzer instances.</returns>
    public static ImmutableArray<DiagnosticAnalyzer> LoadAllAnalyzers()
    {
        if (_cachedAnalyzers.HasValue)
            return _cachedAnalyzers.Value;

        lock (_loadLock)
        {
            if (_cachedAnalyzers.HasValue)
                return _cachedAnalyzers.Value;

            var analyzers = LoadAnalyzersFromAssembly();
            _cachedAnalyzers = analyzers;
            return analyzers;
        }
    }

    /// <summary>
    /// Gets analyzers filtered by category based on diagnostic ID ranges.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>Filtered list of analyzers supporting diagnostics in the specified category.</returns>
    public static ImmutableArray<DiagnosticAnalyzer> GetAnalyzersByCategory(AnalyzerCategory category)
    {
        if (category == AnalyzerCategory.All)
            return LoadAllAnalyzers();

        var allAnalyzers = LoadAllAnalyzers();
        var filtered = new List<DiagnosticAnalyzer>();

        foreach (var analyzer in allAnalyzers)
        {
            var diagnosticIds = analyzer.SupportedDiagnostics.Select(d => d.Id).ToList();

            bool matches = category switch
            {
                AnalyzerCategory.Messages => diagnosticIds.Any(id => IsInRange(id, 1, 15)),
                AnalyzerCategory.NullChecking => diagnosticIds.Any(id => IsInRange(id, 16, 25)),
                AnalyzerCategory.Performance => diagnosticIds.Any(id => IsInRange(id, 26, 35)),
                AnalyzerCategory.BestPractices => diagnosticIds.Any(id => IsInRange(id, 36, int.MaxValue)),
                _ => false
            };

            if (matches)
                filtered.Add(analyzer);
        }

        return filtered.ToImmutableArray();
    }

    /// <summary>
    /// Checks if a diagnostic ID is in the specified UNT range.
    /// </summary>
    /// <param name="id">The diagnostic ID (e.g., "UNT0001").</param>
    /// <param name="min">Minimum value (inclusive).</param>
    /// <param name="max">Maximum value (inclusive).</param>
    /// <returns>True if the ID is a UNT diagnostic in the specified range.</returns>
    private static bool IsInRange(string id, int min, int max)
    {
        if (!id.StartsWith("UNT", StringComparison.OrdinalIgnoreCase))
            return false;

        if (id.Length < 4)
            return false;

        if (int.TryParse(id.AsSpan(3), out var num))
            return num >= min && num <= max;

        return false;
    }

    /// <summary>
    /// Loads analyzers by reflection from the Microsoft.Unity.Analyzers assembly.
    /// </summary>
    /// <returns>Immutable array of instantiated analyzers.</returns>
    private static ImmutableArray<DiagnosticAnalyzer> LoadAnalyzersFromAssembly()
    {
        var analyzers = new List<DiagnosticAnalyzer>();

        try
        {
            // Find the assembly - it should be loaded via NuGet reference
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var unityAnalyzerAssembly = assemblies.FirstOrDefault(a =>
                a.GetName().Name == "Microsoft.Unity.Analyzers");

            if (unityAnalyzerAssembly == null)
            {
                // Try to load explicitly if not already loaded
                try
                {
                    unityAnalyzerAssembly = Assembly.Load("Microsoft.Unity.Analyzers");
                }
                catch (FileNotFoundException)
                {
                    Console.Error.WriteLine("[ULSM Warning] Microsoft.Unity.Analyzers assembly not found");
                    return ImmutableArray<DiagnosticAnalyzer>.Empty;
                }
            }

            // Find all types that are DiagnosticAnalyzers
            var analyzerTypes = unityAnalyzerAssembly.GetTypes()
                .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t)
                         && !t.IsAbstract
                         && t.GetConstructor(Type.EmptyTypes) != null);

            foreach (var type in analyzerTypes)
            {
                try
                {
                    if (Activator.CreateInstance(type) is DiagnosticAnalyzer analyzer)
                    {
                        analyzers.Add(analyzer);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[ULSM Warning] Failed to instantiate analyzer {type.Name}: {ex.Message}");
                }
            }

            Console.Error.WriteLine($"[ULSM] Loaded {analyzers.Count} Unity analyzers");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ULSM Error] Failed to load Unity analyzers: {ex.Message}");
        }

        return analyzers.ToImmutableArray();
    }

    /// <summary>
    /// Gets information about all available Unity diagnostics.
    /// Useful for documentation and discovery.
    /// </summary>
    /// <returns>List of diagnostic descriptors with metadata.</returns>
    public static IEnumerable<object> GetAvailableDiagnostics()
    {
        var analyzers = LoadAllAnalyzers();
        var diagnostics = new Dictionary<string, Microsoft.CodeAnalysis.DiagnosticDescriptor>();

        foreach (var analyzer in analyzers)
        {
            foreach (var descriptor in analyzer.SupportedDiagnostics)
            {
                if (!diagnostics.ContainsKey(descriptor.Id))
                {
                    diagnostics[descriptor.Id] = descriptor;
                }
            }
        }

        return diagnostics.Values
            .OrderBy(d => d.Id)
            .Select(d => new
            {
                id = d.Id,
                title = d.Title.ToString(),
                description = d.Description.ToString(),
                category = d.Category,
                severity = d.DefaultSeverity.ToString(),
                helpLink = d.HelpLinkUri
            });
    }

    /// <summary>
    /// Clears the analyzer cache. Useful for testing or reloading.
    /// </summary>
    public static void ClearCache()
    {
        lock (_loadLock)
        {
            _cachedAnalyzers = null;
        }
    }
}
