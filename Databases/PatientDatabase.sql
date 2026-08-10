-- ============================================================
-- AccuSync — Patient Database Schema
-- Engine:    SQLite
--            (SQL Server equivalents: AUTOINCREMENT → IDENTITY,
--             DATETIME → DATETIME2, TEXT → NVARCHAR(MAX))
--
-- Database:  PatientDatabase.db
-- Purpose:   Patients, tests, imports
--
-- Cross-database foreign keys
--   SQLite does not support foreign keys across database files.
--   Columns that reference SettingsDatabase are stored as plain
--   INTEGER and annotated -- [XDB: SettingsDatabase.TableName.Column].
--   Referential integrity is enforced at the application layer.
--
-- Audit logging
--   All audit events are written by Serilog to a rolling file
--   sink, not to any database table.
--   Path:      %ProgramData%\AccuSync\Logs\audit_YYYYMMDD.log
--   Retention: 90 days (configurable via Serilog settings)
--   Format:    one JSON object per line, e.g.
--     {"ts":"...","user":"...","action":"...","entity":"...","id":0,"note":"..."}
-- ============================================================


-- ============================================================
-- SECTION 1: IMPORT BATCHES
-- One row per imported file. Provides a full audit trail of
-- every import operation and links all imported records back
-- to the file that produced them.
-- ============================================================

CREATE TABLE IF NOT EXISTS ImportBatches (
    ImportBatchId    INTEGER PRIMARY KEY AUTOINCREMENT,
    ImportedByUserId INTEGER,   -- [XDB: SettingsDatabase.Users.UserId]
    SourceFileName   TEXT,
    SourceSystem     TEXT,      -- e.g. 'AccuLink'
    SourceVersion    TEXT,      -- value of <AccuLink.XiMpLe Version="...">
    ExportTimestamp  TEXT,      -- value of <ExportTimestamp>
    BaseLanguage     TEXT,      -- value of <BaseLanguage>
    PatientsInFile   INTEGER,
    PatientsImported INTEGER,
    PatientsSkipped  INTEGER,
    ImportedAt       TEXT    NOT NULL DEFAULT (datetime('now')),
    Notes            TEXT
);


-- ============================================================
-- SECTION 2: PATIENTS
-- One row per patient. SourceId is the device-assigned GUID
-- used for deduplication across imports.
-- Risk factor selections live in PatientRiskFactorValues (see
-- Section 2A), not on this table — a normalized table is needed
-- to represent the tri-state Yes/No/Unknown value per risk
-- factor, which a JSON array of codes cannot express.
-- FreeField1–4 labels are defined in SettingsDatabase.FieldSetup.
-- ============================================================

CREATE TABLE IF NOT EXISTS Patients (
    PatientId           INTEGER PRIMARY KEY AUTOINCREMENT,
    SourceId            TEXT    UNIQUE,         -- <Patient><Id> GUID from device
    ImportBatchId       INTEGER REFERENCES ImportBatches(ImportBatchId),

    -- Cross-database references (no FK enforcement — app layer)
    SiteId              INTEGER,    -- [XDB: SettingsDatabase.Sites.SiteId]
    AssignedUserId      INTEGER,    -- [XDB: SettingsDatabase.Users.UserId]

    PatientRecordNumber TEXT,
    HospitalId          TEXT,

    -- Status flags
    NicuStatus          TEXT,       -- 'Yes' | 'No' | 'Unknown'
    Discharged          TEXT,       -- DATE stored as ISO 8601 text
    Deceased            INTEGER,    -- 1 = true, 0 = false
    Medication          TEXT,

    -- Consent
    ConsentState        TEXT,       -- 'Full' | 'Partial' | 'None'
    TrackingConsent     TEXT,
    ScreeningConsent    TEXT,       -- 'Yes' | 'No'

    -- Clinical
    GestationalAge      INTEGER,    -- weeks
    RaceReferenceId     TEXT,

    -- Referral
    ReferralFrom        TEXT,
    ReferralTo          TEXT,
    ReferralPhone       TEXT,

    -- Free text fields
    FreeText1           TEXT,
    FreeText2           TEXT,
    FreeText3           TEXT,

    -- Configurable free fields (labels defined in SettingsDatabase.FieldSetup)
    FreeField1Value     TEXT,
    FreeField2Value     TEXT,
    FreeField3Value     TEXT,
    FreeField4Value     TEXT,

    PredefinedComments  TEXT,

    -- AccuSync management flags
    IsDeleted           INTEGER NOT NULL DEFAULT 0,
    IsExported          INTEGER NOT NULL DEFAULT 0,
    ExportedAt          TEXT,

    -- Timestamps from the source device
    SourceCreatedAt     TEXT,
    SourceModifiedAt    TEXT,
    CreatedAt           TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt          TEXT    NOT NULL DEFAULT (datetime('now'))
);


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
    ModifiedAt               TEXT    NOT NULL DEFAULT (datetime('now')),
    UNIQUE (PatientId, RiskFactorId)
);


