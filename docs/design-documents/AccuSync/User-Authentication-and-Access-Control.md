[[_TOC_]]

# Who

Author: [Chokkalingam Shanmugam](mailto:chokka.shanmugam@solitontech.com)

# 1. Feature work item

1. ASWD-1 — Login (epic)
2. ASWD-31 — Implement Credential Encryption
3. ASWD-36 — Implement Role-Based Access Control
4. ASWD-41 — Implement User Login and Deactivated User Login Prevention
5. ASWD-45 — Implement User Account Lockout and Password Complexity Requirements
6. ASWD-50 — Implement User Logout

# 2. Links to reference material

- AccuSync Software Requirements Specification, DOC-076814 — referenced clauses: GID-255017
  (failed-attempt lockout threshold), GID-254911 (configurable lockout duration), GID-254907
  (administrator override/unlock).
- Databases/SettingsDatabase.sql — target schema reference for the `Users` table.

# 3. Implementation and design

## Problem statement

User accounts, credentials, and sessions in AccuSync are handled by authentication code
inherited from the original proof-of-concept, which has several properties unsuitable for a
production, healthcare-adjacent desktop application:

- Passwords are stored as reversible, symmetrically-encrypted values and compared by decrypting
  and matching plaintext, rather than being hashed one-way.
- The encryption key is derived from the machine name plus a hardcoded string, with no key
  management.
- The initialization vector is static and reused across every encryption operation, which
  defeats the security guarantee of the cipher mode in use.
- Encryption of stored values is a no-op outside Release builds, so non-Release builds persist
  plaintext credentials.
- Encryption/decryption failures are swallowed and silently fall back to storing/returning the
  unencrypted value.
- There is no role concept: administrative vs. non-administrative behavior is determined by
  string-matching an account name against the literal value `"Admin"` in more than one place.
- Account lockout thresholds and duration are hardcoded, with no administrative control.
- There is no single, authoritative representation of "who is currently signed in" — the signed-in
  username is passed manually between navigation calls as a plain string.
- No audit trail of authentication-related events (login, logout, lockout, role or status
  changes) exists.

This design addresses credential storage, authentication, role-based access control, account
lockout, password complexity, and session/logout handling as one cohesive body of work, since
each later concern depends on the credential-storage fix that precedes it.

## Implementation

### Password storage

Passwords are hashed one-way using PBKDF2-HMAC-SHA256 (`Rfc2898DeriveBytes.Pbkdf2`,
`System.Security.Cryptography`, built into .NET — no third-party dependency), at 210,000
iterations, the OWASP-recommended minimum for this algorithm. PBKDF2 is used in preference to
BCrypt or Argon2 because it has FIPS 140-validated implementations available on Windows, relevant
to healthcare-adjacent software, and avoids introducing a third-party hashing package.

A fresh, cryptographically random 128-bit salt (`RandomNumberGenerator.GetBytes`) is generated
for every hash operation — at initial account seeding and at every password change — and is never
derived from or reused with any other value. Iteration count, salt, and derived hash are stored
together as one self-describing string:

```
{iterations}.{base64(salt)}.{base64(hash)}
```

Verification (`IPasswordHasher.Verify`) re-derives a hash from the supplied password using the
salt and iteration count embedded in the stored value and compares the two hashes in constant
time (`CryptographicOperations.FixedTimeEquals`). Authentication and password-change logic
compare hashes only; stored password values are never decrypted or compared as plaintext.

The same mechanism secures the three-entry password-reuse history (`LastThreePasswords`), which
holds hashes rather than encrypted or plaintext values.

### Username storage

Usernames must remain human-readable — the login screen populates a dropdown of active account
names — so usernames are encrypted (reversible), not hashed. Encryption uses the ASP.NET Core
Data Protection API (`Microsoft.AspNetCore.DataProtection`), consumed as a standalone
library — it requires no web server, hosting, or MVC pipeline, and works from any .NET
application type. Data Protection is active in every build configuration, including Debug
builds, and its key ring is persisted to a folder alongside the application database so both
travel together if the database is backed up or moved to another machine.

