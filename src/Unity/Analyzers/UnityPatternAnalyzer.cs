using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ULSM.Unity.Analyzers;

/// <summary>
/// Custom Unity pattern analyzer for detecting performance issues and anti-patterns
/// that are not covered by Microsoft.Unity.Analyzers.
/// </summary>
public class UnityPatternAnalyzer
{
    /// <summary>
    /// Represents a detected Unity pattern issue.
    /// </summary>
    /// <param name="Id">Diagnostic ID (ULSM0001-ULSM0004).</param>
    /// <param name="Title">Short title describing the issue.</param>
    /// <param name="Description">Detailed description of the problem.</param>
    /// <param name="Severity">Error, Warning, or Info.</param>
    /// <param name="Category">Issue category (e.g., Performance).</param>
    /// <param name="FilePath">Path to the source file.</param>
    /// <param name="Line">Zero-based line number.</param>
    /// <param name="Column">Zero-based column number.</param>
    /// <param name="CodeSnippet">The problematic code.</param>
    /// <param name="Suggestion">Recommended fix or alternative approach.</param>
    public record PatternIssue(
        string Id,
        string Title,
        string Description,
        string Severity,
        string Category,
        string FilePath,
        int Line,
        int Column,
        string CodeSnippet,
        string? Suggestion
    );

