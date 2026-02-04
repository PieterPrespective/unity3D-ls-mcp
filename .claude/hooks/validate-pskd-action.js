#!/usr/bin/env node
/**
 * Validates PSKD (Prespective Knowledge Database / Embranch) tool actions before execution
 * Called by PreToolUse hook - receives input via stdin as JSON
 * Exit 0 to allow, exit 2 to block with error message
 */

// Validation rules
const PROTECTED_COLLECTIONS = ['registry'];

// Read input from stdin
let inputData = '';
process.stdin.setEncoding('utf8');

process.stdin.on('data', (chunk) => {
  inputData += chunk;
});

process.stdin.on('end', () => {
  try {
    const input = JSON.parse(inputData);

    const toolName = input.tool_name || '';
    const toolInput = input.tool_input || {};

    // Block deletion of protected collections
    if (toolName.includes('DeleteCollection')) {
      const collection = toolInput.collectionName || toolInput.collection_name;
      if (PROTECTED_COLLECTIONS.includes(collection)) {
        console.error(`BLOCKED: Cannot delete protected collection '${collection}'`);
        process.exit(2); // Exit 2 = blocking error
      }
    }

    // Rule: ExecuteImport must target staging collection only
    if (toolName.includes('ExecuteImport')) {
      const filter = toolInput.filter;
      if (filter) {
        try {
          const filterObj = JSON.parse(filter);
          if (filterObj.collections) {
            for (const mapping of filterObj.collections) {
              const target = mapping.import_into;
              if (target && !target.startsWith('import-staging-')) {
                console.error(
                  `BLOCKED: ExecuteImport must target a staging collection (import-staging-*), not '${target}'`
                );
                process.exit(2);
              }
            }
          }
        } catch (e) {
          // If filter can't be parsed, allow (import may use default mapping)
        }
      }
    }

    // All checks passed - allow the operation
    console.log(JSON.stringify({ continue: true }));
    process.exit(0);

  } catch (e) {
    // On parse error, allow the operation (fail open)
    console.log(JSON.stringify({ continue: true }));
    process.exit(0);
  }
});

// Handle case where stdin closes immediately
process.stdin.on('error', () => {
  process.exit(0);
});
