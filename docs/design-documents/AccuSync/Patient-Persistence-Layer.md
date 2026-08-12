[[_TOC_]]

# Who

Author: [Chokkalingam Shanmugam](mailto:chokka.shanmugam@solitontech.com)

# 1. Feature work item

1. ASWD-2 — Patient Management (epic)
2. ASWD-84 — Add Patient & Test Data Persistence Layer

# 2. Links to reference material

- AccuSync Software Requirements Specification, DOC-076814 — referenced clauses: GID-254883
  (create patient record), GID-254884 (view patient list), GID-254887 (patient search),
  GID-254888 (patient risk factor selection), GID-254889 (patient comment assignment),
  GID-254895 (delete patient test entry), GID-254896 (test result reassignment), GID-255042
  (patient record storage), GID-256510 (patient database encryption).
- Databases/PatientDatabase.sql — target schema reference for `Patients`, `PatientContacts`,
  `TestSessions`, and `TestRecords`.

# 3. Implementation and design

## Problem statement

The Patient module currently has no data-access layer at all:

- The patient list (`PatientsView`) is 100% hardcoded — `LoadDummyPatients()` builds a fixed set
  of in-memory rows and `AddDummyTestData()` attaches fixed test rows to a subset of them.
  Nothing is read from or written to a database.
- Three separate Patient-shaped types already exist in the codebase, none of them backed by a
  persistence entity: a light list-row model used by the grid, a heavier form view-model used by
  the detail panel, and a flat DTO used only by the device-import pipeline. Conversion between
  them is manual, inline, and written wherever a view happens to need it — there is no shared
  mapper anywhere in the solution.
- `TimestampInterceptor`, the mechanism that auto-stamps row audit timestamps, is hard-coded to
  the `User` entity only, using `long` Unix-seconds. It has no path for any other entity.
- The target schema for `Patients`, `PatientContacts`, `TestSessions`, and `TestRecords` already
  exists in `Databases/PatientDatabase.sql`, but nothing in the application reads or writes
  against it.

This design introduces the data-access layer — context, entities, repositories, and migration —
so every later Patient Management story persists and reads real data instead of mutating
in-memory collections.

## Implementation

### Two-database persistence split

A second EF Core context, `PatientDbContext`, is added alongside the existing `SettingsDbContext`,
pointed at its own file (`PatientDatabase.db`) rather than sharing `SettingsDatabase.db`. This
matches what `PatientDatabase.sql`'s own header already assumes — it documents cross-database
reference columns on the expectation that patient and settings data live in separate files — and
keeps the two domains (clinical/patient data vs. application/user configuration) independently
backed up, sized, and eventually encrypted.

### Cross-database references

Columns that reference rows in `SettingsDatabase` (`SiteId`, `AssignedUserId`, `MatchedDeviceId`,
`MatchedProtocolId`) are modeled as plain nullable integers with no EF-level foreign key or
navigation property. SQLite cannot enforce a foreign key across two separate database files;
referential validity for these columns is checked in the repository layer against
`SettingsDbContext` before a write, the same pattern the schema file's own comments already
describe.

### `Patient`, `PatientContact`, `TestSession`, `TestRecord` entities

New EF entities are added matching the existing schema column-for-column — no schema changes are
introduced by this story:

| Entity | Backing table | Purpose |
|---|---|---|
| `Patient` | `Patients` | Demographic/clinical/status fields for one patient record. Existing `PatientRiskFactors` (JSON text) and `PredefinedComments` columns are carried over unchanged — see Risk factors and comments below. |
| `PatientContact` | `PatientContacts` | One row per contact (`ContactType`: Patient / Mother / Father / Caregiver) associated with a patient; name, demographic, and address fields live here, not on `Patients`. |
| `TestSession` | `TestSessions` | Groups same-day test records for a patient. |
| `TestRecord` | `TestRecords` | One row per individual test (ABR/TEOAE/DPOAE), linked to its `TestSession`. |

All four implement a shared `IAuditableEntity` interface (`CreatedAt`, `ModifiedAt`) so timestamp
maintenance can be applied generically rather than per-entity.

Entities outside this set — `ImportBatches`, `ABRResults`, `TEOAEResults`, `DPOAEResults` — are
not modeled in this story; they belong to the later Import and OAE/ABR Test Result stories. Columns
that will eventually reference them (`ImportBatchId`) are present but treated as plain nullable
values with no navigation, the same as a cross-database reference, until those stories add the
real relationship.

### `PatientDbContext`

Mirrors `SettingsDbContext`'s existing shape: no `OnConfiguring`, provider supplied via dependency
injection, one `IEntityTypeConfiguration<T>` per entity applied in `OnModelCreating`.

### Timestamp auditing

`TimestampInterceptor`'s existing `User`-specific logic (Unix-seconds `long` fields) is left
unchanged. A second, independent pass is added that iterates `IAuditableEntity` entries in the
change tracker and stamps `CreatedAt`/`ModifiedAt` as `DateTime` values, matching the ISO-text
timestamp convention already used by the `Patients`/`PatientContacts`/`TestSessions`/`TestRecords`
schema. The two passes run side by side in the same interceptor without interacting.