    /// <summary>
    /// Unity message methods that run frequently (hot paths).
    /// </summary>
    private static readonly HashSet<string> HotPathMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "Update", "LateUpdate", "FixedUpdate",
        "OnGUI", "OnRenderObject", "OnDrawGizmos",
        "OnTriggerStay", "OnCollisionStay"
    };

    /// <summary>
    /// Methods that involve expensive operations with suggested alternatives.
    /// </summary>
    private static readonly Dictionary<string, string> ExpensiveMethodPatterns = new()
    {
        ["GetComponent"] = "Cache component references in Awake/Start instead of calling every frame",
        ["GetComponentInChildren"] = "Cache component references in Awake/Start",
        ["GetComponentInParent"] = "Cache component references in Awake/Start",
        ["GetComponents"] = "Cache component array in Awake/Start",
        ["FindObjectOfType"] = "Use object references or singletons instead - extremely expensive",
        ["FindObjectsOfType"] = "Use object references or caching - extremely expensive",
        ["FindGameObjectWithTag"] = "Cache references in Awake/Start",
        ["FindGameObjectsWithTag"] = "Cache references or use object pooling",
        ["Find"] = "Use direct references or cached lookups instead of Find()",
        ["SendMessage"] = "Use direct method calls or events instead - uses reflection",
        ["BroadcastMessage"] = "Use direct method calls or events instead - uses reflection",
        ["SendMessageUpwards"] = "Use direct method calls or events instead - uses reflection"
    };

    /// <summary>
    /// Analyzes a syntax tree for Unity-specific patterns.
    /// </summary>
    /// <param name="document">The Roslyn document to analyze.</param>
    /// <param name="semanticModel">The semantic model for symbol resolution.</param>
    /// <returns>List of detected pattern issues.</returns>
    public async Task<List<PatternIssue>> AnalyzeDocumentAsync(Document document, SemanticModel semanticModel)
    {
        var issues = new List<PatternIssue>();
        var root = await document.GetSyntaxRootAsync();

        if (root == null)
            return issues;

        var filePath = document.FilePath ?? "unknown";

        // Find all class declarations that inherit from MonoBehaviour
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

        foreach (var classDecl in classes)
        {
            if (!InheritsFromMonoBehaviour(classDecl, semanticModel))
                continue;

            // Analyze methods within MonoBehaviour classes
            var methods = classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                var methodName = method.Identifier.Text;
                var isHotPath = HotPathMethods.Contains(methodName);

                // Check for expensive calls in hot paths
                if (isHotPath)
                {
                    issues.AddRange(CheckExpensiveCallsInHotPath(method, filePath, semanticModel));
                    issues.AddRange(CheckStringConcatenationInHotPath(method, filePath));
                    issues.AddRange(CheckBoxingInHotPath(method, filePath, semanticModel));
                }

                // Check Camera.main usage anywhere
                issues.AddRange(CheckCameraMainUsage(method, filePath, isHotPath));
            }
        }

        return issues;
    }

    /// <summary>
    /// Checks if a class inherits from MonoBehaviour by examining the type hierarchy.
    /// </summary>
    /// <param name="classDecl">The class declaration syntax.</param>
    /// <param name="semanticModel">The semantic model for type resolution.</param>
    /// <returns>True if the class inherits from MonoBehaviour.</returns>
    private static bool InheritsFromMonoBehaviour(ClassDeclarationSyntax classDecl, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(classDecl);
        if (symbol == null)
            return false;

        var baseType = symbol.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == "MonoBehaviour" ||
                baseType.ToDisplayString().Contains("UnityEngine.MonoBehaviour"))
                return true;
            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Detects expensive method calls in Unity hot path methods.
    /// </summary>
    /// <param name="method">The method to analyze.</param>
    /// <param name="filePath">Source file path for reporting.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <returns>List of pattern issues for expensive calls.</returns>
    private static List<PatternIssue> CheckExpensiveCallsInHotPath(
        MethodDeclarationSyntax method,
        string filePath,
        SemanticModel semanticModel)
    {
        var issues = new List<PatternIssue>();
        var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            string? methodName = null;

            // Handle different invocation patterns
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                methodName = memberAccess.Name.Identifier.Text;
            }
            else if (invocation.Expression is IdentifierNameSyntax identifier)
            {
                methodName = identifier.Identifier.Text;
            }

            if (methodName != null && ExpensiveMethodPatterns.TryGetValue(methodName, out var suggestion))
            {
                var location = invocation.GetLocation();
                var lineSpan = location.GetLineSpan();

                issues.Add(new PatternIssue(
                    Id: "ULSM0001",
                    Title: $"Expensive call '{methodName}' in hot path",
                    Description: $"'{methodName}' is called in '{method.Identifier.Text}' which runs frequently. This can cause performance issues.",
                    Severity: "Warning",
                    Category: "Performance",
                    FilePath: filePath,
                    Line: lineSpan.StartLinePosition.Line,
                    Column: lineSpan.StartLinePosition.Character,
                    CodeSnippet: invocation.ToString(),
                    Suggestion: suggestion
                ));
            }
        }

        return issues;
    }

    /// <summary>
    /// Detects string concatenation in hot paths (causes GC allocation).
    /// </summary>
    /// <param name="method">The method to analyze.</param>
    /// <param name="filePath">Source file path for reporting.</param>
    /// <returns>List of pattern issues for string operations.</returns>
    private static List<PatternIssue> CheckStringConcatenationInHotPath(
        MethodDeclarationSyntax method,
        string filePath)
    {
        var issues = new List<PatternIssue>();
        var binaryExpressions = method.DescendantNodes().OfType<BinaryExpressionSyntax>()
            .Where(b => b.IsKind(SyntaxKind.AddExpression));

        foreach (var expr in binaryExpressions)
        {
            // Check if this is string concatenation
            if (ContainsStringLiteral(expr.Left) || ContainsStringLiteral(expr.Right))
            {
                var location = expr.GetLocation();
                var lineSpan = location.GetLineSpan();

                issues.Add(new PatternIssue(
                    Id: "ULSM0002",
                    Title: "String concatenation in hot path",
                    Description: $"String concatenation in '{method.Identifier.Text}' allocates memory every frame.",
                    Severity: "Warning",
                    Category: "Performance",
                    FilePath: filePath,
                    Line: lineSpan.StartLinePosition.Line,
                    Column: lineSpan.StartLinePosition.Character,
                    CodeSnippet: expr.ToString(),
                    Suggestion: "Use StringBuilder, string.Format with cached values, or avoid string operations in Update"
                ));
            }
        }

        // Also check interpolated strings
        var interpolations = method.DescendantNodes().OfType<InterpolatedStringExpressionSyntax>();
        foreach (var interp in interpolations)
        {
            var location = interp.GetLocation();
            var lineSpan = location.GetLineSpan();

            issues.Add(new PatternIssue(
                Id: "ULSM0002",
                Title: "String interpolation in hot path",
                Description: $"String interpolation in '{method.Identifier.Text}' allocates memory every frame.",
                Severity: "Warning",
                Category: "Performance",
                FilePath: filePath,
                Line: lineSpan.StartLinePosition.Line,
                Column: lineSpan.StartLinePosition.Character,
                CodeSnippet: interp.ToString(),
                Suggestion: "Cache formatted strings or avoid string operations in Update"
            ));
        }

        return issues;
    }

    /// <summary>
    /// Detects potential boxing operations in hot paths via Debug.Log calls.
    /// </summary>
    /// <param name="method">The method to analyze.</param>
    /// <param name="filePath">Source file path for reporting.</param>
    /// <param name="semanticModel">The semantic model for type resolution.</param>
    /// <returns>List of pattern issues for debug logging.</returns>
    private static List<PatternIssue> CheckBoxingInHotPath(
        MethodDeclarationSyntax method,
        string filePath,
        SemanticModel semanticModel)
    {
        var issues = new List<PatternIssue>();

        // Check for calls to methods that take object parameters with value types
        var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            // Check specifically for Debug.Log with value types (common boxing case)
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Expression.ToString() == "Debug" &&
                    (memberAccess.Name.Identifier.Text == "Log" ||
                     memberAccess.Name.Identifier.Text == "LogWarning" ||
                     memberAccess.Name.Identifier.Text == "LogError"))
                {
                    var location = invocation.GetLocation();
                    var lineSpan = location.GetLineSpan();

                    issues.Add(new PatternIssue(
                        Id: "ULSM0003",
                        Title: "Debug logging in hot path",
                        Description: $"Debug.Log in '{method.Identifier.Text}' should be removed or conditionally compiled for release builds.",
                        Severity: "Info",
                        Category: "Performance",
                        FilePath: filePath,
                        Line: lineSpan.StartLinePosition.Line,
                        Column: lineSpan.StartLinePosition.Character,
                        CodeSnippet: invocation.ToString(),
                        Suggestion: "Wrap in #if UNITY_EDITOR or [Conditional(\"UNITY_EDITOR\")] attribute, or remove for release"
                    ));
                }
            }
        }

        return issues;
    }

    /// <summary>
    /// Detects Camera.main usage which calls FindObjectByTag internally.
    /// </summary>
    /// <param name="method">The method to analyze.</param>
    /// <param name="filePath">Source file path for reporting.</param>
    /// <param name="isHotPath">Whether this method is a hot path (affects severity).</param>
    /// <returns>List of pattern issues for Camera.main usage.</returns>
    private static List<PatternIssue> CheckCameraMainUsage(
        MethodDeclarationSyntax method,
        string filePath,
        bool isHotPath)
    {
        var issues = new List<PatternIssue>();
        var memberAccesses = method.DescendantNodes().OfType<MemberAccessExpressionSyntax>();

        foreach (var access in memberAccesses)
        {
            if (access.Expression.ToString() == "Camera" &&
                access.Name.Identifier.Text == "main")
            {
                var location = access.GetLocation();
                var lineSpan = location.GetLineSpan();

                issues.Add(new PatternIssue(
                    Id: "ULSM0004",
                    Title: "Camera.main usage",
                    Description: isHotPath
                        ? $"Camera.main in '{method.Identifier.Text}' calls FindObjectByTag every frame."
                        : "Camera.main calls FindObjectByTag internally. Consider caching the reference.",
                    Severity: isHotPath ? "Warning" : "Info",
                    Category: "Performance",
                    FilePath: filePath,
                    Line: lineSpan.StartLinePosition.Line,
                    Column: lineSpan.StartLinePosition.Character,
                    CodeSnippet: access.ToString(),
                    Suggestion: "Cache Camera.main reference in Awake/Start: private Camera _mainCamera; void Awake() => _mainCamera = Camera.main;"
                ));
            }
        }

        return issues;
    }

    /// <summary>
    /// Helper to check if an expression contains a string literal.
    /// </summary>
    /// <param name="expr">The expression to check.</param>
    /// <returns>True if the expression is a string literal.</returns>
    private static bool ContainsStringLiteral(ExpressionSyntax expr)
    {
        return expr is LiteralExpressionSyntax literal &&
               literal.IsKind(SyntaxKind.StringLiteralExpression);
    }
}
