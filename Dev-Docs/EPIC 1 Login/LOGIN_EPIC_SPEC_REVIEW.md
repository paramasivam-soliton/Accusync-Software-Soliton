# Login Epic (ASWD-1) — Review Sheet

One line per point: what we plan to do, why, and whether it's **Decided**, an **Assumption**, or
a **Question** you need to answer. Full rationale/detail lives in `LOGIN_EPIC_SPEC.md` if you
want to go deeper on any row — this sheet is just for a fast pass.

**Legend:** ✅ Decided (going with this unless you object) · 🟡 Assumption (flag if wrong) · ❓ Question (blocking, need your answer)

**Updated 2026-08-05 after lead review (two rounds)** — changed rows are marked 🔄.

---

## Scope — 5 PRs, in order

| # | Ticket | One-line scope |
|---|---|---|
| 1 | ASWD-31 | Hash passwords, fix username encryption. Foundational — everything below depends on this. |
| 2 | ASWD-36 | Start populating existing `ProfileId` with Admin/Screener; wire up nav-hiding by role. |
| 3 | ASWD-41 | Login screen: active-users dropdown, hash-verify, generic error, block deactivated users. |
| 4 | ASWD-45 | Password complexity rules + reuse check + account lockout. |
| 5 | ASWD-50 | Logout — mostly already works, minor hookup only. |

---

## Decisions

| Topic | Plan | Why | Status |
|---|---|---|---|
| Password hashing algo | PBKDF2-HMACSHA256, built into .NET, 210k iterations, per-user salt | FIPS-validated, no new dependency | ✅ |
| 🔄 Salt format | 128-bit random salt, generated fresh every hash. Stored as `iterations.base64(salt).base64(hash)`. Confirmed `.` is a safe separator — Base64's alphabet never produces `.` | documented this precisely — done, see spec §2.1 | ✅ |
| Username storage | Stays reversibly encrypted, not hashed | Login dropdown must show plaintext usernames | ✅ |
| 🔄 Username encryption key | **Changed from DPAPI Plan → ASP.NET Core Data Protection API** (`Microsoft.AspNetCore.DataProtection`), used standalone (no web server/hosting involved, just a package + DI registration) | Cross-platform, portable across machines (fixes DPAPI's biggest problem), fixes the IV bug internally too | ✅ |
| 🔄 Username uniqueness | Confirmed this is a standard pattern ("blind index") — keeping a separate deterministic-hash column for the unique index/lookup, since the encrypted column can no longer be compared once encryption is non-deterministic | Standard fix for "need uniqueness on an encrypted column"; see spec §2.2 for why other approaches don't fit | ✅ |
| 🔄 Column name | `UsernameHash` (renamed from `UsernameLookupHash`) | Simpler, drops the redundant qualifier | ✅ |
| 🔄🔄 Role vs. ProfileId | **No new column, reversed again.** Reuse the existing `ProfileId` (already an unused plain string) — populate it with `"Admin"`/`"Screener"`, interpreted in application code (small parse helper), no EF-level conversion | Whichever path we pick, the real future Profile system will need *some* data migration later — reusing `ProfileId` means one less column to reconcile at that point, and matches "avoid unnecessary schema churn" | ✅ |
| 🔄 Password rules | **Back to 5 rules** — length 8+, upper, lower, digit, **and special character**. UI under System Config → User & Profile Config → Password Security Rule → "Complex" must say it requires a special character | Matches existing code/FEATURE_REQUIREMENTS.md; JIRA's 4-rule list treated as non-exhaustive | ✅ |
| 🔄 `ProfilePassword` column name | **Not renamed.** Stays `ProfilePassword`, just holds a hash now instead of an encrypted value | Avoid unnecessary rename | ✅ |
| 🔄 Lockout threshold | **Confirmed: 5 attempts, admin-unlock only, no auto-unlock** (JIRA's rule, replacing the old 10-attempt/15-min logic entirely) | Direct instruction | ✅ |
| 🔄 `IsLocked` flag | **Removed from plan.** No discrete column — "locked" = `FailedLoginAttemptCount >= 5`. On the 5th failure, log the time into the existing `FirstFailedLoginTime` field (repurposed, not renamed) | Simpler; attempt count alone is enough | ✅ |
| 🔄 `FirstFailedLoginTime` reuse | **Confirmed** — reuse this existing field as-is for the lockout timestamp, no new field added | Direct instruction | ✅ |
| Users management screen | Stays mock/hardcoded. Test users via DB seed. Only exception: a real `UnlockUserAsync` method (no UI) | Full Users CRUD screen is separate future work | ✅ |
| 🔄 Audit logging | Not needed for this phase — confirmed, not just deferred pending a ticket | Explicit lead call | ✅ |
| Session tracking | New `ICurrentUserContext` (who's logged in, in memory) | Needed by Role checks | ✅ |
| 🔄 Timestamps | **Confirmed already UTC everywhere** (`DateTimeOffset.UtcNow`) — checked every write site, nothing stores local time | You asked us to verify this — done, no code change needed | ✅ |
| 90-day password expiration | Left alone, not touched by these 5 PRs | Not mentioned in any of the 5 tickets | 🟡 |
---

## `User` table changes (one migration, in PR User Story 1)

| Field | Change |
|---|---|
| `ProfilePassword` | 🔄 **unchanged name** — now stores a hash instead of an encrypted value |
| `AccountName` | unchanged name — encryption mechanism changes to Data Protection API |
| *(new)* `UsernameHash` | 🔄 added (renamed from `UsernameLookupHash`) — unique index moves here |
| `ProfileId` | 🔄🔄 **no new column** — reused as-is to hold `"Admin"`/`"Screener"`, interpreted in code |
| `Status` (int) | still planned: replaced → `IsActive` (bool) |
| ~~`IsLocked`, `LockedAt`~~ | 🔄 **removed from plan** — lock state derived from attempt count instead |
| `FailedLoginAttemptCount` | kept — now the sole signal for "is this account locked" (>=5) |
| `FirstFailedLoginTime` | 🔄 kept, not renamed, reuse **confirmed** — repurposed to record when lockout happened |
| `FailedResetAttemptCount`, `FirstResetLoginTime` | 🔄 **kept, not dropped** — plumbed through the repository but never meaningfully used by any feature; most likely a placeholder for the missing self-service password-reset flow noted in FEATURE_REQUIREMENTS.md §2.1, not dead code. Left untouched. |

---

## ❓ Open Questions

**None outstanding.** All previously raised items (lockout threshold, password rules, audit
logging, `FirstFailedLoginTime` reuse, Role storage) are resolved — see the ✅ rows above. Spec
is clear to build against.
