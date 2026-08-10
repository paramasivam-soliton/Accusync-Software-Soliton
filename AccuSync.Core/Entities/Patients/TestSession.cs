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
    /// Persistence entity for the TestSessions table — a logical grouping of test records
    /// that share the same test date for a patient.
    /// </summary>
    public class TestSession : IAuditableEntity
    {
        public int SessionId { get; set; }
        public int PatientId { get; set; }
        public int? ImportBatchId { get; set; }
        public DateTime SessionDate { get; set; }
        public string? OverallResult { get; set; }
        public bool IsExported { get; set; }
        public DateTime? ExportedAt { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public List<TestRecord> TestRecords { get; set; } = new List<TestRecord>();
    }
}
