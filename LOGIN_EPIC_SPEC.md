# Login Epic — Implementation Spec (ASWD-1)

**Purpose:** Pre-development design spec for the 5 user stories under the "Login" epic
(ASWD-1). Each story is developed as its own sequential PR. This document exists to resolve
every implementation ambiguity **before** coding starts, so no design question needs to be
raised mid-development.

**Source documents:** `AccuSync - JIRA - User Story\{ASWD-1 - Feature, UserStory 1-5}.doc`
(extracted 2026-08-04). Cross-referenced against `FEATURE_REQUIREMENTS.md` (§2 Authentication,
§12 Users & Profiles) and `Databases/SettingsDatabase.sql` (target schema), and against the
actual current state of the auth code on this branch.

**Why this isn't a routine feature add:** the existing auth code (inherited from the original
POC) is a real security liability, not just incomplete. Passwords are reversibly AES-decrypted
and compared in plaintext rather than hashed; the encryption key is derived from the machine
name plus a hardcoded string; the IV is static/reused; encryption is silently disabled outside
Release builds; and failures are swallowed and silently fall back to storing plaintext.
`FEATURE_REQUIREMENTS.md` already flags this as a known gap — these 5 tickets are the trigger to
actually fix it, not just add new UI on top of it.

---

## 1. Current-state summary