Because Data Protection's `Protect()` operation is non-deterministic — encrypting the same
username twice produces two different ciphertexts, by design — the encrypted `AccountName`
column cannot support an equality-based uniqueness constraint or lookup. A companion column,
`UsernameHash`, holds a deterministic SHA-256 hash of the normalized (trimmed, lower-cased)
username and carries the table's unique index. This is a standard "blind index" pattern for
enforcing uniqueness or supporting lookup on a field that must otherwise be stored encrypted.
Login and account lookups compute the incoming username's hash, locate the matching row by
`UsernameHash`, and decrypt only that row for display or comparison — no row is ever decrypted
merely to search it.

### `User` entity

| Field | Type | Purpose |
|---|---|---|
| `Guid` | `string` | Primary key. |
| `AccountName` | `string` | Encrypted username (Data Protection API). |
| `UsernameHash` | `string` | Deterministic hash of the normalized username; carries the unique index and drives lookup. |
| `FirstName`, `LastName` | `string` | Encrypted display name fields. |
| `IsActive` | `bool` | Whether the account can authenticate. `false` (deactivated) is rejected at login regardless of credential correctness. |
| `ProfileId` | `string` | Holds the account's role (`"Admin"` / `"Screener"`), interpreted in application code — see Role-based access control below. |
| `ProfilePassword` | `string` | One-way password hash (§ Password storage). Column name is retained from the prior schema; only its contents change. |
| `LastThreePasswords` | `string` | Pipe-delimited history of the three most recent password hashes, used for reuse rejection. |
| `FirstLogin` | `int` (0/1) | Forces a mandatory password change on the account's first successful login. |
| `FailedLoginAttemptCount` | `int` | Consecutive failed login attempts since the last success or unlock; the sole counter driving lockout. |
| `FirstFailedLoginTime` | `long` (Unix seconds, UTC) | Timestamp of the failure that triggered the current lockout; combined with the configured duration to determine when the account auto-unlocks. |
| `PasswordModificationDate` | `long` (Unix seconds, UTC) | Used by the existing 90-day password-expiration check. |
| `CreationDate`, `ModificationDate` | `long` (Unix seconds, UTC) | Row audit timestamps, set automatically on insert/update. |
| `FailedResetAttemptCount`, `FirstResetLoginTime` | `int` / `long` | Retained, currently unused by any implemented feature — see Open issues. |

A companion single-row table, `AppSettings`, holds application-wide configuration:

| Field | Type | Purpose |
|---|---|---|
| `Id` | `string` | Fixed value `"Default"` — the table has exactly one row. |
| `LockoutDurationMinutes` | `int` | Administrator-configurable lockout duration in minutes (default 15). |

### Role-based access control

A two-value role model is used: `UserRole { Screener = 0, Admin = 1 }`, with `Screener` as the
default (least-privileged) value. No new schema is introduced for this — the existing, previously
unused `ProfileId` string column is populated with the literal value `"Admin"` or `"Screener"`
and interpreted by a small parsing helper wherever a role-based decision is needed; anything other
than an exact, case-insensitive match on `"Admin"` resolves to `Screener`.

`ICurrentUserContext` is an in-memory, process-lifetime singleton that holds the identity and
role of the currently signed-in user. It is populated once, at successful sign-in, and cleared
on logout; a role change made to an account takes effect the next time that account signs in, not
live against a session already in progress.

The signed-in user's role determines:
- Which of two distinct dashboard shells is shown after login (an administrative dashboard vs. a
  screener-focused dashboard).
- Which navigation items and features are enabled within that dashboard, driven by a permissions
  object with named presets per role (full access for Admin; patient-workflow-only, no
  administrative navigation, for Screener).

### Login

The login screen presents a dropdown populated only with active (`IsActive = true`) account names,
alongside a password field. On submission, authentication proceeds through the following checks,
in order, for the supplied account name and password:

