# Git convention

- **Branch naming:** `users/{ntid}/feat/AkkuSync-{StoryOrArea}`.
- **Merge model:** stacked feature branches, merged into each other with real merge commits (e.g. US6 → US7 → a realignment branch), then PR-merged into `main` at milestones. See [branch-stack.md](branch-stack.md) for the current stack order and how to propagate a PR-comment fix through it.
- **Commit prefixes:** `feat:`, `fix:`, `chore:`.
- **Commit/push timing:** never commit or push proactively — not at a "logical checkpoint," not because the branch happens to build cleanly. Only commit or push when explicitly asked, and only what was asked for.
- **No AI/Claude attribution, anywhere:** never add a `Co-Authored-By: Claude ...` trailer (or any other AI-authorship line) to a commit. Never mention Claude/AI assistance in commit messages, PR titles/descriptions, code comments, or committed docs — this repo's history and artifacts read as authored by the person committing them, full stop.
- **PR title:** a descriptive summary, e.g. `AccuSync | Extract Application and Core layer - Remove Old AccuSync`. No ticket ID in the title.
- **PR description** — always use this exact 4-section shape:

  ```
  # 1. Justification

  - Justify why this change is needed. What problem does it solve? What is the business value?
  - Reference the related Jira work item(s) or issue(s).

  # 2. Implementation

  - Describe the technical approach, key changes, and important design decisions.
  - List the main files/components modified.
  - Add any additional information that reviewers should know.

  # 3. Testing

  - Attach before-and-after screenshots/GIFs for UI changes.
  - Specify the type of testing performed (unit, integration, system, manual, etc.) to validate the changes.
  - Mention any known limitations or areas that were not tested.

  # 4. Checklist

  - [ ] Code follows project coding standards
  - [ ] Self-review of code completed
  - [ ] Code is properly commented, especially complex areas
  - [ ] Resources are added/updated for changes to user-visible strings
  - [ ] Documentation updated as needed
  - [ ] No new warnings or errors introduced
  - [ ] Manual testing completed and passed
  - [ ] Automation tests added/updated and passing
  ```

  Within this shape: reference ticket IDs and HLD links freely (a PR description isn't a
  permanent artifact the way code or an HLD is); call out review-driven design decisions under
  Implementation (e.g. "per review, X was removed since Y"); in Testing, state plainly what was
  NOT (re-)verified this round and why that's acceptable — never imply full coverage that wasn't
  actually run; only check a Checklist box that's literally true, leave the rest unchecked with a
  short note rather than checking something aspirational.
