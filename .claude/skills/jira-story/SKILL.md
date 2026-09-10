---
name: jira-story
description: Fetches full details (summary, status, priority, parent epic, and full description text) for one or more Jira issues in the ASWD project on natus-jira.atlassian.net — by issue key, by keyword/text search, or by raw JQL. Use this proactively any time a Jira issue key like ASWD-123 is mentioned; any time the user refers to "the story", "this epic", "the ticket", "acceptance criteria", or a User Story by name/number (e.g. "US9", "add patient"); and any time you are about to write, modify, review, or test code for a feature whose driving story's actual requirements aren't already in the conversation. Don't guess what a story says or rely on a stale summary from earlier in the chat — re-fetch it. Also useful for listing/skimming the ASWD backlog when the user hasn't named a specific issue yet.
---

# Jira story lookup

Pulls live issue data straight from the Jira Cloud REST API — not by scraping the board URL the
user might paste (`.../jira/software/c/projects/ASWD/list?jql=...`), which is a JS single-page app
behind login and won't render for a plain fetch. The API gives structured, reliable data instead.

## When to use this

- A message, branch name, file, or diff references a Jira key matching `[A-Z]{2,10}-\d+` (e.g.
  `ASWD-123`).
- The user says things like "the story", "this epic", "the ticket", "acceptance criteria", or names
  a story informally (e.g. "US9", "the add-patient story") without giving the literal key — this
  repo's branches are named `users/{ntid}/feat/AkkuSync-{StoryOrArea}` using a human label, not the
  literal Jira key, so the label and the key are usually different strings. Use `-Search` with the
  label/keywords to resolve it.
- You're about to implement, modify, review, or write tests for a feature and haven't actually seen
  that story's current description/acceptance criteria in this conversation yet. Fetch it first
  rather than working from assumption or a summary given earlier that may be stale.
- The user wants to skim what's in the backlog before picking a story.

## One-time setup (per developer)

1. Generate an Atlassian API token: https://id.atlassian.com/manage-profile/security/api-tokens
2. Copy [`.env.local.example`](.env.local.example) to `.env.local` in this same folder, and fill in
   `JIRA_EMAIL` (the Jira account's email) and `JIRA_API_TOKEN` (the token from step 1).
   `.env.local` is git-ignored — it is never committed, and each developer keeps their own.
3. Alternatively, set `JIRA_EMAIL` / `JIRA_API_TOKEN` / `JIRA_BASE_URL` as real environment
   variables (e.g. via `setx`) instead of using `.env.local` — either source works, env vars win if
   both are present.

If neither is configured, the script fails immediately with a clear message naming exactly what's
missing — surface that message to the user rather than retrying blindly.

## How to call it

Run via the PowerShell tool (this is a Windows environment):

```powershell
# One or more specific issues
./.claude/skills/jira-story/scripts/fetch-jira-issue.ps1 -Keys ASWD-101,ASWD-102

# Find an issue by keyword when you don't know its key
./.claude/skills/jira-story/scripts/fetch-jira-issue.ps1 -Search "add patient"

# Full control via raw JQL (e.g. all stories under an epic, or all epics)
./.claude/skills/jira-story/scripts/fetch-jira-issue.ps1 -Jql "project = ASWD AND issuetype = Epic"
./.claude/skills/jira-story/scripts/fetch-jira-issue.ps1 -Jql "parent = ASWD-50"

# No arguments: lists the ASWD backlog in the same order as the project's default board view
./.claude/skills/jira-story/scripts/fetch-jira-issue.ps1

# Cap how many issues come back (default 25)
./.claude/skills/jira-story/scripts/fetch-jira-issue.ps1 -Jql "project = ASWD" -MaxResults 50
```

`-Keys`, `-Search`, and `-Jql` are mutually exclusive — pick the one that fits. Prefer `-Keys` when
you already have the exact key(s); reach for `-Search` when the user only gave you a name/topic; use
`-Jql` for anything more structural (an epic's children, all issues of a type, etc.).

## Reading the output

Each issue prints as a Markdown block:

```
## ASWD-123 - Story: Add patient screen
- Status: In Progress
- Priority: Medium
- Assignee: Jane Doe
- Parent: ASWD-50 - Patient management epic
- Labels: presentation-layer

### Description
<full description text, converted from Jira's rich text>
```

Multiple issues are separated by `---`. Use the Description section as the actual source of
requirements/acceptance criteria — read it in full before starting implementation, don't skim just
the summary line.

## Using what you fetch — important guardrail

This repo's conventions ([feature-requirement.md](../../conventions/feature-requirement.md)) forbid
citing Jira ticket IDs, User Story/epic names, or spec-doc section numbers inside code, XML doc
`<summary>` comments, or HLDs. Fetching a story's content is for understanding *what to build* —
translate its requirements into plain engineering language in code, tests, and design docs. The key
(e.g. `ASWD-123`) itself belongs only in commit messages/PR descriptions, never inside a `.cs` file
or HLD body.

## Troubleshooting

- **401 error**: `JIRA_EMAIL` or `JIRA_API_TOKEN` is wrong, expired, or revoked — regenerate the
  token and update `.env.local`.
- **404 error**: wrong issue key, wrong `JIRA_BASE_URL`, or the account lacks access to that
  project/issue.
- **`-Search` returns nothing**: Jira's `text ~` search needs a real word/phrase match, not a loose
  paraphrase — try fewer/different keywords, or fall back to `-Jql` (e.g. browse by `issuetype` or
  `status`) and ask the user to confirm the right issue.
- The default no-argument listing sorts by `cf[10019]`, a custom field specific to this Jira
  instance's board — it's only meaningful for that default view, not for `-Search`/`-Jql` results.
