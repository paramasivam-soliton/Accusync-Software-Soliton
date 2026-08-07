# Login Epic — Implementation Spec (ASWD-1)

**Purpose:** Pre-development design spec for the 5 user stories under the "Login" epic
(ASWD-1). Each story is developed as its own sequential PR. This document exists to resolve
every implementation ambiguity **before** coding starts, so no design question needs to be
raised mid-development.

**Source documents:** `AccuSync - JIRA - User Story\{ASWD-1 - Feature, UserStory 1-5}.doc`
(extracted 2026-08-04). Cross-referenced against `FEATURE_REQUIREMENTS.md` (§2 Authentication,
§12 Users & Profiles) and `Databases/SettingsDatabase.sql` (target schema), and against the
actual current state of the auth code on this branch.

**Revision history:**
- 2026-08-04 — initial draft.
- 2026-08-05 — updated after lead review (see inline "Per lead review" notes throughout).

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
| Password complexity | Enforced client-side only, in `ChangePasswordViewModel`: length 8+, upper, lower, digit, special character (5 rules), reuse-of-last-3 check. |
| Logout | Implemented and working — clears the dashboard window and returns to `LoginWindow`. No formal "current session" object to clear alongside it. |
| Audit logging | **Does not exist anywhere in the codebase.** Only mentioned in doc comments as a planned Serilog file sink. |
| Users management screen | 100% hardcoded/mock data — not wired to the database at all (hardcoded default passwords, no real CRUD). |

---

## 2. Foundational decisions (apply across all 5 stories, land in ASWD-31's PR / User Story 1)

### 2.1 Password hashing — PBKDF2-HMACSHA256, built into .NET

- `Rfc2898DeriveBytes.Pbkdf2(...)` (`System.Security.Cryptography`) — **no new NuGet dependency**.
- 210,000 iterations (OWASP 2023 minimum for PBKDF2-HMAC-SHA256).
- Verification is **hash-to-hash only**. `AuthenticationService` must never decrypt/compare
  plaintext again.
- **Why not BCrypt or Argon2**: this is healthcare-adjacent software. PBKDF2 has FIPS
  140-validated implementations available on Windows; neither BCrypt nor Argon2 do. It also
  avoids adding a third-party package when the BCL already provides an industry-accepted
  adaptive hash. *(Not challenged in lead review — stands as decided; still swappable later if
  there's no actual FIPS mandate.)*

**Salt generation (per lead review — documented explicitly):**
- The salt is a **128-bit (16-byte) cryptographically random value**, generated fresh for every
  single hash via `RandomNumberGenerator.GetBytes(16)` — pure randomness, not derived from the
  password, the username, or anything else. A new salt is generated every time a password is
  hashed (initial seed, forced first-login change, self-service change), never reused.
- **Storage format:** the salt and hash are stored together as one self-describing string:
  ```
  {iterations}.{base64(salt)}.{base64(hash)}
  ```
  Example: `210000.vlZmCcKUMsq2+kXXqkmpAg==.ocRXW0fc+J8TSiXmTFVsx6OL6nyM2MYYObJkPd8Ufg0=`
- **Confirmed the `.` separator is safe** — `Convert.ToBase64String` (standard .NET Base64,
  RFC 4648 §4) only ever emits characters from `A–Z`, `a–z`, `0–9`, `+`, `/`, plus `=` padding.
  A literal `.` can never appear inside a Base64-encoded segment, so splitting the stored string
  on `.` is unambiguous by construction — there's no scenario where the salt or hash bytes
  themselves produce a `.` that could be confused with the separator.

### 2.2 Username encryption — ASP.NET Core Data Protection API *(changed per lead review)*

~~Originally proposed: Windows DPAPI.~~ **Superseded** — DPAPI was rejected because it's
Windows-only and ties encrypted data to one specific machine (DB can't be moved to a new PC and
still decrypt). Replaced with:

- **`Microsoft.AspNetCore.DataProtection`**, used as a **standalone library** —
  `services.AddDataProtection().PersistKeysToFileSystem(...)` registered in the DI container like
  any other service. **Confirmed: this does not require a web server, Kestrel, ASP.NET Core
  hosting, or MVC** — it's a small, focused key-management/crypto library that Microsoft ships
  under the ASP.NET Core umbrella but works from any .NET app type, WPF included. Adding the
  package pulls in just the Data Protection library and its direct dependencies, nothing
  web-server-related.
- Cross-platform (Windows/Linux/macOS), and **portable across machines** if the key folder is
  backed up alongside the database file — resolves the DPAPI machine-binding problem entirely.
- Handles IV/nonce generation correctly internally — removes the entire bug class already found
  in today's hand-rolled AES (static/reused IV can't happen; every `Protect()` call is
  non-deterministic by design).