-- ============================================================
-- SECTION 3: PATIENT CONTACTS
-- One row per contact per patient.
-- ContactType: 'Patient' | 'Mother' | 'Father' | 'Caregiver'
-- ============================================================

CREATE TABLE IF NOT EXISTS PatientContacts (
    ContactId               INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientId               INTEGER NOT NULL REFERENCES Patients(PatientId),
    SourceId                TEXT,
    ContactType             TEXT    NOT NULL,

    -- Name
    Title                   TEXT,
    Forename1               TEXT,
    Forename2               TEXT,
    Surname                 TEXT,

    -- Identity
    SocialSecurityNumber    TEXT,
    IdNumber                TEXT,

    -- Demographics
    DateOfBirth             TEXT,
    CalculatedDateOfBirth   TEXT,
    Gender                  TEXT,
    Height                  REAL,
    Weight                  REAL,
    BirthLocation           TEXT,
    LanguageCode            TEXT,
    NationalityCode         TEXT,

    -- Address
    Address1                TEXT,
    Zip                     TEXT,
    City                    TEXT,
    State                   TEXT,
    Country                 TEXT,

    -- Contact
    Phone                   TEXT,
    CellPhone               TEXT,
    Fax                     TEXT,
    Email                   TEXT,

    -- Timestamps from the source device
    SourceCreatedAt         TEXT,
    SourceModifiedAt        TEXT,
    CreatedAt               TEXT NOT NULL DEFAULT (datetime('now')),
    ModifiedAt              TEXT NOT NULL DEFAULT (datetime('now'))
);


-- ============================================================
-- SECTION 4: TEST SESSIONS
-- A logical grouping of tests that share the same date for a
-- patient, derived during import. OverallResult is the
-- aggregate result across all test records in the session.
-- OverallResult: 'Pass' | 'Refer' | 'Incomplete'
-- ============================================================

CREATE TABLE IF NOT EXISTS TestSessions (
    SessionId     INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientId     INTEGER NOT NULL REFERENCES Patients(PatientId),
    ImportBatchId INTEGER          REFERENCES ImportBatches(ImportBatchId),
    SessionDate   TEXT    NOT NULL,
    OverallResult TEXT,
    IsExported    INTEGER NOT NULL DEFAULT 0,
    ExportedAt    TEXT,
    Notes         TEXT,
    CreatedAt     TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt    TEXT    NOT NULL DEFAULT (datetime('now'))
);


-- ============================================================
-- SECTION 5: TEST RECORDS
-- One row per individual test (AbrTest, TeTest, DpTest).
--
-- Transducer data is denormalised here rather than in a
-- separate DeviceTransducers table, because each test carries
-- its own calibration snapshot.
--
-- TestDetail stores the raw base64-encoded binary blob from
-- the import XML exactly as received, preserving full fidelity
-- for future blob deserialisation.
--
-- SummaryJson stores pre-parsed display values so the UI can
-- render test results without deserialising the blob on load.
--
-- MatchedDeviceId and MatchedProtocolId are resolved at import
-- time by matching the source instrument identity against
-- SettingsDatabase.Devices and *Protocols tables.
--
-- TestType:   'ABR' | 'TEOAE' | 'DPOAE'
-- TestObject: 'Left Ear' | 'Right Ear' | 'Binaural'
-- TestResult: 'Pass' | 'Refer' | 'Incomplete'
-- ============================================================

