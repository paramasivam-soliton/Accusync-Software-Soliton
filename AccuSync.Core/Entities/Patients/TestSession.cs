// --------------------------------------------------------------------------------
// <copyright file="TestSession.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace AccuSync.Core.Entities.Patients
{
    /// <summary>
    /// EF entity for the <c>TestSessions</c> table — a logical grouping of tests that
    /// share the same date for a patient, derived during import.
    /// </summary>
    public class TestSession : IAuditableEntity
    {
        public int SessionId { get; set; }
        public int PatientId { get; set; }

        /// <summary>Not modeled with an EF navigation — <c>ImportBatches</c> is out of scope for this story.</summary>
        public int? ImportBatchId { get; set; }

        public DateTime SessionDate { get; set; }

        /// <summary>'Pass' | 'Refer' | 'Incomplete' — aggregate result across this session's test records.</summary>
        public string? OverallResult { get; set; }

        public bool IsExported { get; set; }
        public DateTime? ExportedAt { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public Patient? Patient { get; set; }
        public List<TestRecord> TestRecords { get; set; } = new();
    }
}
