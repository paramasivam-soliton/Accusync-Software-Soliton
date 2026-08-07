# Patient Persistence Layer — High-Level Design (ASWD-84)

**Purpose:** Pre-development design for ASWD-84 ("Add Patient & test data persistence Layer"),
the first story under the Patient Management epic (ASWD-2). This document exists to get every
design decision validated **before** coding starts, so no architectural question needs to be
raised mid-development.

**Source documents:**
- AccuSync Software Requirements (DOC-076814, Rev 01) — 5.3 Patient Management, 5.30 Database
  Storage.
- `Databases/PatientDatabase.sql` — target schema (as it exists today, before this story's
  changes).
- ASWD-84 acceptance criteria, as written by the lead.
- Current shipped code on `users/chokkalingam/feat/AkkuSync-US5-Logout` (the branch point) —
  `SettingsDbContext`, `IUserRepository`/`UserRepository`, `TimestampInterceptor`,
  `SqliteServiceCollectionExtensions`, `DevModeConfig` — used as the precedent this story mirrors.

**Revision history:**
- 2026-08-07 — initial draft, pending lead review.

**Status:** All open items in 11 need an explicit yes/no from the lead (and, for two of them,
the client) before implementation starts. Everything else in this document is proceeding as
written.

---

## 1. Current-state summary