CREATE TABLE IF NOT EXISTS TestRecords (
    TestRecordId            INTEGER PRIMARY KEY AUTOINCREMENT,
    SessionId               INTEGER NOT NULL REFERENCES TestSessions(SessionId),
    PatientId               INTEGER NOT NULL REFERENCES Patients(PatientId),
    ImportBatchId           INTEGER          REFERENCES ImportBatches(ImportBatchId),

    SourceId                TEXT    UNIQUE,
    TestTypeSignature       TEXT,

    TestType                TEXT    NOT NULL,
    TestObject              TEXT,
    ScreeningMethod         TEXT,       -- e.g. 'LeftRightSimultaneous', '1', '3', '7'
    Application             TEXT,       -- stimulus description, e.g. '35 dB nHL', 'Amplitude35dBSPL'
    TestResult              TEXT,
    TestDate                TEXT    NOT NULL,
    Duration                INTEGER,            -- milliseconds

    -- Device identity (denormalised from import XML)
    SourceInstrumentId      TEXT,
    InstrumentSerial        TEXT,
    InstrumentName          TEXT,

    -- Transducer snapshot (denormalised from import XML — Device Information tab)
    TransducerSerial        TEXT,
    TransducerTypeName      TEXT,
    TransducerCalDate       TEXT,
    TransducerNextCalDate   TEXT,

    -- Cross-database: matched device and protocol (resolved at import, nullable)
    MatchedDeviceId         INTEGER,    -- [XDB: SettingsDatabase.Devices.DeviceId]
    MatchedProtocolId       INTEGER,    -- [XDB: SettingsDatabase.*Protocols.*ProtocolId]
    MatchedProtocolType     TEXT,       -- 'ABR' | 'TEOAE' | 'DPOAE'

    -- Source GUIDs from the device for facility/location/user/binaural resolution
    SourceFacilityRefId     TEXT,
    SourceLocationRefId     TEXT,
    SourceUserRefId         TEXT,
    SourceBinauralRefId     TEXT,

    PredefinedComments      TEXT,

    TestDetail              TEXT,   -- raw base64 blob (full fidelity preservation)
    SummaryJson             TEXT,   -- pre-parsed display values (fast UI rendering)

    -- Timestamps from the source device
    SourceCreatedAt         TEXT,
    SourceModifiedAt        TEXT,
    CreatedAt               TEXT NOT NULL DEFAULT (datetime('now')),
    ModifiedAt              TEXT NOT NULL DEFAULT (datetime('now'))
);


-- ============================================================
-- SECTION 6: PARSED TEST DETAILS
-- Populated by deserialising the TestDetail blob on TestRecords.
-- Each table has a 1:1 relationship with TestRecords.
-- Fields marked "from outer XML" do not require blob parsing.
-- All remaining fields are extracted from the binary blob.
-- ============================================================

