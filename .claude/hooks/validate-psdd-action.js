#!/usr/bin/env node
/**
 * Validates PSDD (Dev Diary Database / Embranch) tool actions before execution
 * Called by PreToolUse hook - receives input via stdin as JSON
 * Exit 0 to allow, exit 2 to block with error message
 *
 * Validation rules:
 * 1. AddDocuments to 'dev-diary' must include issue_id and entry_type in metadata
 * 2. Cannot delete the 'registry' collection
 * 3. entry_type must be one of the allowed values
 */

const VALID_ENTRY_TYPES = ['plan', 'work', 'bug', 'issue', 'challenge', 'learning', 'test', 'review'];
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

    // Rule 1: Block deletion of protected collections
    if (toolName.includes('DeleteCollection')) {
      const collection = toolInput.collectionName || toolInput.collection_name;
      if (PROTECTED_COLLECTIONS.includes(collection)) {
        console.error(`BLOCKED: Cannot delete protected collection '${collection}'`);
        process.exit(2);
      }
    }

    // Rule 2: Validate AddDocuments to dev-diary has required metadata
    if (toolName.includes('AddDocuments')) {
      const collection = toolInput.collectionName || toolInput.collection_name;

      if (collection === 'dev-diary' && toolInput.metadatasJson) {
        try {
          const metadatas = JSON.parse(toolInput.metadatasJson);

          for (let i = 0; i < metadatas.length; i++) {
            const meta = metadatas[i];

            // Check required field: issue_id
            if (!meta.issue_id) {
              console.error(`BLOCKED: AddDocuments to 'dev-diary' requires 'issue_id' in metadata (document index ${i})`);
              process.exit(2);
            }

            // Check required field: entry_type
            if (!meta.entry_type) {
              console.error(`BLOCKED: AddDocuments to 'dev-diary' requires 'entry_type' in metadata (document index ${i})`);
              process.exit(2);
            }

            // Check entry_type is valid
            if (!VALID_ENTRY_TYPES.includes(meta.entry_type)) {
              console.error(`BLOCKED: Invalid entry_type '${meta.entry_type}'. Must be one of: ${VALID_ENTRY_TYPES.join(', ')}`);
              process.exit(2);
            }
          }
        } catch (parseErr) {
          // If metadatasJson can't be parsed, block with helpful message
          console.error('BLOCKED: Cannot parse metadatasJson — required for dev-diary AddDocuments');
          process.exit(2);
        }
      }

      // Also block AddDocuments to dev-diary without any metadata
      if (collection === 'dev-diary' && !toolInput.metadatasJson) {
        console.error('BLOCKED: AddDocuments to \'dev-diary\' requires metadatasJson with issue_id and entry_type');
        process.exit(2);
      }
    }

    // Rule 3: ExecuteImport must target staging collection only
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
