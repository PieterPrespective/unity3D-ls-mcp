# Contributing to ULSM

Thank you for your interest in contributing to ULSM! This document provides guidelines for contributing.

## Development Setup

1. **Prerequisites**
   - .NET 8.0 SDK
   - (Optional) .NET Framework 4.7.1 Targeting Pack for Unity workspace testing

2. **Clone and Build**
   ```bash
   git clone https://github.com/prespective/ulsm.git
   cd ulsm
   dotnet build ULSM.sln
   ```

3. **Run Tests**
   ```bash
   dotnet test ULSM.sln
   ```

## Code Style

- Follow existing code conventions in the codebase
- Add XML documentation comments to all public types and members
- Use `Assert.That` style for NUnit tests
- Keep processing logic separate from data structures (functional style preferred)

## Project Structure

```
src/
+-- ULSM.csproj          # Main project
+-- McpServer.cs         # MCP protocol handling
+-- RoslynService.cs     # Core Roslyn functionality
+-- Unity/               # Unity-specific code
    +-- UnityProjectDetector.cs
    +-- UnityWorkspaceLoader.cs
    +-- UnityAdhocWorkspaceBuilder.cs
    +-- UnityAnalysisService.cs
    +-- Analyzers/
        +-- UnityAnalyzerLoader.cs
        +-- UnityPatternAnalyzer.cs
        +-- UnityApiMigrationData.cs

tests/
+-- ULSM.Tests/          # Test project
+-- UnityTestProject/    # Minimal Unity project for testing
```

## Pull Request Process

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass: `dotnet test`
6. Commit with conventional commit format: `feat: add new feature`
7. Push and create a Pull Request

## Commit Message Format

We use [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` - New features
- `fix:` - Bug fixes
- `docs:` - Documentation changes
- `test:` - Test additions or changes
- `refactor:` - Code refactoring
- `chore:` - Maintenance tasks

Examples:
```
feat(unity): add pattern detection for Camera.main in Update
fix(workspace): handle missing ProjectVersion.txt gracefully
docs: update README with troubleshooting section
test: add integration tests for Unity workspace loading
```

## Adding New Unity Analyzers

To add a new custom analyzer pattern:

1. Add the pattern to `UnityPatternAnalyzer.cs`
2. Define the diagnostic ID (ULSM00XX format)
3. Implement the check logic
4. Add tests in `UnityPatternAnalyzerTests.cs`
5. Update documentation

## Adding New API Migration Rules

To add a new deprecated API rule:

1. Add the rule to `UnityApiMigrationData.cs`
2. Include: deprecated API, replacement, Unity version, category
3. Add tests in `UnityApiMigrationDataTests.cs`
4. Update documentation

## Reporting Issues

- Use GitHub Issues for bug reports and feature requests
- Include reproduction steps for bugs
- Include Unity version and .NET SDK version
- Provide relevant log output if available

## Questions?

Open a GitHub Discussion or Issue for any questions about contributing.