-- ABR test result detail.
-- WaveformPointsJson: 64-point waveform as {"A":[...],"B":[...]}
-- ImpedancesJson:     array of electrode impedance readings
--                     e.g. [{"electrode":"White","value_kohm":3.2}]
-- Calibration fields: stored for waveform rendering reference.
CREATE TABLE IF NOT EXISTS ABRResults (
    ABRResultId             INTEGER PRIMARY KEY AUTOINCREMENT,
    TestRecordId            INTEGER NOT NULL UNIQUE REFERENCES TestRecords(TestRecordId),

    -- From outer XML (no blob deserialisation required)
    MyogenicNoiseIndication INTEGER,
    ImpedanceWhite          INTEGER,
    ImpedanceRed            INTEGER,

    -- From blob
    FirmwareVersion         TEXT,
    ProbeType               TEXT,
    ProbeCounter            INTEGER,
    TestName                TEXT,
    StimulusLevel_dB        REAL,
    FrequencySamplingRate   INTEGER,
    WaveformPointsJson      TEXT,
    GraphScale              REAL,

    -- Wave latencies (ms)
    WaveI_Latency_ms        REAL,
    WaveII_Latency_ms       REAL,
    WaveIII_Latency_ms      REAL,
    WaveIV_Latency_ms       REAL,
    WaveV_Latency_ms        REAL,
    WaveVI_Latency_ms       REAL,
    WaveVII_Latency_ms      REAL,

    -- Wave amplitudes (uV)
    WaveI_Amplitude_uV      REAL,
    WaveV_Amplitude_uV      REAL,

    EstimatedThreshold_dB   REAL,

    -- Screening progress metrics
    Progress                REAL,
    FrameCount              INTEGER,
    Energy                  REAL,
    ImpedancesJson          TEXT,

    -- Calibration reference data
    CalibrationFrames       INTEGER,
    CalibrationGraphScale   REAL,
    CalibrationValuesJson   TEXT,
    CalibrationWaveformJson TEXT,

    CreatedAt               TEXT NOT NULL DEFAULT (datetime('now'))
);

-- TEOAE test result detail.
-- WaveformPointsJson: 128 or 192 points as {"A":[...],"B":[...]}
--                     (exact count to be confirmed from blob decode)
-- BandResultsJson:    per-band results array
--                     e.g. [{"band_hz":1000,"snr_dB":6.2,"reproducibility_pct":82,"pass":true}]
-- Individual SNR and Repro columns duplicate BandResultsJson
-- data for efficient single-column queries.
CREATE TABLE IF NOT EXISTS TEOAEResults (
    TEOAEResultId           INTEGER PRIMARY KEY AUTOINCREMENT,
    TestRecordId            INTEGER NOT NULL UNIQUE REFERENCES TestRecords(TestRecordId),

    -- From blob
    FirmwareVersion         TEXT,
    ProbeType               TEXT,
    ProbeCounter            INTEGER,
    TestName                TEXT,
    FrequencySamplingRate   INTEGER,

    -- Screening metrics
    Progress                REAL,
    FrameCount              INTEGER,
    ArtefactCount           INTEGER,
    Stability               REAL,
    ProbeFit                REAL,
    Energy                  REAL,
    PeakIndex               INTEGER,
    NoiseLevel_dBSPL        REAL,

    WaveformPointsJson      TEXT,
    GraphScale              REAL,
    BandResultsJson         TEXT,

    -- Per-band SNR (duplicated from BandResultsJson for fast queries)
    SNR_1000Hz              REAL,
    SNR_1500Hz              REAL,
    SNR_2000Hz              REAL,
    SNR_3000Hz              REAL,
    SNR_4000Hz              REAL,

    -- Per-band reproducibility % (duplicated from BandResultsJson for fast queries)
    Repro_1000Hz            REAL,
    Repro_1500Hz            REAL,
    Repro_2000Hz            REAL,
    Repro_3000Hz            REAL,
    Repro_4000Hz            REAL,

    -- Calibration reference data
    CalibrationFrames       INTEGER,
    CalibrationGraphScale   REAL,
    CalibrationValuesJson   TEXT,
    CalibrationWaveformJson TEXT,

    CreatedAt               TEXT NOT NULL DEFAULT (datetime('now'))
);

