# PP13-103 Tool Name Mapping Reference

Quick reference for all tool name changes from `roslyn:` to `ulsm:` prefix.

## Complete Tool Mapping

| # | Current Name | New Name |
|---|--------------|----------|
| 1 | `roslyn:health_check` | `ulsm:health_check` |
| 2 | `roslyn:load_solution` | `ulsm:load_solution` |
| 3 | `roslyn:get_symbol_info` | `ulsm:get_symbol_info` |
| 4 | `roslyn:go_to_definition` | `ulsm:go_to_definition` |
| 5 | `roslyn:find_references` | `ulsm:find_references` |
| 6 | `roslyn:find_implementations` | `ulsm:find_implementations` |
| 7 | `roslyn:get_type_hierarchy` | `ulsm:get_type_hierarchy` |
| 8 | `roslyn:search_symbols` | `ulsm:search_symbols` |
| 9 | `roslyn:semantic_query` | `ulsm:semantic_query` |
| 10 | `roslyn:get_diagnostics` | `ulsm:get_diagnostics` |
| 11 | `roslyn:get_code_fixes` | `ulsm:get_code_fixes` |
| 12 | `roslyn:apply_code_fix` | `ulsm:apply_code_fix` |
| 13 | `roslyn:get_project_structure` | `ulsm:get_project_structure` |
| 14 | `roslyn:organize_usings` | `ulsm:organize_usings` |
| 15 | `roslyn:organize_usings_batch` | `ulsm:organize_usings_batch` |
| 16 | `roslyn:format_document_batch` | `ulsm:format_document_batch` |
| 17 | `roslyn:get_method_overloads` | `ulsm:get_method_overloads` |
| 18 | `roslyn:get_containing_member` | `ulsm:get_containing_member` |
| 19 | `roslyn:find_callers` | `ulsm:find_callers` |
| 20 | `roslyn:find_unused_code` | `ulsm:find_unused_code` |
| 21 | `roslyn:rename_symbol` | `ulsm:rename_symbol` |
| 22 | `roslyn:extract_interface` | `ulsm:extract_interface` |
| 23 | `roslyn:dependency_graph` | `ulsm:dependency_graph` |

**Total: 23 tools**

## Tool Categories

### Navigation (7 tools)
- `ulsm:health_check`
- `ulsm:load_solution`
- `ulsm:get_symbol_info`
- `ulsm:go_to_definition`
- `ulsm:find_references`
- `ulsm:find_implementations`
- `ulsm:get_type_hierarchy`

### Search (2 tools)
- `ulsm:search_symbols`
- `ulsm:semantic_query`

### Diagnostics (3 tools)
- `ulsm:get_diagnostics`
- `ulsm:get_code_fixes`
- `ulsm:apply_code_fix`

### Structure (4 tools)
- `ulsm:get_project_structure`
- `ulsm:get_method_overloads`
- `ulsm:get_containing_member`
- `ulsm:dependency_graph`

### Refactoring (5 tools)
- `ulsm:organize_usings`
- `ulsm:organize_usings_batch`
- `ulsm:format_document_batch`
- `ulsm:rename_symbol`
- `ulsm:extract_interface`

### Analysis (2 tools)
- `ulsm:find_callers`
- `ulsm:find_unused_code`

## Regex for Find/Replace

### In McpServer.cs - HandleListToolsAsync
```
Find:    name = "roslyn:
Replace: name = "ulsm:
```

### In McpServer.cs - HandleToolCallAsync
```
Find:    "roslyn:
Replace: "ulsm:
```

### Expected Replacement Count
- `HandleListToolsAsync`: 23 replacements (tool definitions)
- `HandleToolCallAsync`: 23 replacements (switch cases)
- **Total**: 46 replacements

## Verification Commands

```bash
# Count roslyn: occurrences (should be 0 after changes)
grep -c '"roslyn:' src/McpServer.cs

# Count ulsm: occurrences (should be 46)
grep -c '"ulsm:' src/McpServer.cs

# List all tool names to verify
grep -o '"ulsm:[^"]*"' src/McpServer.cs | sort | uniq
```
