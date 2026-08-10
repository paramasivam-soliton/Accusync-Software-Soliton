// --------------------------------------------------------------------------------
// <copyright file="TestRecord.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Core.Entities.Patients
{
    /// <summary>
    /// Persistence entity for the TestRecords table — one row per individual ABR/TEOAE/DPOAE
    /// test. Named the same as <see cref="AccuSync.Core.Entities.TestRecord"/> but kept in a
    /// separate namespace deliberately: that type is an unrelated, pre-existing display/import
    /// shape used throughout the parsing pipeline and views, not a persistence entity — the two
    /// are not interchangeable. MatchedDeviceId/MatchedProtocolId reference SettingsDatabase and
    /// are not database-enforced foreign keys; validate at the repository layer.
    /// </summary>
    public class TestRecord : IAuditableEntity
    {
        public int TestRecordId { get; set; }
        public int SessionId { get; set; }
        public int PatientId { get; set; }
        public int? ImportBatchId { get; set; }

        public string? SourceId { get; set; }
        public string? TestTypeSignature { get; set; }

        public string TestType { get; set; } = string.Empty;
        public string? TestObject { get; set; }
        public string? ScreeningMethod { get; set; }
        public string? Application { get; set; }
        public string? TestResult { get; set; }
        public DateTime TestDate { get; set; }
        public int? Duration { get; set; }

        public string? SourceInstrumentId { get; set; }
        public string? InstrumentSerial { get; set; }
        public string? InstrumentName { get; set; }

        public string? TransducerSerial { get; set; }
        public string? TransducerTypeName { get; set; }
        public DateTime? TransducerCalDate { get; set; }
        public DateTime? TransducerNextCalDate { get; set; }

        public int? MatchedDeviceId { get; set; }
        public int? MatchedProtocolId { get; set; }
        public string? MatchedProtocolType { get; set; }

        public string? SourceFacilityRefId { get; set; }
        public string? SourceLocationRefId { get; set; }
        public string? SourceUserRefId { get; set; }
        public string? SourceBinauralRefId { get; set; }

        public string? PredefinedComments { get; set; }

        public string? TestDetail { get; set; }
        public string? SummaryJson { get; set; }

        public string? SourceCreatedAt { get; set; }
        public string? SourceModifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
