# Feature-requirement convention

- Dev-time working docs (e.g. `FEATURE_REQUIREMENTS.md`) are for drafting only. Consult them freely while researching a feature, but never name or cite them inside an HLD or inside code — neither XML doc `<summary>` comments nor inline `//` comments. State the actual rationale instead of pointing at the working doc.
- HLDs live under `docs/design-documents/{Product}/*.md`, one per feature/story grouping (not per PR), following the existing structure:
  1. `Who` — author
  2. `Feature work item` — epic/story ticket IDs (this is their designated home)
  3. `Links to reference material` — SRS doc + clause IDs, target schema files
  4. `Implementation and design` — Problem statement → Implementation
- Ticket IDs and SRS clause numbers belong **only** in the HLD's own "Feature work item"/"Links" sections. Never let either leak into code or into a different document.
- Also never cite JIRA ticket IDs, "User Story"/epic names, or spec-doc section numbers inside `.cs` code — this applies repo-wide regardless of which branch or feature is being worked on. Explain the *why* in plain engineering terms (the constraint, the invariant, the tradeoff) instead.
