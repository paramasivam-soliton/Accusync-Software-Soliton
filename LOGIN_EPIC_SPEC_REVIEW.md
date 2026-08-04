# Login Epic (ASWD-1) — Review Sheet

One line per point: what we plan to do, why, and whether it's **Decided**, an **Assumption**, or
a **Question** you need to answer. Full rationale/detail lives in `LOGIN_EPIC_SPEC.md` if you
want to go deeper on any row — this sheet is just for a fast pass.

**Legend:** ✅ Decided (going with this unless you object) · 🟡 Assumption (flag if wrong) · ❓ Question (blocking, need your answer)

---

## Scope — 5 PRs, in order

| # | Ticket | One-line scope |
|---|---|---|
| 1 | ASWD-31 | Hash passwords, fix username encryption. Foundational — everything below depends on this. |
| 2 | ASWD-36 | Add a Role (Admin/Screener) field; wire up nav-hiding by role. |
| 3 | ASWD-41 | Login screen: active-users dropdown, hash-verify, generic error, block deactivated users. |
| 4 | ASWD-45 | Password complexity rules + reuse check + account lockout. |
| 5 | ASWD-50 | Logout — mostly already works, minor hookup only. |

---

## Decisions

| Topic | Plan | Why | Status |
|---|---|---|---|
| Password hashing algo | PBKDF2-HMACSHA256, built into .NET, 210k iterations, per-user salt | No new dependency; FIPS-validated (healthcare-adjacent product) | ✅ |
| Password hashing algo — alt | Could use BCrypt instead if FIPS doesn't matter to you | Simpler code, but adds a package | ❓ *(only if you want to override PBKDF2)* |
| Username storage | Stays reversibly encrypted (AES + DPAPI), not hashed | Login dropdown must show plaintext usernames — can't reverse a hash | ✅ |
| Username encryption key | Windows DPAPI (`ProtectedData`, machine-scoped) instead of today's hardcoded machine-name key | Removes the hardcoded key; zero custom key-management code | ✅ |
| Username encryption — tradeoff | DPAPI ties data to one PC — DB can't be moved to a different machine and still decrypt | Fine for a per-clinic desktop app (matches how the DB file already lives locally) | ❓ *(confirm this deployment model is correct)* |
| Username uniqueness | Add a new `UsernameLookupHash` column (deterministic SHA-256) for the unique index; encrypted column itself can no longer be unique-checked once we fix the static-IV bug | Fixing the security bug silently breaks duplicate-username detection unless we add this | ✅ |
| Role model | Simple `Role` enum (Admin/Screener) only — not the full 33-permission Profile system from FEATURE_REQUIREMENTS.md | Ticket only asks for 2 roles; full Profile system is a much bigger, separate effort | ✅ |
| Password rules | 8+ chars, upper, lower, digit. **No special-character rule.** | Matches JIRA (ASWD-45) exactly — drops the special-char rule current code has | 🟡 *(per your answer)* |
| Lockout threshold | JIRA says 5 attempts, admin-unlock only, no auto-unlock. Current code does 10 attempts + 15-min auto-unlock. | Direct conflict — not picking one without you | ❓ **BLOCKING for PR 4** |
| Users management screen | Stays mock/hardcoded. Test users provisioned via DB seed, not the UI. Only exception: a real `UnlockUserAsync` method (no UI) so lockout is testable. | full Users CRUD screen is separate future work | 🟡 *(per your answer can be changed)* |
| Audit logging | Not building anything. Every ticket's audit bullets reference an "Audit Trail epic" we don't have. | Per your answer — raise as a question, revisit when that epic exists - no audit for this phase of development | ❓ **need that ticket, or explicit OK to keep deferring** |
| Session tracking | New `ICurrentUserContext` (who's logged in, in memory) replacing today's ad-hoc "pass username string through navigation calls" | Needed by Role checks and (eventually) audit logging; not a new concept, just formalizing what login/logout already imply | ✅ |
| 90-day password expiration | Left alone, not touched by these 5 PRs | Not mentioned in any of the 5 tickets | 🟡 |
| Existing dev/seed passwords | Just re-seed with hashed values, no migration of old encrypted passwords | Assuming no production data exists yet | 🟡 |

---

## `User` table changes (one migration, in PR User Story 1)

| Field | Change |
|---|---|
| `ProfilePassword` | renamed → `PasswordHash` (stores hash now, old name would be misleading) |
| `AccountName` | unchanged (stays encrypted) |
| *(new)* `UsernameLookupHash` | added — unique index moves here |
| *(new)* `Role` | added — `Admin` / `Screener` |
| `Status` (int) | replaced → `IsActive` (bool) |
| *(new)* `IsLocked`, `LockedAt` | added — stored explicitly instead of derived each login |
| `FailedResetAttemptCount`, `FirstResetLoginTime` | dropped — dead fields, unused anywhere today |

---

## ❓ Open Questions — need your answer before PR 4 starts

1. **Lockout: 5 attempts / admin-only unlock (JIRA) vs. 10 attempts / 15-min auto-unlock
   (existing code)?** Pick one, or tell us a third number/behavior.
2. **Audit Trail epic** — can you share that ticket, or confirm we keep deferring audit logging
   (recording login/logout/lockout/role-change events) to a later PR once it exists?

Everything else in this sheet is either already decided or already confirmed by you — flag any
row above you disagree with and we'll adjust before coding starts.
