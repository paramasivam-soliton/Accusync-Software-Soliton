[[_TOC_]]

# Who

Author: [Chokkalingam Shanmugam](mailto:chokka.shanmugam@solitontech.com)

Status: **Implemented on this branch.** Section 3's wide-table recommendation was confirmed
and built as-is; section 4 below is updated to describe what actually shipped, since the
repository shape changed slightly during implementation (see note at the top of that section).

# 1. Current state (as implemented so far on this branch)

- `UserRole` is a C# enum (`AccuSync.Core/Entities/UserRole.cs`): `Screener = 0`, `Admin = 1`.
- `User.ProfileId` (`AccuSync.Core/Entities/User.cs`) is a plain `string` column, populated with
  the literal value `"Admin"` or `"Screener"`.
- `UserRoleParser.Parse(string profileId)` (`AccuSync.Application/Helpers/UserRoleParser.cs`)
  interprets that string as a `UserRole`: anything other than an exact, case-insensitive
  `"Admin"` match resolves to `Screener`.
- `ICurrentUserContext` (`AccuSync.Core/Abstractions/Services/ICurrentUserContext.cs`) exposes
  the signed-in user's `UserRole` as a property, populated at `SignIn`.
- `IUserRepository.UpdateUserRoleAsync(string userGuid, UserRole role)` is the only way to
  assign a role — it only accepts one of the two enum values.
- `UserPermissionsViewModel` (`AccuSync.Presentation/ViewModels/UserPermissionsViewModel.cs`)
  already models what a permission set looks like: 14 independent boolean flags (nav visibility
  for Dashboard/Patients/Users/Sites/Devices/SysConfig/Settings/About, plus
  Add/Edit/Delete/Export/Import Patient and View Reports), with two hardcoded static presets,
  `Admin()` and `Screener()`. It is not wired to `SidebarNavigation` yet — that wiring is still
  open work regardless of which model (enum or data-driven) backs it.

In short: the permission *shape* (the 14 flags above) already exists in code and is a reasonable,
grounded starting catalog. What doesn't exist is any way to store a *named combination* of those
flags anywhere other than as a hardcoded C# method.

# 2. Why the enum doesn't extend to custom profiles

An admin creating a new profile — e.g. "Screener II" with reporting access but no export — is a
runtime data-entry action. A C# enum value is a compile-time constant: adding one means a code
change, a rebuild, and a redeploy for every clinic site, which defeats the point of an
admin-facing feature. The enum can express "this user is one of the two roles we shipped," but
it cannot express "this user is whatever profile an admin assembled after installation."

This is also not a new conclusion — the target schema this project has referenced since its
first commit (`Databases/SettingsDatabase.sql`) already models a real `Profiles` table with a
`Users.ProfileId` foreign key into it, not a role string. The enum was always understood as an
interim stand-in for that table, not a replacement for it — see also the existing HLD's
alternatives section, which explicitly flagged this. The lead's direction is to stop treating it
as interim and build the real thing now, minus its UI.

# 3. Proposed target model

Replace the enum with a real, data-backed `Profile`:

| Table | Columns | Purpose |
|---|---|---|
| `Profiles` | `Id` (PK), `Name` (unique, display string), `IsSystemDefined` (bool), plus one boolean column per permission flag currently on `UserPermissionsViewModel` | One row per assignable profile — seeded with two system-defined rows, `Admin` and `Screener`, matching today's two presets exactly. Additional rows are what an admin creates later through the (future) Profile Management screen. |
| `Users` | `ProfileId` becomes a real foreign key into `Profiles.Id`, instead of a free-text role-name string | Matches the target schema referenced above. |

`IsSystemDefined = true` on the seeded `Admin`/`Screener` rows exists so a future management UI
can refuse to let an admin delete or rename the two built-in profiles, without needing a second
mechanism to identify them.

**Permission shape stays a fixed, known catalog for now** (a wide table, one column per
permission), not a fully dynamic permission-catalog-plus-join-table model where new *kinds* of
permission could be invented at runtime. The product need described so far is "assign users to
admin-defined *combinations* of a known set of permissions," not "let an admin invent new kinds
of permission the software doesn't otherwise understand." A wide table is simpler to query, seed,
and test, and a join-table model can still be introduced later — as its own migration — if that
turns out to be needed; it isn't blocked by choosing the simpler shape now. **This was the one
design choice in this document flagged for explicit sign-off**, and it was — a first
implementation pass tried the join-table model anyway (reasoning that adding a permission later
would then be a data change, not a schema change), but that turned out to be a false economy:
adding permission #34 needs a migration either way — either a new column, or a new seed row —
so the extra table bought schema flexibility for a case that doesn't actually need it. Reverted
to the wide table as originally proposed here.

