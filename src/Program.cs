using Microsoft.Build.Locator;
using ULSM;

// Register MSBuild before any Roslyn code runs
MSBuildLocator.RegisterDefaults();

// Create and run the MCP server
var server = new McpServer();
await server.RunAsync();
