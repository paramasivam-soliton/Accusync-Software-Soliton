# Branch stack convention

The single source of truth for "what sits on top of what" in this repo's stacked-branch
workflow. Read this **before** resolving a PR comment on any branch, so the fix gets propagated
(normal `git merge` + normal `git push`, never force) to every branch stacked on top of it —
missing one means that branch's PR silently ships without the fix.

**This file is a snapshot, not a live query.** The stack changes shape over time (branches merge to
`main`, new branches get inserted, branches get reordered — see the 2026-09 Presentation
Realignment/Logging/Patient reorder). Before trusting it, re-verify with:

```
git fetch origin main
git merge-base --is-ancestor users/chokkalingam/feat/AkkuSync-<parent> users/chokkalingam/feat/AkkuSync-<child>
```

for the specific link you're about to act on. If it disagrees with this file, the repo is right and
this file is stale — fix this file in the same PR that changes the stack.

## Closed — already merged into `main`, no propagation needed

These landed on `main` as squash-merged PRs (#1–#5). They are not part of the active stack; a PR
comment on work this old doesn't apply here.

1. `BaseProject-AkkuSync-View` — PR1, View layer extraction
2. `AccuSync-Presentation-ViewModel` — PR2, ViewModel layer extraction
3. `AccuSync-Application-Persistence` — PR3, Application/Core layer extraction
4. `AccuSync-EFCorePeristence-SQLite` — PR4, EF Core migration
5. `AccuSync-DataParser` — PR5, DataParser (import parsing + QR generation) extraction

## Open — the active stack, in order

Everything below is still unmerged, branched directly or transitively off `AccuSync-DataParser`
(i.e., off the tip of the closed stack above). Each entry sits directly on top of the one before it.

| # | Branch | Epic / story |
|---|---|---|
| 6 | `US1-PasswordHashing` | Identity — password hashing |
| 7 | `US2-RoleBasedAccessControl` | Identity — RBAC |
| 8 | `US3-UserLogin` | Identity — login |
| 9 | `US4-AccountLockout` | Identity — account lockout |
| 10 | `US5-Logout` | Identity — logout |
| 11 | `LI-F1-US6-PasswordReset` | Login Feature 1 — password reset |
| 12 | `LI-F1-US7-DraggableClosableLogin` | Login Feature 1 — draggable/closable login window |
| 13 | `LI-F1-US8-ChangePasswordSettings` | Login Feature 1 — change password from Settings |
| 14 | `PresentationLayerRealignment` | Presentation-layer cleanup (Extract Models, `IApplicationLifecycle`) |
| 15 | `LG-F1-HLD` | Logging Feature 1 — HLD only, no code |
| 16 | `LG-F1-US1-LoggerInterfaceFileWriter` | Logging Feature 1 — `ILoggingService`/file writer |
| 17 | `LG-F1-US2-ReplaceDebugPrintCalls` | Logging Feature 1 — replace `Debug.WriteLine`/`Console.WriteLine` |
| 18 | `LG-F1-US3-Epic0Epic1Logging` | Logging Feature 1 — Epic 0/1 (startup/DI/DB-migration, login/logout/lockout) logging |
| 19 | `US6-PatientPersistenceLayer` | Patient Management — persistence-layer HLD |
| 20 | `US6-PatientPersistenceLayer-Implementation` | Patient Management — persistence-layer implementation |
| 21 | `US7-PatientTableRepo` | Patient Management — patient table + repository |
| 22 | `US8-PatientPresentationLayer` | Patient Management — presentation layer |
| 23 | `US9-AddPatient` | Patient Management — Add Patient (PatientId mapping, auto-refresh, Ctrl+S/Enter) |
| 24 | `CodeFormatting` | Cross-cutting — StyleCop cleanup (`.editorconfig`/`stylecop.json`, doc comments, member ordering) |

Note: Patient Management's own `US6`–`US9` numbering is a separate namespace from Identity's
`US1`–`US5` and Login Feature 1's `LI-F1-US6`–`US8` — matching numbers across these three groups
refer to unrelated stories. Always use the full branch name, never just the number, when talking
about "US6" or "US8" in this repo.

## How to apply

When a PR comment lands on branch *X* in the Open table above:

1. Fix it on *X* itself, commit.
2. Walk down the table from *X*'s row to the bottom, one row at a time: checkout the next branch,
   `git merge --no-commit --no-ff <previous-branch>`, resolve any conflicts, build, run the full test
   suite, commit, then move to the next row.
3. Push each updated branch normally (`git push`) — no force-push is needed for this kind of forward
   propagation, since every commit created this way is a genuine new commit on top of what's already
   pushed, not a history rewrite. Force-push is only for the rare case for actually reordering or
   rewriting this stack's shape (see [git.md](git.md)).
