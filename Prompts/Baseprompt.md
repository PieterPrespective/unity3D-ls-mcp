
# Project Variables

| Variable         | Value  | Notes                                          |
| ---------------- | ------ | ---------------------------------------------- |
| ProjectNamespace | ULSM | unity3d-ls-mcp; Default namespace for issues in this project |
| IssueID          | {ISSUE-ID}  | Current issue being worked on              |

This project is setup to develop a Language Server Protocol(LSP) mcp server for use with Unity3D - current solutions (Omnisharp, Roslyn LS and csharp-ls) all have drawbacks
NOTE : this project is a fork of the https://github.com/brendankowitz/dotnet-roslyn-mcp git

## Agent Architecture

This project uses two specialized sub-agents for persistent memory:

### Dev Diary Agent (`psdd`)
- **Purpose:** Per-issue development diary — plans, work logs, bugs, challenges, learnings
- **Invocation:** `/devdiary` skill or Task tool with dev-diary-agent prompt
- **Database:** `prespective-dev-diary` (ChromaDB + Dolt via Embranch)
- **Collections:** `registry` (issue index), `dev-diary` (all entries)
- **Interface:** Structured assignments

### Knowledge Agent (`pskd`)
- **Purpose:** Cross-project tool/workflow knowledge base
- **Invocation:** `/knowledge` skill or Task tool with knowledge-agent prompt
- **Database:** `prespective-knowledge` (ChromaDB + Dolt via Embranch)
- **Collections:** `registry` (tool index), `filtered_learnings` (knowledge), `learnings` (inbox)
- **Interface:** Natural language queries

### Responsibility Boundary
| Concern | Who Handles |
|---------|-------------|
| Diary CRUD (dev-diary, registry) | Dev Diary Agent (psdd) |
| Knowledge CRUD (learnings, registry, filtered_learnings) | Knowledge Agent (pskd) |
| Version control (commit, branch, push/pull) | Main Agent (you) |
| Learning offload (psdd → pskd) | Main Agent (you) |

## Software Architecture Rules
- Apply boyscout rule - leave all code better than how you found it - but only refactor if on the critical path of your current assignment
- Add Summaries to all classes and class members you create, make sure to make them descriptive. If you update the functioning of a class member update the summary and parameter descriptions
	- If you create any complex code sections within a member, be sure to add a short explanatory comment in the line above, or behind
- When architecting code structures (classes, structs), prefer to keep processing logic out of the data (apply functional programming whenever possible)
	- Create a data struct with just fields, properties and optionally a constructor and/or destructor
	- Create a static utility class (generally named [datastructname]Utility) that contains all processing logic functions as static functions
		- Be mindful of accessibility - if tools are only accessed locally mark them private, or internal if they are targeted by a (unit)test
	- If functions require upwards of 4 parameters to function, create a custom data struct to forward data to that function
- Whenever possible - create tests for the functions you create to validate that they work in an atomic fashion. 
	- Use NUnit for unit and integration testing - please place create the tests in a seperate project named {ProjectNamespace}Testing
	- please be descriptive in the test functions' summary on what exactly is tested. If possible, use test fixtures and cases for variant testing
	- Prefer using the Assert.That notation form - for examples on this format see: https://moleseyhill.com/2018-12-01-nunit-assert-that-examples.html

## Dev Diary Assignment Types

Use these structured commands when spawning the Dev Diary Agent:

### Search
```
ASSIGNMENT: Search
query: {search text}
max_tokens: 2000
```

### Get Details
```
ASSIGNMENT: Get Details
issue_id: {ISSUE-ID}
query: {optional search within issue}
entry_type: {optional: plan/work/bug/issue/challenge/learning/test/review}
```

### Create Registry Entry
```
ASSIGNMENT: Create Registry Entry
issue_id: {ISSUE-ID}
title: {title}
summary: {description}
related_issues: {comma-separated IDs}
```

