#!/usr/bin/env node
/**
 * Logs PSKD (Prespective Knowledge Database / Embranch) tool actions to the knowledge agent log file
 * Called by PostToolUse hook - receives input via stdin as JSON
 */

const fs = require('fs');
const path = require('path');

const LOG_FILE = 'prespective-knowledge-agent-log.txt';

// Read input from stdin
let inputData = '';
process.stdin.setEncoding('utf8');

process.stdin.on('data', (chunk) => {
  inputData += chunk;
});

process.stdin.on('end', () => {
  try {
    const input = JSON.parse(inputData);

    const toolName = input.tool_name || 'unknown';
    const toolInput = input.tool_input || {};
    const sessionId = input.session_id || 'unknown';
    const cwd = input.cwd || process.cwd();

    // Determine operation type
    function getOperationType(name) {
      if (name.includes('Add') || name.includes('Create')) return 'CREATE';
      if (name.includes('Query') || name.includes('Get') || name.includes('List') || name.includes('Peek')) return 'READ';
      if (name.includes('Update') || name.includes('Modify')) return 'UPDATE';
      if (name.includes('Delete')) return 'DELETE';
      if (name.includes('Preview')) return 'IMPORT_PREVIEW';
      if (name.includes('ExecuteImport')) return 'IMPORT_EXECUTE';
      if (name.includes('Dolt')) return 'VERSION';
      return 'OTHER';
    }

    // Get collection from input
    function getCollection(inp) {
      if (inp.filepath) return `import:${inp.filepath.split(/[\\\/]/).pop()}`;
      return inp.collectionName || inp.collection_name || 'system';
    }

    // Format timestamp
    function getTimestamp() {
      return new Date().toISOString().replace('T', ' ').slice(0, 19);
    }

    const operation = getOperationType(toolName);
    const collection = getCollection(toolInput);
    const timestamp = getTimestamp();
    const shortToolName = toolName.replace('mcp__pskd__', '');

    const inputStr = JSON.stringify(toolInput);
    const logEntry = `[${timestamp}] ${operation} ${collection}: ${shortToolName}
    Input: ${inputStr.slice(0, 200)}${inputStr.length > 200 ? '...' : ''}
    Session: ${sessionId.slice(0, 8)}
`;

    // Append to log file
    const logPath = path.join(cwd, LOG_FILE);
    fs.appendFileSync(logPath, logEntry);

    // Output success (will be shown in verbose mode)
    console.log(JSON.stringify({ continue: true }));
    process.exit(0);

  } catch (e) {
    // Non-blocking error - just log to stderr
    console.error('Hook error:', e.message);
    process.exit(0); // Exit 0 to not block the operation
  }
});

// Handle case where stdin closes immediately
process.stdin.on('error', () => {
  process.exit(0);
});