| Area | Current state |
|---|---|
| Patient list (`PatientsView`) | 100% hardcoded — `LoadDummyPatients()` builds 10 fixed rows, `AddDummyTestData()` attaches fixed test rows to 3 of them. No database involved. |
| Patient-shaped types | **Three separate types already exist**, none of them an EF entity: `AccuSync.Application.Models.Patient` (light list-row model, used by `PatientsView`'s grid), `AccuSync.Presentation.ViewModels.PatientViewModel` (heavy form VM — validation, dirty-tracking, QR generation — used by the detail panel), `AccuSync.Core.Entities.PatientData` (flat ~80-field DTO, used only by the device-import pipeline). |
| Mapping between those types | Manual, inline, object-initializer-based, written wherever a View happens to need it (e.g. `PatientsView.PatientsListView_SelectionChanged`). No shared mapper exists anywhere in the solution, for any entity. |
| Target schema | Already defined in `Databases/PatientDatabase.sql`: `ImportBatches`, `Patients`, `PatientContacts`, `TestSessions`, `TestRecords`, `ABRResults`, `TEOAEResults`, `DPOAEResults`, plus indexes. Patient name fields (`Forename1`, `Surname`, etc.) live on `PatientContacts` (`ContactType = 'Patient'`), **not** on `Patients` itself. |
| Persistence precedent | `SettingsDbContext` (Users, AppSettings) is fully shipped: no `OnConfiguring`, provider injected via DI, one `IEntityTypeConfiguration<T>` per entity, wired through `SqliteServiceCollectionExtensions.AddSqlitePersistence(...)`. `IUserRepository`/`UserRepository` is the repository pattern to mirror — plain `Task<bool>`/`Task<T>` returns, no Result-wrapper type, `InitializeDatabaseAsync()` does a backup-then-migrate-with-rollback dance before `MigrateAsync()`. |
| Cross-database references | `PatientDatabase.sql`'s own header already documents the pattern: SQLite can't FK across database files, so columns referencing `SettingsDatabase` (`SiteId`, `AssignedUserId`, `MatchedDeviceId`, etc.) are plain `INTEGER` with an `-- [XDB: SettingsDatabase.Table.Column]` comment; referential integrity is enforced at the application layer. |
| `TimestampInterceptor` | Exists, but hard-coded to `User` only, using `long` Unix-seconds. The `Patient*` tables use `TEXT` ISO-datetime `CreatedAt`/`ModifiedAt` columns instead — the two conventions don't match today. |
| `DevModeConfig` | File-flag based (`skip_login.flag`, `skip_dashboard.flag` under `%ProgramData%\AccuSync\`), exposes `IsAnyDevMode`. This is the existing "dev-mode bypass" the story's dev-seeder should reuse. |

---

## 2. Decisions carried into this story

| # | Decision | Chosen approach | Why |
|---|---|---|---|
| 1 | One database or two | **Two** — new `PatientDbContext` → `PatientDatabase.db`, alongside existing `SettingsDbContext` → `SettingsDatabase.db` | Matches what's already shipped; `PatientDatabase.sql`'s own header already assumes this split (cross-DB `[XDB:]` annotations); SRS lists AES-256 encryption as two separate requirements (GID-256510, GID-256511) — one per database. |
| 2 | Cross-DB references (`SiteId`, `AssignedUserId`, `MatchedDeviceId`, `MatchedProtocolId`) | Plain `int?` columns, validated at the repository layer against `SettingsDbContext` before write. No EF navigation/FK across contexts. | SQLite cannot enforce FKs across separate database files; this is the pattern the schema file already documents. |
| 3 | Entity scope for this story | **In scope:** `Patients`, `PatientContacts`, `TestSessions`, `TestRecords`, plus a new `PatientRiskFactorValues` table. **Out of scope:** `ImportBatches`, `ABRResults`, `TEOAEResults`, `DPOAEResults`. | Those four tables belong to the later Import and OAE/ABR Test Result stories. This story's code is still built so those can be added later without rework (`ImportBatchId`). |
| 4 | Whole-database encryption (GID-256510) | **Out of scope for this story.** `PatientDatabase.db` is created unencrypted, same current state as `SettingsDatabase.db`. | No existing pattern to mirror (Settings DB doesn't do whole-DB encryption either — only per-field encryption of `Username`), and it isn't in this story's acceptance criteria. Tracked as a known gap, not silently dropped. |
| 5 | Risk-factor storage | **Join table now** (`PatientRiskFactorValues`), replacing the `Patients.PatientRiskFactors` JSON column — for the schema change. | Solves the Yes/No/Unknown tri-state gap (GID-254888) that the JSON-array-of-codes design can't represent. Doing it in this story avoids building JSON mapping now and ripping it out later.|
| 6 | List / detail / import type split | **Keep all three existing types** (`Application.Models.Patient` for the list grid, `PatientViewModel` for the detail/edit panel, `PatientData` for import), each produced by one shared mapper (see 7). | `PatientViewModel`'s validation/dirty-tracking/QR-generation machinery isn't meant for bulk list binding; forcing every list row through it would be wasteful. The acceptance criteria's wording ("PatientViewModel / PatientData") most likely didn't account for the third type already in use — not a deliberate instruction to remove it. |

---

## 3. Schema changes

All changes are additive/replacing within `PatientDatabase.sql`'s existing `Patients` section — no other table changes.

**Remove:**
```sql
PatientRiskFactors  TEXT,       -- JSON array of RiskFactor.Code values
```
from `Patients` (column dropped in the initial migration — no production data exists yet for
this database, so there is nothing to migrate).

**Add** (new table, new section in the `.sql` file):
```sql
-- ============================================================
-- SECTION 2A: PATIENT RISK FACTOR VALUES
-- One row per (Patient, RiskFactor) pair the user has explicitly
-- set. RiskFactorId codes must match rows in
-- SettingsDatabase.RiskFactors (cross-database, no FK — same
-- pattern as Patients.SiteId/AssignedUserId).
-- Value is tri-state: 'Yes' | 'No' | 'Unknown'.
-- ============================================================

CREATE TABLE IF NOT EXISTS PatientRiskFactorValues (
    PatientRiskFactorValueId INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientId                INTEGER NOT NULL REFERENCES Patients(PatientId),
    RiskFactorId             INTEGER NOT NULL,    -- [XDB: SettingsDatabase.RiskFactors.RiskFactorId]
    Value                    TEXT    NOT NULL,    -- 'Yes' | 'No' | 'Unknown'
    CreatedAt                TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt                TEXT    NOT NULL DEFAULT (datetime('now')),
    UNIQUE (PatientId, RiskFactorId)
);

CREATE INDEX IF NOT EXISTS IX_PatientRiskFactorValues_PatientId    ON PatientRiskFactorValues(PatientId);
CREATE INDEX IF NOT EXISTS IX_PatientRiskFactorValues_RiskFactorId ON PatientRiskFactorValues(RiskFactorId);
```

**Note on `ImportBatchId`** (present on `Patients`, `TestSessions`, `TestRecords`): since
`ImportBatches` itself is out of scope, these columns are modeled as plain
nullable `int?` with **no EF-level FK/navigation** for now — treated the same as a cross-DB
reference even though `ImportBatches` will eventually live in this same database. The Import
story adds the real FK once `ImportBatches` is modeled.

---

## 4. EF layer design

### 4.1 Entities (`AccuSync.Core/Entities/`)

New classes, matching the (updated) schema column-for-column: `Patient`, `PatientContact`,
`TestSession`, `TestRecord`, `PatientRiskFactorValue`.

All five implement a new shared marker interface:
```csharp
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime ModifiedAt { get; set; }
}
```

### 4.2 `PatientDbContext` (`AccuSync.EF/Contexts/PatientDbContext.cs`)

Mirrors `SettingsDbContext` exactly — no `OnConfiguring`, provider injected via DI:

```csharp
public class PatientDbContext : DbContext
{
    public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientContact> PatientContacts => Set<PatientContact>();
    public DbSet<TestSession> TestSessions => Set<TestSession>();
    public DbSet<TestRecord> TestRecords => Set<TestRecord>();
    public DbSet<PatientRiskFactorValue> PatientRiskFactorValues => Set<PatientRiskFactorValue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PatientConfiguration());
        modelBuilder.ApplyConfiguration(new PatientContactConfiguration());
        modelBuilder.ApplyConfiguration(new TestSessionConfiguration());
        modelBuilder.ApplyConfiguration(new TestRecordConfiguration());
        modelBuilder.ApplyConfiguration(new PatientRiskFactorValueConfiguration());
    }
}
```

### 4.3 `TimestampInterceptor` extension

The existing `User`-only block is **left untouched**. A second, independent block is added for
the new entities, using `DateTime` (matching the schema's `TEXT` ISO-datetime convention, not
`User`'s `long` Unix-seconds):

```csharp
private void ApplyTimestamps(DbContext context)
{
    if (context == null) return;

    // Existing — unchanged
    long nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    foreach (var entry in context.ChangeTracker.Entries<User>())
    {
        if (entry.State == EntityState.Added) { entry.Entity.CreationDate = nowUnix; entry.Entity.ModificationDate = nowUnix; }
        else if (entry.State == EntityState.Modified) { entry.Entity.ModificationDate = nowUnix; }
    }

    // New — generic, covers Patient/PatientContact/TestSession/TestRecord/PatientRiskFactorValue
    var nowUtc = DateTime.UtcNow;
    foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
    {
        if (entry.State == EntityState.Added) { entry.Entity.CreatedAt = nowUtc; entry.Entity.ModifiedAt = nowUtc; }
        else if (entry.State == EntityState.Modified) { entry.Entity.ModifiedAt = nowUtc; }
    }
}
```

### 4.4 DI wiring (`SqliteServiceCollectionExtensions`)

`AddSqlitePersistence` gains a second path parameter and wires the second context alongside the
first, in the same method (per the acceptance criteria — "registered ... alongside
SettingsDbContext"):

```csharp
public static IServiceCollection AddSqlitePersistence(
    this IServiceCollection services, string settingsDatabasePath, string patientDatabasePath)
{
    services.AddSingleton<TimestampInterceptor>();

    services.AddDbContext<SettingsDbContext>((provider, options) =>
    {
        options.UseSqlite($"Data Source={settingsDatabasePath};Default Timeout=5", b => b.MigrationsAssembly("AccuSync.EF"));
        options.AddInterceptors(provider.GetRequiredService<TimestampInterceptor>());
    });

    services.AddDbContext<PatientDbContext>((provider, options) =>
    {
        options.UseSqlite($"Data Source={patientDatabasePath};Default Timeout=5", b => b.MigrationsAssembly("AccuSync.EF"));
        options.AddInterceptors(provider.GetRequiredService<TimestampInterceptor>());
    });

    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
    services.AddScoped<IPatientRepository, PatientRepository>();
    services.AddScoped<ITestRepository, TestRepository>();

    return services;
}
```

The call site (`App.xaml.cs`) resolves the second path the same way it resolves the first —
`Persistence:PatientDatabasePath` in `appsettings.json`, falling back to
`%ProgramData%\Natus\AccuSync\PatientDatabase.db` per the acceptance criteria.

### 4.5 Migration

One migration, same flat folder as `SettingsDbContext`'s migrations
(`AccuSync.EF\Migrations\`, differentiated by `[DbContext(typeof(PatientDbContext))]` on its own
`PatientDbContextModelSnapshot.cs`) — EF Core keeps all contexts' migrations in one
`MigrationsAssembly`, this is not a new folder/convention.

`PatientRepository.InitializeDatabaseAsync()` replicates `UserRepository`'s existing
backup-then-migrate-with-rollback dance (back up the `.db` file, `MigrateAsync()`, restore on
failure, clear a stale `__EFMigrationsLock` row) rather than calling `MigrateAsync()` bare.

---

## 5. Repository layer

Two interfaces per the acceptance criteria, split by what they operate on:

**`AccuSync.Core/Abstractions/Repositories/IPatientRepository.cs`** — Patients + PatientContacts + PatientRiskFactorValues:
```csharp
public interface IPatientRepository
{
    Task InitializeDatabaseAsync();
    Task<PagedResult<Patient>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool includeDeleted = false);
    Task<Patient?> GetByIdAsync(int patientId);   // includes PatientContacts, risk factor values, and test summary
    Task<bool> CreateAsync(Patient patient);
    Task<bool> UpdateAsync(Patient patient);
    Task<bool> SoftDeleteAsync(int patientId);
}
```

**`AccuSync.Core/Abstractions/Repositories/ITestRepository.cs`** — TestSessions + TestRecords:
```csharp
public interface ITestRepository
{
    Task<List<TestRecord>> GetTestsForPatientAsync(int patientId);
    Task<bool> DeleteTestRecordAsync(int testRecordId);                       // GID-254895
    Task<bool> ReassignTestRecordAsync(int testRecordId, int newPatientId);   // GID-254896
}
```

**Search fields** (`GetPagedAsync`'s `searchTerm`) cover patient ID, first/last name (joined from
`PatientContacts` where `ContactType = 'Patient'`), and date of birth, per GID-254887. Date-of-test
range filtering is a separate optional parameter, added when this method's consumer (the list
view) needs it.

**Conventions followed from `IUserRepository`:** `Task<bool>` for mutations, `Task<T?>`/
`Task<List<T>>` for reads, `null`/empty-list signals not-found (no Result<T> wrapper introduced).
**One deliberate improvement over the existing pattern:** mutation methods log the caught
exception before returning `false`, rather than swallowing it silently (a known, already-flagged
smell in `UserRepository` — not repeated here, but not fixed there either, since that's out of
this story's scope).

`PagedResult<T>` is a new, minimal shape (`List<T> Items`, `int TotalCount`) — `IUserRepository`
has no paging today, so there's nothing to mirror; `PatientsView` already paginates in-memory and
will need a total count to do so against real data.

---

## 6. Mapping layer

One new static class, `PatientMapper`, holding every conversion between `Patient` (the EF entity)
and the three UI-facing shapes — replacing the scattered inline object-initializers described in
1:

```csharp
public static class PatientMapper
{
    // Entity -> lightweight list row
    public static Application.Models.Patient ToListRow(this Core.Entities.Patient entity) { ... }