- Automatic key rotation/versioning if ever needed later.
- Remove the `#if RELEASE` gating on encryption — active in every build configuration, Debug
  included. Stop swallowing exceptions and silently returning plaintext on failure — fail loudly
  instead.

**Username uniqueness — blind index / lookup hash column (per lead review: confirmed as a
standard pattern, documented here):**

Because Data Protection's `Protect()` is non-deterministic (same input → different ciphertext
every call, by design), the database can no longer enforce username-uniqueness with a plain
`UNIQUE` index on the encrypted column — two encryptions of the same username will never compare
equal as ciphertext. The fix is a companion column holding a **deterministic** hash of the
username, used only for uniqueness and lookup; the securely encrypted value stays the one used
for display.

- **Is this a standard approach?** Yes — this is commonly called a **"blind index"** (also seen
  as "keyed lookup hash" or "searchable encryption index" in vendor docs). It's the standard
  answer, referenced in OWASP's cryptographic-storage guidance, for "I need equality search or a
  uniqueness constraint on a column that must otherwise be encrypted." It shows up as a named
  feature in several real libraries/products (e.g., Rails' `blind_index` gem, various
  field-level-encryption add-ons for Postgres/SQL Server).
- **Why not other approaches:**
  - *Decrypt-and-scan every row* (what today's code already does for username lookup) doesn't
    scale, and — more importantly — provides no DB-enforced uniqueness guarantee at write time,
    only a slow, racy application-level check.
  - *Deterministic/order-preserving encryption of the whole field* would restore comparability,
    but reintroduces the exact weakness we're fixing: identical plaintexts always producing
    identical or comparable ciphertext leaks patterns (defeats semantic security).
  - *Full searchable-encryption schemes* (homomorphic, order-revealing encryption) solve a much
    bigger problem — range queries, partial match — than we actually have (exact-match-only, one
    field). Unjustified complexity here.
- **Column name:** `UsernameHash` *(renamed per lead review — simpler than the originally
  proposed `UsernameLookupHash`; the column's single purpose is self-evident without the extra
  qualifier)*. Deterministic SHA-256 of the normalized (trimmed/lowercased) username, carries the
  unique index. Login lookup becomes: hash the entered username → find the row by
  `UsernameHash` → decrypt the display column only for the matched row.

### 2.3 `User` entity changes (`AccuSync.Core/Entities/User.cs`)

| Field | Change | Notes |
|---|---|---|
| `ProfilePassword` | **No rename** *(reversed per lead review)* | Keeps its existing name. Only its *contents* change — from a reversibly-encrypted value to a one-way PBKDF2 hash (§2.1). The column name stays `ProfilePassword` even though it now holds a hash, per lead's explicit preference to avoid the rename. |
| `AccountName` | Unchanged name; encryption mechanism switches to Data Protection API (§2.2) | |
| *(new)* `UsernameHash` | Added — unique index moves here (§2.2) | |
| `ProfileId` | **Reused to hold the role value** *(revised again — no new `Role` column)* | No schema change at all here. `ProfileId` is already a plain, unused `string` column — start actually writing `"Admin"`/`"Screener"` into it instead of leaving it empty. Interpreted as a `UserRole` enum in **application code** only (a small parse/mapping helper), not via an EF-level `HasConversion` — the database column itself is untouched, just finally populated. See explicit note below. |
| `Status` (int) | Still planned: replace → `bool IsActive` | Unchanged from original plan — current `Status` semantics are undefined in code (pre-existing TODO). Not commented on in lead review; stands as decided. |
| ~~`IsLocked`, `LockedAt`~~ | **Removed from plan** *(per lead review)* | No discrete lock flag/column. See §2.4 below — lock state is derived, not stored as a separate boolean. |
| `FailedLoginAttemptCount` | Kept — becomes the **sole driver** of lock state | `>= 5` means locked (§2.4). |
| `FirstFailedLoginTime` | Kept, **not renamed, reuse confirmed** | Repurposed (not renamed) per §2.4 — previously tracked "start of a failure streak" for the old 10-attempt/15-minute-window logic; now records the moment lockout occurs (set only on the 5th consecutive failure, cleared to `null` on admin unlock). **Confirmed by lead — reusing this existing field is the intended approach, not an open question.** |
| `LastThreePasswords` | Kept | Still a pipe-delimited history, still used for reuse checking — now holds hashes (§2.1's format), not encrypted plaintext. Name unchanged (consistent with not renaming `ProfilePassword`). |
| `FirstLogin`, `PasswordModificationDate`, `CreationDate`, `ModificationDate` | Unchanged | All already store **UTC** Unix timestamps (`DateTimeOffset.UtcNow.ToUnixTimeSeconds()`) — confirmed per lead review, see §2.5. No code change needed here, this was already correct. |

**Role vs. ProfileId — revised again, no new column** *(superseded — an earlier revision of this
doc proposed a separate `Role` column; that's now removed)*: the full Profile/permission-bundle
system (FEATURE_REQUIREMENTS.md §12, 33 permission bits) is confirmed as real future work, not
aspirational — but that doesn't argue for a separate `Role` column today. Reasoning: whichever
path we take now, *some* transformation is needed once the real `Profiles` table exists — either
a new `Role` column has to be reconciled with it, or `ProfileId`'s literal string values
(`"Admin"`/`"Screener"`) get converted into real foreign keys pointing at seeded Profile rows.
Since that future work is unavoidable either way, the simpler option wins today: **no new
column** — reuse `ProfileId` as-is, populate it with `"Admin"`/`"Screener"` now, and interpret it
via a small C# helper (parses the string into a `UserRole` enum for type-safe comparisons in
code) wherever a role-based decision is needed. When the real Profile system is eventually built,
`ProfileId`'s values get migrated from literal role strings to actual profile foreign keys in
one clean pass — no separate `Role` column to reconcile alongside it.

### 2.4 Account lockout — derived from attempt count, no discrete flag *(rewritten per lead review)*

- **Threshold, confirmed per lead review:** follow the JIRA ticket (ASWD-45) literally — **5
  consecutive failed login attempts**, locked until an **Admin** manually unlocks it. **No
  automatic unlock/cooldown** (this replaces the existing code's 10-attempt/15-minute-auto-unlock
  behavior entirely — that behavior is being removed, not kept alongside the new rule).
- **No `IsLocked` boolean.** "Locked" is a derived condition: `FailedLoginAttemptCount >= 5`.
  Nothing separate needs to be checked or kept in sync.
- **On the 5th consecutive failure:** `FailedLoginAttemptCount` reaches 5 (already being
  incremented by existing code), and `FirstFailedLoginTime` is set to the current UTC time —
  this becomes the record of *when* the account became locked (displayable later, e.g. "Locked
  since ...", and useful once audit logging exists).
- **On Admin unlock:** reset `FailedLoginAttemptCount` to `0` and `FirstFailedLoginTime` to
  `null` (or `0`, matching the field's existing non-nullable `long` type) — both together, in
  one operation, so the account is fully reset, not just nominally "unlocked." This is the
  `UnlockUserAsync` repository method already planned in §2.6 below (unchanged).

### 2.5 Password complexity rules — 5 rules including special character *(reversed per lead review)*

~~Originally decided: drop the special-character rule to match ASWD-45's implementation notes
literally (4 rules).~~ **Reversed** — keep **5 rules**: minimum 8 characters, ≥1 uppercase, ≥1
lowercase, ≥1 digit, **and ≥1 special (non-alphanumeric) character**. This matches what
`ChangePasswordViewModel` already enforces today and what FEATURE_REQUIREMENTS.md §2.2
describes — the JIRA ticket's 4-rule list is being treated as non-exhaustive, not as the
authoritative full rule set.

**UI tie-in (per lead review):** System Configuration → User & Profile Configuration →
**Password Security Rule** (`None` / `Simple` / `Complex`) already exists as a setting concept
(FEATURE_REQUIREMENTS.md §10.5). The **`Complex`** tier's on-screen description must explicitly
state that it requires a special character, so the UI copy matches the actual enforced rule
rather than leaving "complex" undefined. *(Wiring this setting to actually vary enforcement
behavior, vs. always enforcing the fixed 5-rule set, remains a FEATURE_REQUIREMENTS.md-level
aspiration beyond what ASWD-45 itself asks for — flagging this distinction so scope stays clear:
ASWD-45 delivers the fixed 5-rule enforcement + correct UI copy under `Complex`; making the
None/Simple/Complex setting actually change behavior is not required by this epic unless the
lead wants it pulled in.)*

Reuse-of-last-3-passwords check: unchanged mechanism (compare against `LastThreePasswords`), now
via hash comparison instead of decrypt-and-compare.

### 2.6 Users management screen — out of scope for these 5 PRs *(unchanged, confirmed earlier)*

- `UsersContentView.xaml.cs` stays hardcoded/mock for now — wiring its full Create/Edit/Delete
  flow to real persistence is a separate future epic, not part of this Login epic.
- Test/dev users are provisioned via a DB seed, not through the UI, for the duration of these 5
  PRs.
- **Narrow exception:** ASWD-45's "cannot log in until unlocked by an Admin" AC needs *some*
  unlock capability to be testable. Resolution: implement unlock at the repository/service layer
  only — `IUserRepository.UnlockUserAsync(userId)` clearing `FailedLoginAttemptCount` and
  `FirstFailedLoginTime` together (§2.4) — covered by tests, without building or wiring the Users
  screen's UI.

### 2.7 Audit logging — confirmed not needed for this phase *(resolved per lead review)*

**RESOLVED (2026-08-05):** audit logging is not required for this phase of development. Every
story's "Audit logging (per Audit Trail epic)" bullet is explicitly deferred — no audit
mechanism (interface, service, file sink, table) will be designed or built as part of these 5
PRs, and no Audit Trail epic ticket is needed to proceed. This is a closed decision, not a
pending question. The bullets stay documented in each story's spec section purely as a record of
what JIRA originally asked for, in case audit logging is picked up as separate future work.

### 2.8 RBAC scope: `ProfileId`-derived role, not the full 33-permission `Profile` system

*(Updated — no new `Role` column, per §2.3's revised reasoning.)* Build: populate `ProfileId`
with `"Admin"`/`"Screener"` (no schema change), a small `UserRole` parse/mapping helper in
application code, a `Role` property on `ICurrentUserContext` derived from that helper, and wire
the existing-but-never-invoked `UserPermissionsViewModel` / `SidebarNavigation.SetPermissions`
scaffold with real data derived from it.

### 2.9 Timestamps — confirmed already UTC-based, no change needed

**Per lead review's ask to "ensure time is not stored abruptly and instead UTC based time":**
checked every timestamp write site in the current codebase — `User.cs`'s constructor
(`CreationDate`, `ModificationDate`, `PasswordModificationDate`), `AuthenticationService.cs`'s
lockout-window arithmetic, and `TimestampInterceptor` all already use
`DateTimeOffset.UtcNow.ToUnixTimeSeconds()` consistently. **Nothing was found storing local time.**
This is a confirmation, not a change — calling it out explicitly since it was asked about, but no
code needs to move.

### 2.10 Login-failure reason vocabulary (decided, for whenever audit lands)

Unchanged: `enum LoginFailureReason { InvalidCredentials, DeactivatedAccount, AccountLocked,
PasswordExpired }`, defined now so every future audit call site agrees on the same values.

### 2.11 Out of scope - Not Yet decided / Need to discuss with client

- **90-day password expiration**: pre-existing code, not part of any of the 5 tickets, left as-is.
---

## 3. Per-story spec

### ASWD-31 — Implement Credential Encryption *(foundational PR)*
**Acceptance criteria (from JIRA):** stored password values hashed (non-reversible); stored
username values encrypted (not human-readable); DB inspection shows no plain-text
passwords/usernames.

**Design:** §2.1, §2.2, §2.3, §2.9. Files: `EncryptionService.cs` (Data Protection API, remove
`#if RELEASE`, remove silent failure), new `IPasswordHasher` (Core) / `PasswordHasher`
(Application impl, PBKDF2), `User.cs` (no rename — see §2.3), `UserConfiguration.cs`,
`AuthenticationService.cs` (decrypt-and-compare → hash-and-compare), `UserRepository.cs`
(populate `UsernameHash` on write), new EF migration, new `ICurrentUserContext` wiring.

### ASWD-36 — Implement Role-Based Access Control
Unchanged from prior draft — see §2.3's explicit `Role`/`ProfileId` clarification and §2.8.

### ASWD-41 — Implement User Login and Deactivated User Login Prevention
Unchanged from prior draft.

### ASWD-45 — Implement User Account Lockout and Password Complexity Requirements
**Acceptance criteria (from JIRA):** complexity rules enforced with specific per-rule feedback
(§2.5, now 5 rules including special character); reject reuse of last 3 passwords; lock after
**5** consecutive failed attempts (§2.4, confirmed — no longer an open question); locked accounts
cannot log in even with correct credentials; successful login resets the counter.

**Design:** `PasswordPolicy.Validate(...)` shared validator (5 rules, §2.5), lockout logic in
`AuthenticationService` using `FailedLoginAttemptCount >= 5` as the derived lock check (§2.4, no
`IsLocked` column), repository-level `UnlockUserAsync` (§2.6's narrow exception) covered by tests.
Password Security Rule UI copy update under System Configuration (§2.5).

### ASWD-50 — Implement User Logout
Unchanged from prior draft.

---

## 4. Open Questions for Lead

None outstanding as of 2026-08-05 — all previously raised items are resolved:

1. ~~Lockout threshold and unlock behavior~~ **RESOLVED:** 5 consecutive failed attempts,
   admin-unlock only, no auto-unlock. See §2.4.
2. ~~Audit Trail epic ticket~~ **RESOLVED:** audit logging is not needed for this phase. See §2.7.
3. ~~`FirstFailedLoginTime` reuse~~ **RESOLVED:** confirmed — reuse the existing field as-is,
   no new field added. See §2.3/§2.4.
4. ~~Role storage~~ **RESOLVED:** no new `Role` column — reuse `ProfileId`, interpreted in
   application code. See §2.3/§2.8.

---

## 5. Verification plan (once implementation starts)

- **Unit tests** per sub-task: hash/verify round-trip, encrypt/decrypt round-trip including the
  `UsernameHash` uniqueness path, password-policy rule-by-rule rejection (all 5 rules
  independently), lockout trigger at exactly 5 + reset via unlock, reuse-of-last-3 rejection.
- **Manual walkthrough:** fresh seeded DB → log in as seeded Admin and Screener accounts →
  confirm nav differences per role → force exactly 5 failed logins → confirm the account is
  locked and stays locked (no auto-unlock) → confirm `UnlockUserAsync` clears both
  `FailedLoginAttemptCount` and `FirstFailedLoginTime` together → change password through all 5
  complexity rules individually failing/passing → attempt reuse of a recent password → log out →
  confirm re-prompt for login on next launch.
- **DB inspection:** open the SQLite file directly and confirm `ProfilePassword` is not
  reversible-looking (PBKDF2 format, §2.1); confirm `AccountName` ciphertext differs across two
  logically-identical re-encryptions (proves non-deterministic Data Protection encryption);
  confirm `UsernameHash` still uniquely identifies each user despite the encrypted column no
  longer being comparable.