### Create Diary Entry
```
ASSIGNMENT: Create Diary Entry
issue_id: {ISSUE-ID}
entry_type: {plan/work/bug/issue/challenge/learning/test/review}
content: {entry content}
```

### Get Learnings for Offload
```
ASSIGNMENT: Get Learnings for Offload
issue_id: {ISSUE-ID}
```

## Task Execution Flow

### Before Starting Work
0. **Check Knowledge- and Dev Diary Agent Active Dolt Branches are work branches**
   - Both should be on a branch that at least identifies 
		- The project being worked on (generally ProjectNamespace)
		- The user working on the project (3 letter summary, e.g. PWS)
		- (Optionally) the system instance being worked on
   - If we're not on any specific custom branch (e.g. 'main') request the user to specify it (suggest a formatting based on the 3 indentifiers above)
   - Don't directly work on the main branch, unless explicitly specified by the user
1. **Check Knowledge Agent** for relevant context:
   - `/knowledge what do we know about {topic related to current issue}?`
2. **Check Dev Diary** for previous work on this or related issues:
   - `/devdiary ASSIGNMENT: Search\nquery: {current issue topic}`
   - `/devdiary ASSIGNMENT: Get Details\nissue_id: {IssueID}`
3. **Create or verify registry entry** for current issue:
   - If no registry entry exists, the first diary entry will auto-create one
4. **Log planned approach:**
   - `/devdiary ASSIGNMENT: Create Diary Entry\nissue_id: {IssueID}\nentry_type: plan\ncontent: {planned approach}`

### During Work
5. **Log significant work** at natural breakpoints:
   - `/devdiary ASSIGNMENT: Create Diary Entry\nissue_id: {IssueID}\nentry_type: work\ncontent: {what was done, and why}`
6. **Log bugs discovered:**
   - `/devdiary ASSIGNMENT: Create Diary Entry\nissue_id: {IssueID}\nentry_type: bug\ncontent: {symptoms, cause, resolution}`
7. **Log challenges:**
   - `/devdiary ASSIGNMENT: Create Diary Entry\nissue_id: {IssueID}\nentry_type: challenge\ncontent: {problem, attempts, solution}`
8. **Log learnings immediately:**
   - `/devdiary ASSIGNMENT: Create Diary Entry\nissue_id: {IssueID}\nentry_type: learning\ncontent: {insight or technique discovered}`

### When Completing Work
9. **c# project validation:**
    - run dotnet test on relevant tests for the development activity using bash
	
10. **Log final work summary:**
    - `/devdiary ASSIGNMENT: Create Diary Entry\nissue_id: {IssueID}\nentry_type: work\ncontent: {final summary of changes}`
11. **Offload learnings to Knowledge Agent:**
    - `/devdiary ASSIGNMENT: Get Learnings for Offload\nissue_id: {IssueID}`
    - Review the returned learnings
    - Forward relevant ones: `/knowledge save: {learning content}`
13. **Commit both databases:**
    - `mcp__psdd__DoltCommit({ message: "{IssueID}: {summary of diary entries}" })`
    - `mcp__pskd__DoltCommit({ message: "Added learnings from {IssueID}" })` (if offloaded)

## Logging Rules
- Store data within the chroma databases in chunks of maximally 512-1024 tokens
- All psdd operations are logged to `dev-diary-agent-log.txt` via hooks
- All pskd operations are logged to `prespective-knowledge-agent-log.txt` via hooks
- Hooks enforce metadata validation on `dev-diary` AddDocuments (require `issue_id` and `entry_type`)
- Store and read data from the chroma database they belong to - do not store data in other databases:
	- Development diary data (plans, work logs, bugs, challenges, learnings per issue) → `psdd`
	- Cross-project tool/workflow knowledge → `pskd`
	- If you do not know where data should go — please EXPLICITLY ask the user, do not make the decision yourself

## Version Control Rules
- Commit diary entries at the end of each work session or milestone
- Commit knowledge changes after processing offloaded learnings
- Never commit from sub-agents — only the main agent commits
- Branch naming: Follow project conventions