-- DPOAE test result detail.
-- FrequencyResultsJson: full per-frequency result array
--   e.g. [{"f1_hz":...,"f2_hz":...,"dp_hz":...,"dp_level_db":...,
--           "noise_floor_db":...,"snr_db":...,"pass":true,"frames":...}]
-- Per-frequency DP columns duplicate FrequencyResultsJson data
-- for efficient single-column queries. Adjust frequency columns
-- to match the actual protocol after first blob decode.
-- ReListJson / ImListJson: real and imaginary components used
-- for rendering the DP-gram chart.
CREATE TABLE IF NOT EXISTS DPOAEResults (
    DPOAEResultId           INTEGER PRIMARY KEY AUTOINCREMENT,
    TestRecordId            INTEGER NOT NULL UNIQUE REFERENCES TestRecords(TestRecordId),

    -- From blob
    FirmwareVersion         TEXT,
    ProbeType               TEXT,
    ProbeCounter            INTEGER,
    TestName                TEXT,
    FrequencySamplingRate   INTEGER,
    NumberOfTests           INTEGER,

    -- Stimulus levels
    L1                      REAL,   -- typically 65 dB SPL
    L2                      REAL,   -- typically 55 dB SPL

    -- Full frequency result data
    FrequencyResultsJson    TEXT,
    Frequency1ListJson      TEXT,
    Frequency2ListJson      TEXT,

    -- Per-frequency columns (adjust after first blob decode)
    DP_2000Hz_Level         REAL,
    DP_2000Hz_Noise         REAL,
    DP_2000Hz_SNR           REAL,
    DP_3000Hz_Level         REAL,
    DP_3000Hz_Noise         REAL,
    DP_3000Hz_SNR           REAL,
    DP_4000Hz_Level         REAL,
    DP_4000Hz_Noise         REAL,
    DP_4000Hz_SNR           REAL,
    DP_5000Hz_Level         REAL,
    DP_5000Hz_Noise         REAL,
    DP_5000Hz_SNR           REAL,
    DP_6000Hz_Level         REAL,
    DP_6000Hz_Noise         REAL,
    DP_6000Hz_SNR           REAL,
    DP_8000Hz_Level         REAL,
    DP_8000Hz_Noise         REAL,
    DP_8000Hz_SNR           REAL,

    -- Screening metrics
    ProbeFit                REAL,
    SingleResultListJson    TEXT,
    FrameCounterListJson    TEXT,
    EnergyListJson          TEXT,

    -- Raw complex components for DP-gram rendering
    ReListJson              TEXT,
    ImListJson              TEXT,

    -- Calibration reference data
    CalibrationFrames       INTEGER,
    CalibrationGraphScale   REAL,
    CalibrationValuesJson   TEXT,
    CalibrationWaveformJson TEXT,

    CreatedAt               TEXT NOT NULL DEFAULT (datetime('now'))
);


-- ============================================================
-- INDEXES
-- ============================================================

CREATE INDEX IF NOT EXISTS IX_Patients_SourceId      ON Patients(SourceId);
CREATE INDEX IF NOT EXISTS IX_Patients_SiteId        ON Patients(SiteId);
CREATE INDEX IF NOT EXISTS IX_Patients_AssignedUser  ON Patients(AssignedUserId);
CREATE INDEX IF NOT EXISTS IX_Patients_IsExported    ON Patients(IsExported);
CREATE INDEX IF NOT EXISTS IX_Patients_IsDeleted     ON Patients(IsDeleted);

CREATE INDEX IF NOT EXISTS IX_RiskFactorValues_PatientId    ON PatientRiskFactorValues(PatientId);
CREATE INDEX IF NOT EXISTS IX_RiskFactorValues_RiskFactorId ON PatientRiskFactorValues(RiskFactorId);

CREATE INDEX IF NOT EXISTS IX_Contacts_PatientId     ON PatientContacts(PatientId);
CREATE INDEX IF NOT EXISTS IX_Contacts_Type          ON PatientContacts(ContactType);

CREATE INDEX IF NOT EXISTS IX_Sessions_PatientId     ON TestSessions(PatientId);
CREATE INDEX IF NOT EXISTS IX_Sessions_Date          ON TestSessions(SessionDate);

CREATE INDEX IF NOT EXISTS IX_TestRec_SessionId      ON TestRecords(SessionId);
CREATE INDEX IF NOT EXISTS IX_TestRec_PatientId      ON TestRecords(PatientId);
CREATE INDEX IF NOT EXISTS IX_TestRec_SourceId       ON TestRecords(SourceId);
CREATE INDEX IF NOT EXISTS IX_TestRec_TestType       ON TestRecords(TestType);
CREATE INDEX IF NOT EXISTS IX_TestRec_TestDate       ON TestRecords(TestDate);
CREATE INDEX IF NOT EXISTS IX_TestRec_ImportBatch    ON TestRecords(ImportBatchId);
