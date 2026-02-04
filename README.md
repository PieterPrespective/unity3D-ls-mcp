# ULSM - Unity Language Server MCP

[![NuGet Tool](https://img.shields.io/badge/.NET%20Tool-Install-blue?logo=nuget)](https://www.nuget.org/packages/ulsm)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Unity 6](https://img.shields.io/badge/Unity-6.x-000000?logo=unity)](https://unity.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A Model Context Protocol (MCP) server providing Roslyn-based semantic code analysis for **Unity 6.x projects**. ULSM extends [dotnet-roslyn-mcp](https://github.com/brendankowitz/dotnet-roslyn-mcp) with Unity-aware workspace loading, Unity-specific analyzers, and API migration checking.

**22+ powerful tools** including Unity diagnostics, pattern analysis, API migration checks, impact analysis, safe refactoring, and dependency visualization!

## Why ULSM?

Unity projects have unique challenges that standard Roslyn tools don't handle well:

| Challenge | ULSM Solution |
|-----------|---------------|
| Unity's legacy .csproj format (ToolsVersion 4.0) | Automatic MSBuild configuration |
| Absolute HintPath references to Unity DLLs | Framework path resolution + fallback parsing |
| Unity-specific anti-patterns (GetComponent in Update) | Microsoft.Unity.Analyzers + custom pattern detection |
| API deprecations in Unity 6.x | 30+ migration rules with fix suggestions |

## Features

### Standard Roslyn Capabilities
- **Semantic Analysis**: 100% compiler-accurate code understanding
- **Cross-Solution Navigation**: Find references, implementations, callers, and type hierarchies
- **Impact Analysis**: See what code calls your methods before refactoring
- **Safe Refactoring**: Rename symbols across solution with preview mode
- **Dead Code Detection**: Find unused types, methods, and fields
- **Real-time Diagnostics**: Get compilation errors and warnings

### Unity-Specific Enhancements
- **Unity Project Detection**: Automatically detects Unity solutions by folder structure
- **Unity Diagnostics**: 50+ rules from Microsoft.Unity.Analyzers (UNT0001-UNT0xxx)
- **Hot Path Analysis**: Custom ULSM rules for GetComponent in Update, Camera.main, etc.
- **API Migration**: Check for deprecated Unity APIs with suggested replacements
- **Legacy Format Support**: Handles Unity's ToolsVersion 4.0 csproj files

## Quick Start

### Prerequisites

- .NET 8.0 SDK or Runtime
- (Optional) .NET Framework 4.7.1 Reference Assemblies (for full Unity DLL resolution)

### Installation

```bash
# Install as global tool
dotnet tool install --global ulsm

# Verify installation
ulsm --version
```

### Configure Claude Code

```bash
# Add ULSM to Claude Code
claude mcp add --transport stdio ulsm \
  --env DOTNET_SOLUTION_PATH="/path/to/YourUnityProject.sln" \
  -- ulsm
```

For Unity projects, you may also want to set the framework path:

```bash
claude mcp add --transport stdio ulsm \
  --env DOTNET_SOLUTION_PATH="/path/to/YourUnityProject.sln" \
  --env ULSM_FRAMEWORK_PATH="C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.1" \
  -- ulsm
```

## Configuration

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `DOTNET_SOLUTION_PATH` | (Required) | Path to .sln file or directory containing it |
| `ULSM_LOG_LEVEL` | Information | Logging level (Debug, Information, Warning, Error) |
| `ULSM_MAX_DIAGNOSTICS` | 100 | Maximum diagnostics to return per request |
| `ULSM_TIMEOUT_SECONDS` | 30 | Operation timeout (increase for large solutions) |
| `ULSM_FRAMEWORK_PATH` | (Auto-detect) | Override path to .NET Framework 4.7.1 reference assemblies |
| `ULSM_FORCE_ADHOC` | false | Force AdhocWorkspace instead of MSBuildWorkspace |
| `UNITY_EDITOR_PATH` | (Auto-detect) | Path to Unity Editor installation for reference resolution |

### Path Resolution

`DOTNET_SOLUTION_PATH` supports three path formats:

| Format | Example | Notes |
|--------|---------|-------|
| Absolute | `C:/Projects/MyGame/MyGame.sln` | Always works reliably |
| Variable | `${workspaceFolder}/MyGame.sln` | Expanded by MCP client (recommended for portability) |
| Relative | `./MyGame.sln` | Resolved against process working directory |

**Recommended:** Use `${workspaceFolder}` in configuration files for portable, reliable path resolution:

```json
{
  "env": {
    "DOTNET_SOLUTION_PATH": "${workspaceFolder}/MyGame.sln"
  }
}
```

Relative paths (e.g., `./MyGame.sln`) work but are resolved against the MCP server's working directory, which may vary depending on how the server is launched.

### Framework Path Locations

ULSM automatically searches for .NET Framework 4.7.1 reference assemblies:

**Windows:**
- `C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.1`
- `C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8`

**macOS:**
- `/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.7.1-api`

**Linux:**
- `/usr/lib/mono/4.7.1-api`

If these paths don't exist on your system, set `ULSM_FRAMEWORK_PATH` manually or install the .NET Framework Targeting Pack.

## Available Tools (22 Total)

### Core & Health
| Tool | Description |
|------|-------------|
| `ulsm:health_check` | Check server health and workspace status (includes Unity detection info) |
| `ulsm:load_solution` | Load a .NET/Unity solution for analysis |
| `ulsm:get_symbol_info` | Get detailed semantic information about a symbol |

### Navigation
| Tool | Description |
|------|-------------|
| `ulsm:go_to_definition` | Navigate to symbol definition |
| `ulsm:find_references` | Find all references to a symbol |
| `ulsm:find_implementations` | Find all implementations of an interface/abstract class |
| `ulsm:find_callers` | Find all methods that call a specific method (impact analysis) |
| `ulsm:get_type_hierarchy` | Get inheritance hierarchy for a type |

### Search
| Tool | Description |
|------|-------------|
| `ulsm:search_symbols` | Search for symbols by name across solution |
| `ulsm:semantic_query` | Natural language semantic search |

### Diagnostics
| Tool | Description |
|------|-------------|
| `ulsm:get_diagnostics` | Get compiler errors and warnings |
| `ulsm:get_code_fixes` | Get available code fixes for diagnostics |
| `ulsm:apply_code_fix` | Apply a specific code fix |

### Structure
| Tool | Description |
|------|-------------|
| `ulsm:get_project_structure` | Get solution/project structure |
| `ulsm:get_method_overloads` | Get all overloads of a method |
| `ulsm:get_containing_member` | Get containing method/property/class info |
| `ulsm:dependency_graph` | Visualize project dependencies and detect cycles |

### Refactoring
| Tool | Description |
|------|-------------|
| `ulsm:organize_usings` | Sort and remove unused using directives |
| `ulsm:organize_usings_batch` | Organize usings across multiple files |
| `ulsm:format_document_batch` | Format multiple documents |
| `ulsm:rename_symbol` | Safely rename symbol across solution with preview |
| `ulsm:extract_interface` | Generate interface from class for DI/testability |

### Analysis
| Tool | Description |
|------|-------------|
| `ulsm:find_unused_code` | Find dead code (unused types, methods, fields) |

### Unity Analysis (New in ULSM)
| Tool | Description |
|------|-------------|
| `ulsm:unity_diagnostics` | Get Unity-specific diagnostics (UNT0001, UNT0002, etc.) |
| `ulsm:check_unity_patterns` | Check for Unity anti-patterns (ULSM0001-0004) |
| `ulsm:api_migration` | Check for deprecated Unity APIs with migration suggestions |
| `ulsm:list_unity_rules` | List all available Unity diagnostic rules |

## Unity Diagnostics Reference

### Microsoft.Unity.Analyzers (UNT Rules)

| ID | Description | Severity |
|----|-------------|----------|
| UNT0001 | Empty Unity message (Update, Start, etc.) | Warning |
| UNT0002 | Inefficient tag comparison (use CompareTag) | Warning |
| UNT0003 | Usage of non-generic GetComponent | Info |
| UNT0006 | Incorrect message signature | Error |
| UNT0010 | MonoBehaviour instance created with new | Warning |
| UNT0014 | Invalid method for GetComponent | Error |
| UNT0022 | Unity objects should not use null coalescing | Warning |
| UNT0024 | Prefer float math over double | Info |

[Full list available via `ulsm:list_unity_rules`]

### ULSM Custom Patterns (ULSM Rules)

| ID | Description | Severity |
|----|-------------|----------|
| ULSM0001 | Expensive call in hot path (GetComponent in Update) | Warning |
| ULSM0002 | String operation in hot path (allocation) | Warning |
| ULSM0003 | Debug.Log in hot path (performance) | Info |
| ULSM0004 | Camera.main in hot path (expensive property) | Warning |

### API Migration Categories

| Category | Example Deprecations |
|----------|---------------------|
| Input | `Input.GetAxis` -> Input System |
| Networking | `NetworkBehaviour` -> Netcode for GameObjects |
| Rendering | `Camera.stereoActiveEye` -> XR APIs |
| XR | `XRSettings` -> XR Management |
| Physics | Various query method updates |

## Example Prompts

### Unity-Specific Analysis

```
"Check my PlayerController for Unity anti-patterns"
"What Unity APIs in my project are deprecated in Unity 6?"
"Find all GetComponent calls in Update methods"
"List all Unity diagnostics in the Assembly-CSharp project"
```

### Impact Analysis

```
"Find all callers of the ProcessInput method"
"What code will break if I change this method signature?"
"Who uses the PlayerStats class?"
```

### Safe Refactoring

```
"Preview renaming IWeaponHandler to IWeapon"
"Safely rename ProcessInput to HandleInput across the solution"
```

### Dead Code Detection

```
"Find all unused code in the Scripts folder"
"What private methods are never called?"
"Show me unused ScriptableObjects"
```

## Architecture

```
+-------------------------------------------------+
|         Claude Code (MCP Client)                |
+------------------------+------------------------+
                         | stdin/stdout (JSON-RPC 2.0)
+------------------------v------------------------+
|              McpServer.cs                       |
|  - Protocol handling                            |
|  - Tool registration (22+ tools)                |
|  - Request routing                              |
+------------------------+------------------------+
                         |
+------------------------v------------------------+
|            RoslynService.cs                     |
|  - Solution management                          |
|  - Semantic analysis                            |
|  - Symbol resolution                            |
|  - Unity project detection                      |
+------------------------+------------------------+
                         |
          +--------------+--------------+
          |                             |
+---------v---------+   +---------------v-----------------------+
| MSBuildWork-      |   |    Unity/                             |
| space             |   |  +- UnityProjectDetector              |
| (Standard)        |   |  +- UnityWorkspaceLoader              |
|                   |   |  +- UnityAnalysisService              |
|                   |   |  +- Analyzers/                        |
|                   |   |      +- UnityAnalyzerLoader           |
|                   |   |      +- UnityPatternAnalyzer          |
|                   |   |      +- UnityApiMigrationData         |
+-------------------+   +---------------------------------------+
```

## Troubleshooting

### Unity Project Not Detected

**Symptom:** Health check shows `isUnityProject: false` for a Unity project.

**Causes and Solutions:**

1. **No Assembly-CSharp.csproj**: Regenerate project files in Unity (Edit -> Preferences -> External Tools -> Regenerate Project Files)

2. **Missing folder structure**: ULSM looks for `Assets/` and `ProjectSettings/` folders

3. **Path resolution issues**: If using relative paths, ensure they resolve correctly. Use `${workspaceFolder}/YourProject.sln` for reliable resolution, or use absolute paths

### Framework Reference Errors

**Symptom:** Many CS0012 errors about missing types like `System.Object`.

**Solution:** Set `ULSM_FRAMEWORK_PATH` to your .NET Framework 4.7.1 reference assemblies:

```bash
# Windows
ULSM_FRAMEWORK_PATH="C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.1"

# macOS (with Mono)
ULSM_FRAMEWORK_PATH="/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.7.1-api"
```

### MSBuild Workspace Failures

**Symptom:** Solution fails to load with MSBuild errors.

**Solution:** Try forcing AdhocWorkspace mode:

```bash
ULSM_FORCE_ADHOC=true
```

This bypasses MSBuild and parses .csproj files directly. You may lose some features but gain compatibility.

### Large Solution Timeout

**Symptom:** Loading times out for large Unity projects.

**Solution:** Increase the timeout:

```bash
ULSM_TIMEOUT_SECONDS=120
```

### Unity Analyzers Not Loading

**Symptom:** `ulsm:list_unity_rules` returns empty or `ulsm:unity_diagnostics` finds nothing.

**Solution:** Ensure the solution is loaded first:

1. Call `ulsm:load_solution` if not auto-loaded
2. Check `ulsm:health_check` shows "Ready" status
3. Verify `unityAnalyzersLoaded: true` in health check response

## Building from Source

```bash
# Clone the repository
git clone https://github.com/prespective/ulsm.git
cd ulsm

# Build
dotnet build ULSM.sln -c Release

# Run tests
dotnet test ULSM.sln

# Pack as global tool
dotnet pack src/ULSM.csproj -c Release

# Install locally
dotnet tool install --global --add-source ./src/bin/Release ulsm
```

### Running from Source (Development)

```bash
claude mcp add --transport stdio ulsm-dev \
  --env DOTNET_SOLUTION_PATH="/path/to/YourUnityProject.sln" \
  -- dotnet run --project /path/to/ulsm/src/ULSM.csproj
```

## MCP Client Configuration Examples

### Claude Code (Recommended)

**~/.claude/mcp_servers.json:**

```json
{
  "servers": {
    "ulsm": {
      "command": "ulsm",
      "args": [],
      "env": {
        "DOTNET_SOLUTION_PATH": "C:/Projects/MyUnityGame/MyUnityGame.sln",
        "ULSM_FRAMEWORK_PATH": "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.1",
        "ULSM_LOG_LEVEL": "Information"
      }
    }
  }
}
```

### Project-Local Configuration

**.claude/mcp-spec.json** (in your Unity project root):

```json
{
  "mcpServers": {
    "ulsm": {
      "command": "ulsm",
      "env": {
        "DOTNET_SOLUTION_PATH": "${workspaceFolder}/MyGame.sln"
      }
    }
  }
}
```

### VS Code (with MCP extension)

**.vscode/settings.json:**

```json
{
  "mcp.servers": {
    "ulsm": {
      "command": "ulsm",
      "env": {
        "DOTNET_SOLUTION_PATH": "${workspaceFolder}/MyGame.sln"
      }
    }
  }
}
```

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Setup

1. Fork and clone the repository
2. Open `ULSM.sln` in your IDE
3. Run tests: `dotnet test`
4. Make changes
5. Submit a pull request

## Credits

ULSM is a fork of [dotnet-roslyn-mcp](https://github.com/brendankowitz/dotnet-roslyn-mcp) by Brendan Kowitz, extended with Unity-specific capabilities by Prespective.

### Key Dependencies

- [Microsoft.CodeAnalysis (Roslyn)](https://github.com/dotnet/roslyn) - Compiler APIs
- [Microsoft.Unity.Analyzers](https://github.com/microsoft/Microsoft.Unity.Analyzers) - Unity-specific diagnostics
- [ModelContextProtocol](https://github.com/anthropics/model-context-protocol) - MCP SDK

## License

MIT License - See [LICENSE](LICENSE) for details.

---

*Built with Roslyn for Unity developers who want AI-powered code assistance that truly understands their codebase.*