### Repository layer

Two interfaces are introduced, following the existing `IUserRepository`/`UserRepository`
convention (`Task<bool>` for mutations, `Task<T?>`/`Task<List<T>>` for reads, no result-wrapper
type):

- `IPatientRepository` — database initialization/migration, paged list retrieval with optional
  search term and an `includeDeleted` flag, get-by-id (including contacts and test history),
  create, update, and soft-delete.
- `ITestRepository` — retrieving a patient's tests, deleting an individual test record, and
  reassigning a test record to a different patient (GID-254895, GID-254896).

Search covers patient ID, first/last name (via the associated `PatientContacts` row), and date of
birth, per GID-254887.

### Mapping layer

A single static class, `PatientMapper`, holds every conversion between the `Patient` entity and
the three existing UI-facing Patient shapes (list row, detail view-model, import DTO), replacing
the scattered inline object-initializers described in the problem statement. No mapping library is
introduced — nothing comparable exists elsewhere in the solution, and one entity does not justify
adding one.

### WPF wiring

`PatientsView`'s `LoadDummyPatients()` and `AddDummyTestData()` are removed; the view loads its
list from `IPatientRepository` through the mapper instead. A dev-only seeder, gated behind the
existing `DevModeConfig.IsAnyDevMode` flag (no new flag is introduced), populates a handful of
patients through the real repository on first run so development and QA still see data without a
manual import. `SplashViewModel` initializes the new database alongside the existing settings
database during the app's startup sequence.

### Risk factors and comments

Patient risk factors (GID-254888) and patient comments (GID-254889) are persisted using the
columns that already exist on `Patients` today (`PatientRiskFactors` as JSON text,
`PredefinedComments` as text) — no new table, entity, or schema change is introduced for either
in this story. Redesigning risk-factor storage to resolve its known tri-state ambiguity (see Open
issues) is left to a dedicated future story.

### Modules affected

- `AccuSync.Core` — `Patient`, `PatientContact`, `TestSession`, `TestRecord` entities;
  `IAuditableEntity`; `IPatientRepository`/`ITestRepository` abstractions; `PagedResult<T>`.
- `AccuSync.EF` — `PatientDbContext`, entity configurations, initial migration, `PatientRepository`
  / `TestRepository`, `TimestampInterceptor` extension, DI registration.
- `AccuSync.Presentation` — `PatientMapper`; `SplashViewModel` gains patient database
  initialization.
- `AccuSync.WPF` — `PatientsView` wired to the repository in place of the removed dummy-data
  methods; dependency-injection registration for the second database path.

# 4. Alternative implementations and designs

1. **A single shared database for both patient and settings data** — considered and rejected in
   favor of two separate database files. The schema file's own cross-database reference comments
   already assume a split, and keeping the domains separate avoids coupling their backup, sizing,
   and (eventual) encryption characteristics together.
2. **EF-enforced foreign keys across the two databases** — not possible with SQLite, which cannot
   enforce referential integrity across separate database files; validation for these columns is
   done at the repository layer instead.
3. **Redesigning risk-factor storage into a normalized table now** — considered, and deferred out
   of scope for this story. The existing `PatientRiskFactors` JSON column is retained as-is; see
   Open issues for the known limitation this leaves unresolved.
4. **Consolidating the three existing Patient-shaped types (list row, detail view-model, import
   DTO) onto a single type** — considered and rejected. Each serves a materially different
   purpose (lightweight list binding, validated/dirty-tracked form editing, flat import parsing),
   and forcing all three through one shape would carry unneeded overhead into the list view. A
   shared mapper is used instead so the conversion logic exists in one place without collapsing
   the types themselves.
5. **A general-purpose mapping library (e.g. AutoMapper)** — considered and rejected. Nothing
   comparable is used anywhere else in the solution, and introducing one for a single entity would
   be disproportionate to the problem being solved.

# 5. Open issues

- **Risk-factor tri-state gap (GID-254888):** the existing `PatientRiskFactors` JSON column can
  only represent "this factor is flagged," not an explicit No or Unknown answer per factor. This
  story does not resolve that gap — it is tracked for a dedicated future story.
- **Patient comment configuration (GID-254889, 5.17):** predefined/comment management screens are
  out of scope here; this story only persists whatever value is written to the existing
  `PredefinedComments` column.
- **Deleted-patient visibility:** whether a soft-deleted patient should remain visible to Admins
  via a toggle, or disappear from every view entirely, is pending a client decision. The default
  list query excludes soft-deleted patients regardless of the outcome.
- **Test entry deletion retention:** whether deleting an individual test entry should retain the
  underlying data for a period, or a hard delete is acceptable, is pending a client decision. This
  story implements a hard delete, matching the current schema (`TestRecords` has no `IsDeleted`
  column).
- **Database encryption at rest (GID-256510):** not implemented by this story. The requirement is
  file-based encryption of the database file itself, not per-field/per-record encryption of
  individual columns; no implementation approach is assumed here.
- **`ImportBatches`, `ABRResults`, `TEOAEResults`, `DPOAEResults`:** not modeled in this story;
  anticipated as part of the later Import and OAE/ABR Test Result stories.