| Area | Current state |
|---|---|
| Password storage | Reversible AES-256-CBC "encryption", decrypt-and-compare at login. **Not hashed at all.** |
| Encryption key | `Environment.MachineName + "AccuSync2024"` → SHA-256 → AES key. Hardcoded, no key management. |
| IV | Static, derived the same way, reused for every encryption (defeats CBC's security guarantee). |
| Build-mode gating | Encryption is a no-op (`#if RELEASE`) outside Release builds — Debug builds store plaintext. |
| Error handling | Encrypt/Decrypt swallow exceptions and silently return the unencrypted value. |
| Username storage | Encrypted (reversibly) via the same flawed scheme — correct *approach* (must stay reversible so the login dropdown can display it), wrong *implementation*. |
| Role / RBAC | No `Role` field exists. Role is inferred by string-matching `AccountName == "Admin"` in two ViewModels. A `UserPermissionsViewModel` permission-flag scaffold exists but is never invoked. |
| Lockout | 10 consecutive failures → locked for 15 minutes → auto-unlocks. Derived at auth time from `FailedLoginAttemptCount`/`FirstFailedLoginTime`, not a stored discrete flag. |
| Password complexity | Enforced client-side only, in `ChangePasswordViewModel`: length 8+, upper, lower, digit, **special character** (5 rules), reuse-of-last-3 check. |
| Logout | Implemented and working — clears the dashboard window and returns to `LoginWindow`. No formal "current session" object to clear alongside it. |
| Audit logging | **Does not exist anywhere in the codebase.** Only mentioned in doc comments as a planned Serilog file sink. |
| Users management screen | 100% hardcoded/mock data — not wired to the database at all (hardcoded default passwords, no real CRUD). |

---

## 2. Foundational decisions (apply across all 5 stories, land in ASWD-31's PR / User Story 1)

### 2.1 Password hashing — PBKDF2-HMACSHA256, built into .NET
- `Rfc2898DeriveBytes.Pbkdf2(...)` (`System.Security.Cryptography`) — **no new NuGet dependency**.
- 210,000 iterations (OWASP 2023 minimum for PBKDF2-HMAC-SHA256), per-user random 128-bit salt,
  256-bit derived key.
- Stored as one column: `{iterations}.{base64(salt)}.{base64(hash)}` — self-describing, so the
  iteration count can be raised later without invalidating existing hashes.
- Verification is **hash-to-hash only**. `AuthenticationService` must never decrypt/compare
  plaintext again.
- **Why not BCrypt or Argon2** (both cited as examples in the ticket and in
  FEATURE_REQUIREMENTS.md): this is healthcare-adjacent software. PBKDF2 has FIPS 140-validated
  implementations available on Windows; neither BCrypt nor Argon2 do. It also avoids adding
  `BCrypt.Net-Next` / `Konscious.Security.Cryptography` as new dependencies when the BCL already
  provides an industry-accepted adaptive hash.
  **→ Confirm with lead**: if there's no actual FIPS mandate, BCrypt is a reasonable, simpler
  alternative (salt embedded in the hash string, no manual iteration/salt bookkeeping). Flagging
  as swappable if FIPS isn't a real constraint for this product.

### 2.2 Username encryption — keep reversible AES, fix the real flaws, add a lookup hash
Username must stay **reversible** (not hashed) — the login screen needs a dropdown of plaintext
usernames, which a one-way hash can't provide. Fix the actual implementation:
- Replace the machine-name-derived key with **Windows DPAPI**
  (`System.Security.Cryptography.ProtectedData`, `DataProtectionScope.LocalMachine`) — a
  Microsoft-maintained NuGet package, no custom key-management code to write or audit.
- Remove the `#if RELEASE` gating — encryption active in every build configuration, Debug
  included.
- Stop swallowing exceptions and silently returning plaintext on failure — fail loudly instead.

  **→ Confirm with lead — deployment tradeoff**: DPAPI with `LocalMachine` scope ties encrypted
  data to the specific Windows machine it was encrypted on. If a clinic's SQLite DB file is ever
  copied/restored to a different PC, usernames become undecryptable. This matches how the DB
  file itself already lives locally and isn't designed to be portable, but is a real operational
  property worth an explicit sign-off.

- **Non-obvious gotcha, flagged explicitly**: fixing the static-IV flaw makes encryption
  non-deterministic (the same username encrypts to different ciphertext each time) — which
  **breaks the existing DB-level unique index on `AccountName`**, since two encryptions of the
  same username will never compare equal as ciphertext. **Fix**: add a second column,
  `UsernameLookupHash` (deterministic SHA-256 of the normalized/lowercased username), and move
  the **unique index** there. Login lookup becomes: hash the entered username → find the row by
  `UsernameLookupHash` → decrypt `AccountName` only for display. This is a standard "blind
  index" pattern — one small schema addition, but it's easy to miss and would otherwise silently
  reintroduce duplicate-username bugs.

### 2.3 `User` entity changes (`AccuSync.Core/Entities/User.cs`)
One batched EF migration, landing with ASWD-31:

| Change | Reason |
|---|---|
| Rename `ProfilePassword` → `PasswordHash` | It will hold a hash, not a password in any reversible form — the old name is actively misleading. |
| Add `UsernameLookupHash` (unique-indexed) | Per §2.2. |
| Add `Role` (new `enum UserRole { Admin, Screener }`, `HasConversion<string>()`) | Replaces the unused, freeform `ProfileId` string as the actual source of role. DB column stays human-readable (`"Admin"`/`"Screener"`). |
| Replace `int Status` → `bool IsActive` | Current `Status` semantics are literally undefined in code (already flagged by a TODO). Matches FEATURE_REQUIREMENTS.md and the SQL target schema. |
| Add `bool IsLocked` + `long? LockedAt` | ASWD-45 says "set account status to locked **in the local data store**" — a discrete stored flag, not derived at auth time as today. Also what the Users-screen "Locked" indicator needs to eventually bind to for real. |
| Keep `FailedLoginAttemptCount`, `FirstFailedLoginTime` | Still drive the lockout trigger. |
| Keep `LastThreePasswords` | Still needed for reuse checking — now stores hashes, not encrypted plaintext. |
| Keep `FirstLogin`, `PasswordModificationDate` | Unrelated to this epic's changes. |
| **Drop** `FailedResetAttemptCount`, `FirstResetLoginTime` | Confirmed dead — never read or written anywhere in the codebase. Remove rather than carry forward unused, unless the lead knows of a planned self-service-reset flow that needs them. |

### 2.4 New abstraction: `ICurrentUserContext`
No "who is logged in right now" service exists today — the username is threaded manually as a
raw string through navigation method parameters (`SetCurrentUser(username)`). RBAC checks
(ASWD-36), audit logging's future "acting user" field, and logout (ASWD-50) all need one source
of truth for the active session.

- `AccuSync.Core.Abstractions.Services.ICurrentUserContext`: `UserId`, `AccountName`, `Role`,
  `SignIn(User)`, `SignOut()`.
- Implemented as an in-memory singleton in `AccuSync.Application`, registered `AddSingleton`.
- `LoginViewModel` calls `SignIn(...)` on success; the logout handler (ASWD-50) calls
  `SignOut()`.
- This is exactly what ASWD-41 itself describes: "the authenticated user context is held in
  application memory for the duration of the session."

### 2.5 Password complexity — 4 rules, matching ASWD-45 literally
**Decision (confirmed):** length ≥ 8, ≥1 uppercase, ≥1 lowercase, ≥1 digit. **No special-character
rule** — this drops the 5th rule present in today's code and in FEATURE_REQUIREMENTS.md §2.2.
Move validation out of `ChangePasswordViewModel` into a shared `PasswordPolicy.Validate(string)`
(in `AccuSync.Application`) so it applies consistently to change-password now and any future
create-user flow, rather than being re-implemented per screen.

Reuse-of-last-3-passwords check: keep the existing mechanism, switched to hash comparison.

### 2.6 Users management screen — out of scope for these 5 PRs
**Decision (confirmed):** `UsersContentView.xaml.cs` stays hardcoded/mock. Wiring its full
Create/Edit/Delete flow to real persistence is a separate future epic. Test/dev users are
provisioned via a DB seed for the duration of these 5 PRs, not through the UI.

**Narrow exception:** ASWD-45's "cannot log in until unlocked by an Admin" AC needs *some* unlock
capability to be testable. Resolution: implement unlock at the repository/service layer only —
`IUserRepository.UnlockUserAsync(userId)` clearing `IsLocked`, `LockedAt`,
`FailedLoginAttemptCount`, and `FirstFailedLoginTime` together (per FEATURE_REQUIREMENTS.md
§2.1's explicit note that a correct unlock must clear the counter and window, not just the
flag) — covered by tests, without building or wiring the Users screen's UI.

### 2.7 Audit logging — deferred, not built in this PR sequence
**Decision (confirmed):** four of the five stories reference audit events "per the Audit Trail
epic" — that epic's ticket wasn't provided and doesn't exist in this repo. No audit mechanism
(interface, service, file sink, or table) will be designed or built as part of these 5 PRs. Each
story's audit bullets are kept below, marked **(deferred)**, so nothing is silently dropped from
the record. They'll be implemented for real, likely as their own retrofit PR, once the actual
epic ticket is available. **See Open Question #2.**

### 2.8 RBAC scope — simple `Role` enum, not the full 33-permission Profile system
ASWD-36 asks for a 2-value role gating nav visibility, not the richer 33-permission-bit
`Profiles` system documented in FEATURE_REQUIREMENTS.md §12 / `Databases/SettingsDatabase.sql`.
Building the full system now would be materially larger than "assign a role to a user" — out of
scope for this ticket.

Build: `Role` enum on `User` (§2.3), `Role` on `ICurrentUserContext` (§2.4), and wire the
**existing but never-invoked** `UserPermissionsViewModel` / `SidebarNavigation.SetPermissions(...)`
scaffold with real data derived from `Role` (`Admin()` / `Screener()` presets already exist)
instead of the current dead code path. Gate at both nav-visibility (existing pattern) and
individual command level where a restricted action already exists on a Screener-visible screen.

### 2.9 Login-failure reason vocabulary (decided now, wired later)
Defined so every future audit call site agrees on the same values once audit logging lands:
```
enum LoginFailureReason { InvalidCredentials, DeactivatedAccount, AccountLocked, PasswordExpired }
```

### 2.10 Explicitly out of scope / unchanged
- **90-day password expiration** (`AuthenticationService`, current lines ~120-136): not
  mentioned in any of the 5 tickets. Left as-is — orthogonal to hashing (reads
  `PasswordModificationDate`, independent of hash-vs-encrypt).
- **No production-data migration path**: pre-production rebuild, no live users yet. Existing
  encrypted dev/seed passwords are simply replaced by re-seeding with hashed values — no
  rehash-on-next-login strategy needed. *(Assumption — flag if there's live data this doesn't
  account for.)*

---

## 3. Per-story spec

### ASWD-31 — Implement Credential Encryption *(foundational PR)*
**User story:** As a developer, when user credentials are stored, I want them protected at rest
so that user data is protected.

**Acceptance criteria (from JIRA):**
- Stored password values are hashed (not reversible to plain text).
- Stored username values are encrypted (not human-readable in the local database).
- Local database inspection shows no plain-text passwords or usernames.

**Design:** §2.1–§2.4 in full.

**Files:** `EncryptionService.cs` (DPAPI, remove `#if RELEASE`, remove silent failure), new
`IPasswordHasher` (Core) / `PasswordHasher` (Application impl), `User.cs`, `UserConfiguration.cs`,
`AuthenticationService.cs` (decrypt-and-compare → hash-and-compare), `UserRepository.cs`
(populate `UsernameLookupHash` on write), new EF migration, new `ICurrentUserContext` +
`AuthenticationResult` wiring.

**Sub-tasks:** ASWD-32 (hashing/encryption impl), ASWD-34 (unit tests: hash verify,
encrypt/decrypt round-trip, lookup-hash uniqueness), ASWD-35 (security-focused code review).

---

### ASWD-36 — Implement Role-Based Access Control
**User story:** As an admin, I want users assigned to Admin or Screener roles so that access is
limited to appropriate functions.

**Acceptance criteria (from JIRA):**
- Users can be assigned a role of Admin or Screener at account creation.
- Admin users can access all app features, including user management.
- Screener users can access only the features designated for their role.
- Role changes take effect on the user's next login.

**Design:** §2.8. Role assignment "at account creation" is satisfied via DB seed per §2.6 (no
Users-screen UI work in this story). "Takes effect on next login" matches the in-memory-only
`ICurrentUserContext` (§2.4) — no live re-evaluation mid-session is expected or required.

**Files:** `User.cs` (Role, landed in ASWD-31), `UserPermissionsViewModel.cs` (wire up real
`Admin()`/`Screener()` presets), `SidebarNavigation.xaml.cs` (actually call `SetPermissions`
with real data), `LoginViewModel.cs` / `App.xaml.cs` (replace username string-matching with
`user.Role`).

**Audit bullets (deferred, §2.7):** role assignment at creation → audit event (target user,
assigned role, performing admin, timestamp, status). Role change → audit event (target user,
previous role, new role, performing admin, timestamp, status).

---

### ASWD-41 — Implement User Login and Deactivated User Login Prevention
**User story:** As a user, I want to log on to the app with my username and password so that I
can use the app.

**Acceptance criteria (from JIRA):**
- Login screen appears on app launch.
- Username dropdown lists only active users.
- Valid credentials grant access.
- Invalid credentials display an error and deny access.
- Deactivated users cannot log in regardless of password correctness.

**Design:** `AuthenticationService.AuthenticateAsync` — hash-verify (§2.1), check `IsActive`
alongside the credential check, keep the existing generic error message ("Invalid username or
password" — never reveal which field is wrong), populate `ICurrentUserContext` on success
(§2.4). Username dropdown: decrypt `AccountName` for all `IsActive` users — fine at this scale
(a per-clinic desktop app, realistically tens of users), no pagination/lazy-load needed.

**Audit bullets (deferred, §2.7):** successful login → audit event (user, timestamp, status).
Failed login attempt → audit event (attempted username, timestamp, status, reason — using the
`LoginFailureReason` enum from §2.9 once wired).

---

### ASWD-45 — Implement User Account Lockout and Password Complexity Requirements
**User story:** As a user, I want password rules and account protections in place so that my
account stays secure.

**Acceptance criteria (from JIRA):**
- Password creation/change rejects passwords not meeting complexity rules, with specific
  feedback on which rule failed.
- Password creation/change rejects passwords matching any of the last 3 used.
- After **5 consecutive failed login attempts**, the account is locked.
- Locked accounts cannot log in even with correct credentials.
- A successful login before the 5th failed attempt resets the counter.

**⚠️ Do not start this story's lockout-count logic until Open Question #1 (below) is resolved** —
the attempt threshold and whether lockout auto-expires are in direct conflict with the existing
implementation.

**Design:** §2.5 (4-rule `PasswordPolicy.Validate`, shared/reusable), lockout logic in
`AuthenticationService` writing discrete `IsLocked`/`LockedAt` (§2.3), repository-level
`UnlockUserAsync` (§2.6's narrow exception) covered by tests.

**Audit bullets (deferred, §2.7):** password change → audit event (user, timestamp, status —
never log the password or its hash). Account lockout → audit event (user, timestamp, status,
triggering condition).

---

### ASWD-50 — Implement User Logout
**User story:** As a user, I want to be able to log out to end my session.

**Acceptance criteria (from JIRA):**
- Logout option is visible and accessible from all screens within the app.
- Clicking logout ends the session and returns the user to the login screen.
- After logout, the user must re-authenticate to access the app.

**Design:** Mostly already implemented (`SidebarNavigation.xaml.cs` `Logout_Click` →
`App.NavigateAfterLogin(..., null, null)`). The one addition: call
`ICurrentUserContext.SignOut()` (§2.4) at the same point, so the new session-context abstraction
stays in sync with the existing navigation-based logout instead of introducing a second,
competing "who's logged in" source of truth.

**Audit bullets (deferred, §2.7):** logout → audit event (user, timestamp, status).

---

## 4. Open Questions for Lead

These must be resolved before (or at the very start of) ASWD-45 — everything else in this spec
is unblocked.

1. **Lockout threshold and unlock behavior.** JIRA (ASWD-45) says 5 consecutive failed attempts,
   locked until an Admin manually unlocks it — no auto-unlock mentioned. The *existing code* and
   *FEATURE_REQUIREMENTS.md §2.1* both implement 10 attempts with an automatic 15-minute
   cooldown/unlock. These differ on both the threshold **and** whether lockout ever self-clears.
   **Recommendation if a default is needed:** follow the JIRA ticket literally (5 attempts,
   admin-unlock only) as the most specific, most recently authored artifact for this exact PR —
   but this needs explicit lead confirmation rather than an assumption, since it's a behavior
   change from what's already built.

2. **Audit Trail epic ticket.** Every story but ASWD-31 references audit events "per the Audit
   Trail epic," which wasn't provided and isn't in this repo. Per direction already given: not
   building a provisional audit mechanism now. Requesting the actual epic ticket (or confirmation
   that audit logging is intentionally deferred to a later, separate PR) before that epic's work
   begins — the audit acceptance-criteria bullets above stay documented so nothing already
   agreed to is lost in the meantime.

---

## 5. Verification plan (once implementation starts)

- **Unit tests** per sub-task: hash/verify round-trip, encrypt/decrypt round-trip including the
  `UsernameLookupHash` uniqueness path, password-policy rule-by-rule rejection (each of the 4
  rules independently), lockout trigger + reset, reuse-of-last-3 rejection.
- **Manual walkthrough:** fresh seeded DB → log in as seeded Admin and Screener accounts →
  confirm nav differences per role → force failed logins up to the agreed threshold (Open
  Question #1) → confirm lock behavior matches whatever that resolves to → confirm
  `UnlockUserAsync` clears counter + timestamp + flag together → change password through all 4
  complexity rules individually failing/passing → attempt reuse of a recent password → log out →
  confirm re-prompt for login on next launch.
- **DB inspection:** open the SQLite file directly and confirm `PasswordHash` is not
  reversible-looking; confirm `AccountName` ciphertext differs across two logically-identical
  re-encryptions (proves the static-IV fix); confirm `UsernameLookupHash` still uniquely
  identifies each user despite the encrypted column no longer being comparable.
