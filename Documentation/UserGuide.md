# ULSM User Guide

## Table of Contents

1. [Introduction](#introduction)
2. [Installation](#installation)
3. [Configuration](#configuration)
4. [Unity Project Setup](#unity-project-setup)
5. [Tool Reference](#tool-reference)
6. [Troubleshooting](#troubleshooting)
7. [FAQ](#faq)

---

## Introduction

ULSM (Unity Language Server MCP) is a Model Context Protocol server that provides Roslyn-based semantic code analysis specifically optimized for Unity projects. It enables AI coding assistants like Claude Code to understand your Unity codebase at a deep semantic level.

### Key Capabilities

- **Semantic Code Understanding**: Full compiler-accurate analysis of your C# code
- **Unity-Aware Analysis**: Understands MonoBehaviour lifecycle, Unity APIs, and common patterns
- **Anti-Pattern Detection**: Finds performance issues like GetComponent in Update loops
- **API Migration Assistance**: Identifies deprecated Unity APIs and suggests replacements
- **Safe Refactoring**: Rename symbols, extract interfaces, and organize code with preview

---

## Installation

### Global Tool Installation (Recommended)

```bash
dotnet tool install --global ulsm
```

### Verify Installation

```bash
ulsm --version
```

### Update to Latest Version

```bash
dotnet tool update --global ulsm
```

### Uninstall

```bash
dotnet tool uninstall --global ulsm
```

---

## Configuration

### Claude Code Configuration

Add ULSM to Claude Code:

```bash
claude mcp add --transport stdio ulsm \
  --env DOTNET_SOLUTION_PATH="/path/to/YourProject.sln" \
  -- ulsm
```

### Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `DOTNET_SOLUTION_PATH` | Yes | Path to your Unity project's .sln file |
| `ULSM_FRAMEWORK_PATH` | No | Override .NET Framework reference assembly path |
| `ULSM_FORCE_ADHOC` | No | Set to `true` to bypass MSBuild |
| `ULSM_LOG_LEVEL` | No | Logging verbosity (Debug, Information, Warning, Error) |
| `ULSM_TIMEOUT_SECONDS` | No | Operation timeout in seconds |
| `ULSM_MAX_DIAGNOSTICS` | No | Maximum diagnostics to return per request |
| `UNITY_EDITOR_PATH` | No | Path to Unity Editor for reference resolution |

### Path Resolution for DOTNET_SOLUTION_PATH

The solution path supports multiple formats:

| Format | Example | Resolution |
|--------|---------|------------|
| Absolute | `C:/Projects/MyGame/MyGame.sln` | Used as-is |
| Variable | `${workspaceFolder}/MyGame.sln` | Expanded by MCP client before passing to ULSM |
| Relative | `./MyGame.sln` | Resolved against the MCP server's working directory |

**Recommendation:** Use `${workspaceFolder}` for portable configuration that works across different machines:

```json
"DOTNET_SOLUTION_PATH": "${workspaceFolder}/MyGame.sln"
```

Relative paths work but depend on the working directory when the MCP server starts, which may vary.

### Configuration File

**.claude/mcp-spec.json** (in your project root):

```json
{
  "mcpServers": {
    "ulsm": {
      "command": "ulsm",
      "env": {
        "DOTNET_SOLUTION_PATH": "${workspaceFolder}/YourGame.sln",
        "ULSM_LOG_LEVEL": "Information"
      }
    }
  }
}
```

---

## Unity Project Setup

### Generating Project Files

Before using ULSM, ensure Unity has generated the .sln and .csproj files:

1. Open your Unity project
2. Go to **Edit -> Preferences -> External Tools**
3. Click **Regenerate project files**

### Required Files

ULSM detects Unity projects by looking for:

- `Assembly-CSharp.csproj` (main scripts)
- `Assets/` folder
- `ProjectSettings/` folder
- `ProjectSettings/ProjectVersion.txt`

### Framework Path Setup

Unity targets .NET Framework 4.7.1. For full reference resolution, install one of:

**Windows:**
- .NET Framework 4.7.1 Targeting Pack (from Visual Studio Installer or direct download)

**macOS/Linux:**
- Mono Framework

Or set `ULSM_FRAMEWORK_PATH` to your reference assemblies location:

```bash
# Windows
ULSM_FRAMEWORK_PATH="C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.1"

# macOS
ULSM_FRAMEWORK_PATH="/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.7.1-api"

# Linux
ULSM_FRAMEWORK_PATH="/usr/lib/mono/4.7.1-api"
```

---

## Tool Reference

### Unity-Specific Tools

#### ulsm:unity_diagnostics

Run Microsoft.Unity.Analyzers on your code.

**Parameters:**
- `filePath` (optional): Analyze specific file
- `projectPath` (optional): Analyze specific project
- `category` (optional): Filter by category (Performance, Correctness, etc.)

**Example prompts:**
- "Get Unity diagnostics for PlayerController.cs"
- "Show all Unity warnings in the Assembly-CSharp project"

#### ulsm:check_unity_patterns

Find Unity anti-patterns using custom ULSM rules.

**Parameters:**
- `filePath`: File to analyze

**Example prompts:**
- "Check my EnemyAI script for performance anti-patterns"
- "Find GetComponent calls in Update methods in PlayerController.cs"

#### ulsm:api_migration

Check for deprecated Unity APIs.

**Parameters:**
- `filePath` (optional): Check specific file
- `targetVersion` (optional): Unity version to target (default: 6000.0)
- `category` (optional): Filter by category (Input, Networking, etc.)

**Example prompts:**
- "What deprecated APIs am I using that need to change for Unity 6?"
- "Check my project for deprecated Input API calls"

#### ulsm:list_unity_rules

List all available Unity diagnostic rules.

**Example prompt:** "Show me all Unity analyzer rules"

### Navigation Tools

#### ulsm:find_references

Find all usages of a symbol.

**Example prompt:** "Find all references to the TakeDamage method"

#### ulsm:find_implementations

Find implementations of interfaces or virtual methods.

**Example prompt:** "What classes implement IWeapon?"

#### ulsm:find_callers

Find all methods that call a specific method.

**Example prompt:** "What code calls PlayerStats.AddExperience?"

#### ulsm:get_type_hierarchy

Get the inheritance tree for a type.

**Example prompt:** "Show me the type hierarchy for EnemyBase"

#### ulsm:go_to_definition

Navigate to where a symbol is defined.

**Example prompt:** "Go to the definition of ProcessInput"

### Refactoring Tools

#### ulsm:rename_symbol

Safely rename a symbol across the solution.

**Parameters:**
- `filePath`: File containing the symbol
- `line`: Line number
- `column`: Column number
- `newName`: New name
- `preview`: Set to true for preview only

**Example prompts:**
- "Rename the ProcessInput method to HandleInput"
- "Preview renaming IWeaponHandler to IWeapon"

#### ulsm:extract_interface

Generate an interface from a class.

**Example prompt:** "Extract an interface from the PlayerService class"

#### ulsm:organize_usings

Sort and remove unused using directives.

**Example prompt:** "Organize usings in PlayerController.cs"

### Diagnostic Tools

#### ulsm:get_diagnostics

Get compiler errors and warnings.

**Example prompts:**
- "Show all compilation errors"
- "Get warnings in GameManager.cs"

#### ulsm:get_code_fixes

Get available fixes for diagnostics.

**Example prompt:** "What fixes are available for the errors in LevelManager.cs?"

#### ulsm:find_unused_code

Find dead code in your project.

**Example prompts:**
- "Find unused methods in the Scripts folder"
- "What private fields are never accessed?"

---

## Troubleshooting

### "Unity project not detected"

1. Ensure `.sln` and `.csproj` files exist (regenerate in Unity if needed)
2. Use `${workspaceFolder}/YourProject.sln` or an absolute path in `DOTNET_SOLUTION_PATH` for reliable resolution
3. Check that `Assets/` and `ProjectSettings/` folders exist
4. If using relative paths, verify they resolve correctly from the MCP server's working directory

### "Missing type references" (CS0012 errors)

1. Install .NET Framework 4.7.1 Targeting Pack (Windows)
2. Or set `ULSM_FRAMEWORK_PATH` manually
3. Or install Mono (macOS/Linux)

### "MSBuild failed to load solution"

1. Try `ULSM_FORCE_ADHOC=true` to bypass MSBuild
2. Check that all projects in the solution can be found
3. Regenerate project files in Unity

### "Analysis times out"

1. Increase `ULSM_TIMEOUT_SECONDS` (e.g., 120)
2. Ensure no other heavy processes are using the solution

### "Unity analyzers not loading"

1. Verify solution is loaded (`ulsm:health_check`)
2. Check for `unityAnalyzersLoaded: true` in health check
3. Ensure Microsoft.Unity.Analyzers package is referenced

### Workspace keeps reloading

If the workspace seems to reload frequently:
1. Avoid modifying project files while ULSM is running
2. Check for external tools regenerating csproj files

---

## FAQ

**Q: Does ULSM require Unity to be installed?**

A: No. ULSM analyzes the code files without running Unity. However, having the .NET Framework reference assemblies helps with full type resolution.

**Q: Can I use ULSM with older Unity versions?**

A: Yes. ULSM supports Unity's legacy csproj format used by all recent Unity versions. The Unity-specific analyzers and migration rules are optimized for Unity 6.x but work with older versions.

**Q: Why aren't all Unity diagnostics showing?**

A: Some diagnostics require the actual Unity DLLs to be resolvable. Without them, the analyzer may not recognize certain Unity types. Set `ULSM_FRAMEWORK_PATH` for better results.

**Q: Can I add custom analyzers?**

A: Currently, ULSM includes Microsoft.Unity.Analyzers and custom ULSM patterns. Adding custom analyzers would require source code modifications.

**Q: Does ULSM work with Assembly Definitions (asmdef)?**

A: Yes. ULSM loads all projects in the solution, including those generated from asmdef files.

**Q: How do I analyze only specific projects?**

A: Use the project-specific parameters in tools like `ulsm:unity_diagnostics` to target specific projects.

**Q: What's the difference between MSBuildWorkspace and AdhocWorkspace?**

A: MSBuildWorkspace uses MSBuild to fully evaluate project files, providing better fidelity. AdhocWorkspace parses csproj files directly, which is faster but may miss some configuration. Use `ULSM_FORCE_ADHOC=true` if MSBuild fails.

**Q: How do I report a bug or request a feature?**

A: Open an issue on the [GitHub repository](https://github.com/prespective/ulsm/issues).

**Q: Can ULSM modify my code?**

A: ULSM provides analysis and refactoring suggestions, but code modifications are applied by the MCP client (e.g., Claude Code) based on your approval.

**Q: Is my code sent anywhere?**

A: ULSM runs locally on your machine. Code is analyzed locally and only the analysis results (not source code) are returned to the MCP client.