    // Entity -> detail/edit form view model
    public static PatientViewModel ToViewModel(this Core.Entities.Patient entity, IQrCodeGenerator qrGenerator) { ... }

    // Edited view model -> write changes back onto the entity
    public static void ApplyTo(this PatientViewModel viewModel, Core.Entities.Patient entity) { ... }

    // Import DTO -> new entity
    public static Core.Entities.Patient ToEntity(this PatientData importData) { ... }
}
```

No AutoMapper or other mapping library is introduced — nothing like that exists anywhere in the
solution today, and adding one for a single entity would be disproportionate. Plain static
extension methods match the codebase's existing (if scattered) manual-mapping style, just
consolidated into one file.

---

## 7. WPF wiring changes

- **`PatientsView.xaml.cs`**: `LoadDummyPatients()` and `AddDummyTestData()` are removed. The
  constructor calls a new `LoadPatientsFromRepositoryAsync()` that calls
  `IPatientRepository.GetPagedAsync(...)` and maps each result through
  `PatientMapper.ToListRow()`. `PatientsListView_SelectionChanged` is updated to call
  `IPatientRepository.GetByIdAsync(...)` and `PatientMapper.ToViewModel(...)` instead of building
  a `PatientViewModel` inline.
- **Dev-only seeder**: gated behind the existing `DevModeConfig.IsAnyDevMode` flag (no new flag
  file introduced) — if true, seeds a handful of patients through the real repository on first
  run instead of the removed dummy methods, so dev/QA still sees populated data without a manual
  import.
- **`SplashViewModel`**: gains an `IPatientRepository` constructor dependency alongside the
  existing `IUserRepository`, and calls its `InitializeDatabaseAsync()` right after the Settings
  DB's, with its own status string. Failure behavior matches today's — any exception shuts the
  app down; Patient DB init failure is not treated as recoverable/non-fatal in this story.

---

## 8. Testing plan

Unit tests (`Tests/AccuSync.EF/Repositories/`, mirroring `UserRepository.test.cs`'s location):

- **Patient round-trip**: create → read back by id → update a field → soft-delete → confirm the
  patient no longer appears in `GetPagedAsync`'s default (non-`includeDeleted`) results.
- **Risk factor round-trip**: create a patient with Yes/No/Unknown values across multiple risk
  factors → read back via `GetByIdAsync` → confirm all three distinct values persist (the exact
  gap the JSON-array design couldn't cover).
- **`GetByIdAsync` aggregate load**: confirm `PatientContacts` and associated `TestRecords` are
  both populated on the returned entity, not just the bare `Patients` row.
- **Search/filter**: patient ID, name (via joined `PatientContacts`), date of birth.
- **`ITestRepository`**: deleting a test record removes it from `GetTestsForPatientAsync`;
  reassigning a test record moves it to the target patient's list and off the original's.

---

## 9. Explicitly out of scope for this story

- `ImportBatches`, `ABRResults`, `TEOAEResults`, `DPOAEResults` entities/migrations (2, decision
  3) — later Import / OAE / ABR stories.
- Whole-database AES-256 encryption, GID-256510 (2, decision 4) — tracked gap, not fixed here.
- An admin-facing "Show Deleted" toggle/filter for patients — the default list query excludes
  soft-deleted patients regardless of the answer to this; the toggle is additional UI, pending
  the client decision in 11.
- A retained/soft-deleted `TestRecords` row — deleting an individual test entry
  (`ITestRepository.DeleteTestRecordAsync`) is a **hard delete** for now, matching what's
  actually in the schema today (`TestRecords` has no `IsDeleted` column). If the client requires
  retention, that becomes a follow-on migration adding the column — not a rebuild of this story's
  work.

---

## 10. Open items requiring confirmation

**Needs the client's answer (not blocking the start of development — see 9 for the interim
default each one gets):**
1. Should a deleted patient remain visible to Admins via a "Show Deleted" toggle, or disappear
   from every view entirely?
2. Should deleting an individual test entry retain the underlying test data for some period, or
   is a hard delete (with only an audit-log entry of the event) acceptable?

**FYI to the lead, not a question (decisions already made, stated here for visibility):**
3. The risk-factor storage redesign (2, decision 5; 3) is being done now, in this story,
   including a change to `Databases/PatientDatabase.sql` — not deferred to a later story.
4. The list/detail/import type split (2, decision 6) keeps all three existing Patient-shaped
   types rather than consolidating onto `PatientViewModel` as the acceptance criteria's wording
   alone might suggest.

---

## 11. Assumptions & risks

- No `PatientDatabase.db` exists yet in any dev/UAT environment with real data — dropping
  `Patients.PatientRiskFactors` in the initial migration is assumed safe with nothing to migrate.
  **Flagging explicitly in case that assumption is wrong.**
- Patient DB initialization failure on splash is fatal, same as Settings DB today — not made
  independently recoverable in this story.
- `RiskFactorId` values written into `PatientRiskFactorValues` are not validated against
  `SettingsDatabase.RiskFactors` at the database level (impossible cross-DB); validation happens
  at the repository layer, same limitation already accepted for `SiteId`/`AssignedUserId`.

---

## 12. Traceability to acceptance criteria

| Acceptance criterion | Covered in |
|---|---|
| `PatientDbContext` in `AccuSync.EF/Contexts/`, registered via `SqliteServiceCollectionExtensions`, pointed at `PatientDatabase.db` | 4.2, 4.4 |
| EF entities/configs for Patients, PatientContacts, TestSessions, TestRecords matching the schema | 3, 4.1 (plus `PatientRiskFactorValues`, a deliberate addition — 2 decision 5) |
| Initial migration creates schema automatically; splash screen covers it | 4.5, 7 |
| `IPatientRepository`/`ITestRepository` in Core/EF, following the `IUserRepository` pattern | 5 |
| `TimestampInterceptor` applies to the new context | 4.3 |
| Mapping layer, one place | 6 |
| Unit tests: create → read → update → soft-delete → excluded from default list | 8 |
| `LoadDummyPatients()`/`AddDummyTestData()` removed; dev-only seeder gated like `DevModeConfig` | 7 |