# 4. What changes, concretely (as actually implemented)

- **New**: `Profile` entity (`AccuSync.Core/Entities/Profile.cs`) with `Id`, `Name`,
  `Description`, `IsSystemDefined`, and 33 permission flags — one bool column per permission in
  the real catalog found in `ProfilesContentView.xaml.cs` (§3), grouped by component
  (`AccuScreenAllTests`, `PatientsView`, `UsersProfilesAddEditUsers`, etc.), not the smaller
  14-flag `UserPermissionsViewModel` shape this document originally assumed before that catalog
  was found.
- **New**: `IProfileRepository` / `ProfileRepository` (`AccuSync.Core.Abstractions.Repositories`
  / `AccuSync.EF`) — `GetAllProfilesAsync`, `GetProfileByIdAsync`, `CreateProfileAsync(Profile)`,
  `UpdateProfileAsync(Profile)`. The update method takes the whole `Profile` (name, description,
  and every permission flag) and replaces the stored row wholesale — the same pattern
  `IUserRepository.UpdateUserAsync` already uses — rather than a separate permission-id list
  parameter, since permissions are just properties on the entity now.
  - `DeleteProfileAsync` is deliberately **not** included yet — deleting a profile that users are
    still assigned to needs a reassignment-or-block decision that has no UI to drive it yet;
    adding it later is a smaller change than removing it if the wrong default gets picked now.
    `UserConfiguration`'s FK from `Users.ProfileId` to `Profiles.Id` uses `DeleteBehavior.Restrict`
    for the same reason.
- **Changed**: `User.ProfileId` — stays named `ProfileId`, but changes from an encrypted
  role-name string to a real, unencrypted FK value (`string`, matching `Profile.Id`).
  `UserRepository` no longer encrypts/decrypts this column.
- **Changed**: `IUserRepository.UpdateUserRoleAsync(string userGuid, UserRole role)` becomes
  `UpdateUserProfileAsync(string userId, string profileId)` — an admin exception method, same as
  today, just accepting any profile id instead of one of two enum values.
- **Changed**: `ICurrentUserContext` — `Role` (`UserRole`) is replaced by exposing the signed-in
  user's whole `Profile` row directly (loaded via `IProfileRepository.GetProfileByIdAsync` at
  sign-in), instead of an enum the caller has to re-interpret. Call sites that today check
  `Role == UserRole.Admin` will, once wired to a screen, check the relevant permission flag
  instead (e.g. `Profile.UsersProfilesAddEditUsers`) — the correct long-term check regardless of
  this migration, since "is Admin" was always really shorthand for "has admin-only permissions."
- **Removed**: `UserRoleParser` and the `UserRole` enum — no longer needed once profile is a real
  row lookup rather than a string to interpret.
- **Migration**: one flattened `AccuSync.EF` migration adding `Profiles` and updating `Users`,
  seeding the two system-defined profiles — Admin with every permission flag `true`, Screener
  with the same 13-of-33 subset `ProfilesContentView.xaml.cs`'s `BuildScreenerPermissions()`
  already grants — so behavior is unchanged for this PR's own acceptance criteria; only the
  storage mechanism changes. Verified with a fresh-database `dotnet ef database update` smoke
  test and a direct row check of both seeded profiles' flags.

# 5. What stays deferred (unchanged from the existing HLD's open issues)

- No Profile Management screen. Profiles are seeded, the same way `Users` are currently
  provisioned by seed rather than through a UI.
- `SidebarNavigation` wiring to a permissions object was already open work before this proposal
  and stays open work after it — this document only changes what that object is loaded from.
- A dynamic, admin-inventable permission catalog (§3) is explicitly out of scope unless a future
  requirement shows the fixed-catalog assumption is wrong.

# 6. Open questions

1. Does a fixed, known permission catalog (the 33 flags in §4) match what's actually needed, or
   is there a near-term requirement for admins to define entirely new *kinds* of permission
   (which would change §3's recommendation)? Resolved for this PR as "fixed catalog" — flagged
   here in case that assumption needs revisiting later.
2. `Profiles.Id` type — resolved as `string`, using the profile name itself (`"Admin"`,
   `"Screener"`) as the id, consistent with how `User.ProfileId` already stored these values
   before this change.