1. Reject if either field is empty, with a generic "required fields" message.
2. Look up the account by its username hash; an unmatched account name returns the same generic
   invalid-credentials message used for a wrong password, so a caller cannot distinguish "account
   does not exist" from "wrong password" (prevents account enumeration).
3. Reject a deactivated account (`IsActive = false`) with the same generic invalid-credentials
   message, regardless of whether the supplied password is correct.
4. Evaluate lockout state (see Account lockout, below); a locked account is rejected with a
   message stating the remaining wait time.
5. Verify the supplied password against the stored hash. A mismatch increments the failure
   counter and returns a message indicating the number of attempts remaining before lockout.
6. Reject if the account's password has exceeded its configured expiration age.
7. On success, reset the failure counter, populate `ICurrentUserContext` with the account's
   identity and role, and route to the appropriate dashboard. If the account's first-login flag
   is set, a mandatory password change is required before the dashboard is shown.

### Password complexity and reuse

A new password is evaluated against five independent rules, each surfaced to the user as a
real-time pass/fail indicator as they type: minimum length of 8 characters, at least one
uppercase letter, at least one lowercase letter, at least one digit, and at least one
non-alphanumeric (special) character. The new password must also differ from the account's
current password.

On save, the new password is additionally checked against the account's three most recently used
password hashes and rejected if it matches any of them. On a successful change, the previous
password hash is added to that three-entry history (oldest entry dropped), the password
modification timestamp is updated, and the first-login flag is cleared.

### Account lockout

An account is locked after 5 consecutive failed login attempts — the same threshold for every
account, with no exception for administrator accounts. Lockout state is derived entirely from
`FailedLoginAttemptCount` and `FirstFailedLoginTime`; no separate boolean lock flag exists to
keep in sync.

The lockout duration is an administrator-configurable, application-wide value (default 15
minutes), stored in the `AppSettings` table. Once the configured duration has elapsed since the
lockout began, the next login attempt against that account automatically clears the failure
counter and lockout timestamp and proceeds with a normal password check — no administrative
action is required for this case. Independently of the timer, an administrator can clear a
lockout for a specific account immediately, before the duration elapses, as an explicit override
that operates alongside the automatic expiry rather than replacing it.

A successful login always resets the failure counter to zero.

### Session management and logout

`ICurrentUserContext` is the single authoritative source for the currently signed-in account and
role for the lifetime of the application process, replacing the prior pattern of passing the
signed-in username as a string parameter through navigation calls. Logout is reachable from the
persistent navigation available on every dashboard screen; selecting it prompts for confirmation,
and confirming clears the session context and returns to the login screen. Any further access to
the application requires signing in again.

A session is also ended automatically after a fixed period of no mouse or keyboard activity
anywhere in the application (15 minutes). Idle detection (`InactivityPolicy`) is a
framework-agnostic policy in `AccuSync.Application`, decoupled from the WPF-specific code that
reports activity and reacts to expiry (`InactivityMonitor`, `AccuSync.WPF`); on expiry, it invokes
the same session-clearing, return-to-login path used by manual logout, without a confirmation
prompt.

### Users and system-configuration administration

Full administrative screens for managing user accounts (create/edit/deactivate, role assignment)
and system-wide configuration (including the lockout duration) are not part of this design and
remain their existing mock/placeholder presentation; user accounts are provisioned by database
seed. In their place, this design provides real, testable administrative operations at the
repository layer — assigning a role, activating/deactivating an account, clearing a lockout, and
setting the lockout duration — usable by an administrator or test harness ahead of the
corresponding UI being built.

### Modules affected

- `AccuSync.Core` — `User` and `AppSettings` entities; `UserRole` enum; authentication,
  password-hashing, encryption, current-user-context, and repository abstractions.
- `AccuSync.Application` — password hasher, encryption service, authentication service,
  current-user-context implementation, role-parsing helper, inactivity policy.
- `AccuSync.EF` — user and app-settings repositories and EF configurations; schema migrations
  for the username blind index, the `Status`→`IsActive` column (type and rename), and the new
  `AppSettings` table.
