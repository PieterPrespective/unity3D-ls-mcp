# PP13-107 Phase 5 Checklist

## Quick Reference for Documentation and Polish Implementation

### Pre-Flight Checks

- [ ] Dev diary branch is `ULSM-PWS`
- [ ] Knowledge agent branch is `ULSM-PWS`
- [ ] `dotnet build ULSM.sln` succeeds
- [ ] `dotnet test ULSM.sln` passes
- [ ] PP13-106 commits present in git log
- [ ] Create registry entry and plan in dev diary

---

## Documentation Files

### README.md (Complete Rewrite)
- [ ] Replace entire file with ULSM-focused content
- [ ] Include sections:
  - [ ] Project badges (NuGet, .NET, Unity, License)
  - [ ] "Why ULSM?" comparison table
  - [ ] Features (Standard + Unity-specific)
  - [ ] Quick Start guide
  - [ ] Environment Variables table
  - [ ] Tools Reference (22+ tools organized by category)
  - [ ] Unity Diagnostics Reference (UNT + ULSM rules)
  - [ ] Example Prompts for Unity analysis
  - [ ] Architecture diagram
  - [ ] Troubleshooting section
  - [ ] MCP Client Configuration examples
  - [ ] Building from Source
  - [ ] Credits and License
- [ ] No references to "Roslyn MCP Server" or "dotnet-roslyn-mcp"

### CHANGELOG.md (Create)
- [ ] Version 1.0.0 entry with date
- [ ] Added section: Unity Project Support
- [ ] Added section: Unity Analysis Tools
- [ ] Added section: Testing Infrastructure
- [ ] Changed section: Rebranding details
- [ ] Attribution note

### LICENSE (Update)
- [ ] Add Prespective copyright (2026)
- [ ] Preserve Brendan Kowitz copyright (2025)
- [ ] Keep MIT license text

### CONTRIBUTING.md (Create)
- [ ] Development setup instructions
- [ ] Code style guidelines
- [ ] Pull request process
- [ ] Commit message format
- [ ] Issue reporting guidance

### Documentation/UserGuide.md (Create)
- [ ] Table of contents
- [ ] Introduction and key capabilities
- [ ] Installation instructions
- [ ] Configuration guide
- [ ] Unity project setup
- [ ] Tool reference (detailed)
- [ ] Troubleshooting guide
- [ ] FAQ

---

## Package Metadata

### src/ULSM.csproj
- [ ] PackAsTool = true
- [ ] ToolCommandName = ulsm
- [ ] PackageId = ulsm
- [ ] Version = 1.0.0
- [ ] Authors = Prespective
- [ ] Description (includes Unity-specific features)
- [ ] PackageTags (mcp, roslyn, unity, unity3d, csharp, etc.)
- [ ] PackageReadmeFile = README.md
- [ ] RepositoryUrl set

### src/server.json
- [ ] Verify all 22+ tools listed
- [ ] Verify Unity Analysis category (4 tools)
- [ ] Verify all environment variables documented

---

## Validation

### Build & Test
- [ ] `dotnet build ULSM.sln -c Release` succeeds
- [ ] `dotnet test ULSM.sln` all tests pass
- [ ] `dotnet pack src/ULSM.csproj -c Release` creates package

### Content Check
- [ ] Search for "RoslynMcp" → should find 0 in user-facing docs
- [ ] Search for "roslyn:" → should be "ulsm:" everywhere
- [ ] Search for "ROSLYN_" → should be "ULSM_" in docs
- [ ] All code examples tested

---

## Git Commits

1. [ ] `docs: rewrite README for ULSM`
2. [ ] `docs: add CHANGELOG documenting fork changes`
3. [ ] `docs: update LICENSE with dual attribution`
4. [ ] `docs: add CONTRIBUTING guide and User Guide`
5. [ ] `chore: update NuGet package metadata for release`
6. [ ] `docs: add PP13-107 Phase 5 assignment prompt`

---

## Dev Diary

- [ ] Create registry entry for PP13-107
- [ ] Log plan entry at start
- [ ] Log work entry on completion
- [ ] Commit psdd changes

---

## Success Criteria Summary

| Requirement | Met? |
|-------------|------|
| README.md complete rewrite | [ ] |
| CHANGELOG.md created | [ ] |
| LICENSE updated | [ ] |
| CONTRIBUTING.md created | [ ] |
| UserGuide.md created | [ ] |
| Package metadata complete | [ ] |
| Build succeeds | [ ] |
| Tests pass | [ ] |
| No old branding in docs | [ ] |
