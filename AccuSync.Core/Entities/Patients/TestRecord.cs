// --------------------------------------------------------------------------------
// <copyright file="TestRecord.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Core.Entities.Patients
{
    /// <summary>
    /// EF entity for the <c>TestRecords</c> table — one row per individual test (ABR/TEOAE/DPOAE).
    /// Deliberately placed in this sub-namespace rather than <c>AccuSync.Core.Entities</c> to avoid
    /// colliding with the pre-existing <see cref="AccuSync.Core.Entities.TestRecord"/> display shape
    /// used by the import parsers and legacy test-results view.
    /// </summary>
    public class TestRecord : IAuditableEntity
    {
        public int TestRecordId { get; set; }
        public int SessionId { get; set; }

        /// <summary>
        /// Present for query convenience but has no EF-level FK to <see cref="Patient"/> — the
        /// cascade-delete path runs through <see cref="SessionId"/> → TestSessions → Patients only,
        /// avoiding a second, redundant cascade path to the same row.
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>Not modeled with an EF navigation — <c>ImportBatches</c> is out of scope for this story.</summary>
        public int? ImportBatchId { get; set; }

        public string? SourceId { get; set; }
        public string? TestTypeSignature { get; set; }

        /// <summary>'ABR' | 'TEOAE' | 'DPOAE'.</summary>
        public string TestType { get; set; } = string.Empty;

        /// <summary>'Left Ear' | 'Right Ear' | 'Binaural'.</summary>
        public string? TestObject { get; set; }
        public string? ScreeningMethod { get; set; }
        public string? Application { get; set; }

        /// <summary>'Pass' | 'Refer' | 'Incomplete'.</summary>
        public string? TestResult { get; set; }
        public DateTime TestDate { get; set; }

        /// <summary>Milliseconds.</summary>
        public int? Duration { get; set; }

        public string? SourceInstrumentId { get; set; }
        public string? InstrumentSerial { get; set; }
        public string? InstrumentName { get; set; }

        public string? TransducerSerial { get; set; }
        public string? TransducerTypeName { get; set; }
        public DateTime? TransducerCalDate { get; set; }
        public DateTime? TransducerNextCalDate { get; set; }

        /// <summary>Cross-database reference — [XDB: SettingsDatabase.Devices.DeviceId]. No FK; resolved at import.</summary>
        public int? MatchedDeviceId { get; set; }

        /// <summary>Cross-database reference — [XDB: SettingsDatabase.*Protocols.*ProtocolId]. No FK; resolved at import.</summary>
        public int? MatchedProtocolId { get; set; }

        /// <summary>'ABR' | 'TEOAE' | 'DPOAE'.</summary>
        public string? MatchedProtocolType { get; set; }

        public string? SourceFacilityRefId { get; set; }
        public string? SourceLocationRefId { get; set; }
        public string? SourceUserRefId { get; set; }
        public string? SourceBinauralRefId { get; set; }

        public string? PredefinedComments { get; set; }

        /// <summary>Raw base64 blob from the import XML, preserved for full-fidelity re-deserialization.</summary>
        public string? TestDetail { get; set; }

        /// <summary>Pre-parsed display values so the UI can render results without deserializing <see cref="TestDetail"/>.</summary>
        public string? SummaryJson { get; set; }

        public DateTime? SourceCreatedAt { get; set; }
        public DateTime? SourceModifiedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public TestSession? TestSession { get; set; }
    }
}
