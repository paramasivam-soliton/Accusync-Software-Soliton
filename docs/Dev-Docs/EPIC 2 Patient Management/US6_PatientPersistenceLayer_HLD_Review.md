# Patient Persistence Layer — High-Level Design

## 1. Overview

This document defines the design of the Patient/Test data-access layer (ASWD-84): a database
context, entity model, repository layer, and mapping layer that replace the in-memory patient
data currently used in the application, enabling Patient Management features to read and write
persisted data instead of static in-memory collections.

## 2. Scope

**In scope:**
- `PatientDbContext` and its entity model: `Patient`, `PatientContact`, `TestSession`,
  `TestRecord`, `PatientRiskFactorValue`.
- Initial EF Core migration with automatic database creation on first run.
- `IPatientRepository` and `ITestRepository`, implemented against the new context.
- Timestamp auditing (`CreatedAt`/`ModifiedAt`) for all new entities.
- A single mapping layer between the persistence entities and existing UI-facing types.
- Removal of hardcoded sample patient data from the patient list view.

**Out of scope (later phases):**
- `ImportBatches`, `ABRResults`, `TEOAEResults`, `DPOAEResults` entities — covered by the Import
  and Test Result phases.
- Database-level encryption at rest (file-based encryption of the database file itself, not
  per-field/per-record encryption of individual columns).
- UI for viewing or restoring soft-deleted patients.
- Retention of deleted individual test records beyond an audit log entry.

## 3. Current State

The patient list view currently renders a fixed set of hardcoded records with no database
backing. No entity model, database context, or repository exists for patient or test data today.
Two supporting patterns already exist in the codebase and are reused by this design: a
SQLite-backed `DbContext`/repository pattern (currently used for user and application-settings
data) and a save-time timestamp interceptor.

## 4. Architecture

### 4.1 Database Topology

Patient and test data are stored in a dedicated SQLite database, `PatientDatabase.db`, separate
from the existing settings/configuration database. This preserves independent backup, retention,
and encryption handling for clinical data versus application configuration, and matches the
two-database structure already defined in the target schema.

### 4.2 Cross-Database References

Columns that logically reference the settings database (`SiteId`, `AssignedUserId`,
`MatchedDeviceId`, `MatchedProtocolId`, `RiskFactorId`) are stored as plain integer values with
no database-enforced foreign key, since SQLite does not support foreign keys across separate
database files. Referential integrity for these columns is enforced at the application/repository
layer.

## 5. Data Model

### 5.1 Entities

| Entity | Maps to table | Description |
|---|---|---|
| `Patient` | `Patients` | Demographic and clinical core record. |
| `PatientContact` | `PatientContacts` | Patient, mother, father, and caregiver contact records — one row per contact per patient. |
| `TestSession` | `TestSessions` | Groups test records recorded on the same date. |
| `TestRecord` | `TestRecords` | Individual ABR/TEOAE/DPOAE test entry. |
| `PatientRiskFactorValue` | `PatientRiskFactorValues` (new) | One row per risk factor explicitly set on a patient. |

### 5.2 Risk Factor Storage

Risk factor selections are stored as a normalized table (`PatientId`, `RiskFactorId`, `Value`)
rather than a single JSON column. This supports the required tri-state value (Yes / No / Unknown)
and allows an efficient existence check when determining whether a risk factor definition is
currently in use. `RiskFactorId` references the settings database and is not database-enforced,
consistent with 4.2.

```sql
CREATE TABLE IF NOT EXISTS PatientRiskFactorValues (
    PatientRiskFactorValueId INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientId                INTEGER NOT NULL REFERENCES Patients(PatientId),
    RiskFactorId             INTEGER NOT NULL,
    Value                    TEXT    NOT NULL,
    CreatedAt                TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt               TEXT    NOT NULL DEFAULT (datetime('now')),
    UNIQUE (PatientId, RiskFactorId)
);
```

## 6. Component Design

### 6.1 Database Context

`PatientDbContext` follows the existing context pattern used elsewhere in the solution: no
embedded connection configuration, provider and options supplied via dependency injection, and
one entity configuration class per entity.

### 6.2 Timestamp Auditing

All new entities implement a shared `IAuditableEntity` interface (`CreatedAt`, `ModifiedAt`). The
existing save-time interceptor is extended with a generic pass over this interface, applied
independently of its existing handling for other entity types.

### 6.3 Dependency Injection

The existing persistence registration extension is extended to register `PatientDbContext`
alongside the current database context, each pointed at its own database file and connection
string.

### 6.4 Migration and Initialization

A single EF Core migration creates the full schema on first run. Database initialization
(backup, migrate, integrity check) follows the same procedure already used for the existing
database, invoked from the same application startup step.

## 7. Repository Layer

Two repository interfaces are introduced:

**`IPatientRepository`** — patient records, contacts, and risk factor values:
- Paged list retrieval, with search by patient ID, name, and date of birth, and optional
  inclusion of soft-deleted records.
- Retrieval by ID, including associated contacts and tests.
- Create, update, and soft-delete.

**`ITestRepository`** — test sessions and records:
- Retrieval of tests for a given patient.
- Deletion of an individual test record.
- Reassignment of a test record to a different patient.

Both follow the repository conventions already established in the codebase: asynchronous
methods, boolean success indicators for mutations, and no additional result-wrapper abstraction.

## 8. Mapping Layer

A single mapping component translates between the persistence entity and each UI-facing
representation already in use:
- A lightweight row representation for list display.
- A detail/edit view model for the patient detail panel.
- A flat data-transfer object used by the import pipeline.

Centralizing this mapping replaces the current pattern of constructing these representations
inline at each call site.

## 9. Application Integration

- The patient list view loads data through `IPatientRepository` instead of a hardcoded in-memory
  list.
- Database initialization for the new context is added to the existing application startup
  sequence, alongside the existing database's initialization.
- A development-only data seeder, gated behind the existing development-mode configuration,
  replaces the removed hardcoded sample data for local testing.

## 10. Testing Strategy

- Repository round-trip test: create, retrieve, update, soft-delete, and verify exclusion from
  the default list query.
- Risk factor persistence test: verify all three tri-state values are stored and retrieved
  correctly.
- Aggregate retrieval test: verify a patient retrieved by ID includes associated contacts and
  test records.
- Search/filter tests: patient ID, name, and date of birth.
- Test record management tests: deletion and reassignment.

## 11. Non-Functional Considerations

- **Encryption at rest:** not implemented in this phase. The applicable requirement (GID-256510)
  is file-based encryption of the database file itself, not per-field/per-record encryption of
  individual columns. Tracked as a known gap for a future phase; no implementation approach is
  assumed here.
- **Data retention:** individual test record deletion is a hard delete in this phase; no
  retention period is implemented.

## 12. Open Decision Points

| Item | Description |
|---|---|
| Deleted patient visibility | Whether a soft-deleted patient should remain visible/recoverable through an administrative filter, or be fully hidden from all views. |
| Test record deletion retention | Whether deleted individual test records must be retained for a defined period, or whether permanent deletion with an audit log entry is sufficient. |

## 13. Traceability

| Capability | Delivered by |
|---|---|
| Patient database context and registration | 6.1, 6.3 |
| Entity model matching target schema | 5 |
| Automatic schema creation on first run | 6.4 |
| Repository layer | 7 |
| Timestamp auditing | 6.2 |
| Mapping layer | 8 |
| Repository unit tests | 10 |
| Removal of hardcoded sample data | 9 |