- `AccuSync.Presentation` — login, change-password, and permissions view models.
- `AccuSync.WPF` — login and change-password windows, dashboard shells, application startup and
  navigation/logout wiring, dependency-injection registration, inactivity monitoring
  (activity detection, reacting to session expiry).

# 4. Alternative implementations and designs

1. **BCrypt or Argon2 for password hashing** — considered and rejected in favor of
   PBKDF2-HMAC-SHA256. Neither has a FIPS 140-validated implementation available on Windows,
   relevant to healthcare-adjacent software, and both would add a third-party dependency where
   the .NET base class library already provides an industry-accepted adaptive hash.
2. **Windows DPAPI for username encryption** — considered and rejected in favor of the ASP.NET
   Core Data Protection API. DPAPI is Windows-only and, at machine scope, binds encrypted data to
   the specific machine it was encrypted on, preventing the database from being relocated to
   another machine and still being readable.
3. **Decrypt-and-scan every row for username lookup/uniqueness** — considered and rejected. It
   does not scale with the number of accounts, and more importantly provides no
   database-enforced uniqueness guarantee at write time, only a slow, race-prone
   application-level check.
4. **Deterministic or order-preserving encryption of the username field**, to keep it directly
   comparable — considered and rejected, since it reintroduces the weakness being fixed:
   identical plaintexts producing identical or comparable ciphertext leaks patterns and defeats
   semantic security.
5. **A new, discrete `Role` column on the `User` table** — considered and rejected in favor of
   reusing the existing, previously unused `ProfileId` column. A future, richer profile/permission
   system will require a data-migration step regardless of which path is taken now; reusing
   `ProfileId` avoids introducing an additional column that would also need to be reconciled at
   that point.
6. **A discrete `IsLocked` flag (with a separate `LockedAt` timestamp)** — considered and
   rejected in favor of deriving lockout state entirely from `FailedLoginAttemptCount` and
   `FirstFailedLoginTime`, avoiding a second piece of state that could drift out of sync with the
   attempt counter.
7. **A hardcoded lockout duration** — considered and rejected in favor of an
   administrator-configurable value held in a dedicated settings table, since the duration must
   be adjustable without a code change.
8. **Inactivity detection and expiry logic implemented directly in `AccuSync.WPF`** — considered
   and rejected in favor of a framework-agnostic policy in `AccuSync.Application`
   (`InactivityPolicy`), with WPF limited to reporting input activity and reacting to expiry
   (`InactivityMonitor`). This matches how other session state (`ICurrentUserContext`) is already
   implemented in `AccuSync.Application` rather than WPF, and keeps the expiry decision
   unit-testable without a running WPF `Application`.

# 5. Open issues

- The existing 90-day password-expiration check is retained unchanged and is not part of this
  design effort.
- Full administrative screens for user management (create/edit/deactivate accounts, role
  assignment) and system configuration (including editing the lockout duration through a screen)
  remain mock/placeholder; only the underlying repository operations are implemented. Building
  these screens is anticipated as separate future work.
- Audit logging of authentication-related events (login success/failure, logout, lockout,
  role or status changes) is not implemented in this design. A fixed vocabulary of login-failure
  reasons exists in code for forward compatibility, but no persistence or emission mechanism
  exists yet; this is anticipated as a dedicated future effort once an audit-logging mechanism is
  defined for the application as a whole.
- Self-service password reset (as opposed to an administrator-driven change) is not implemented;
  a locked-out or expired account currently requires administrator intervention. Two fields on
  the `User` entity (`FailedResetAttemptCount`, `FirstResetLoginTime`) appear reserved for a future
  reset flow and are retained but not currently used by any implemented feature.
- A richer, granular permission-bundle model (as opposed to the two-value Admin/Screener role
  used here) is described in broader product requirements but is not implemented by this design.
- The automatic-logout idle timeout (15 minutes) is currently a fixed value; making it
  administrator-configurable, matching the existing `AppSettings`-driven lockout duration, is
  anticipated future work.
