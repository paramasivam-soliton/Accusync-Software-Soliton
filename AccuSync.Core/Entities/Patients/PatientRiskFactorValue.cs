// --------------------------------------------------------------------------------
// <copyright file="PatientRiskFactorValue.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Core.Entities.Patients
{
    /// <summary>
    /// Persistence entity for the PatientRiskFactorValues table. One row per (Patient,
    /// RiskFactor) pair the user has explicitly set, with a tri-state Value ("Yes" | "No" |
    /// "Unknown"). Replaces the earlier JSON-array-of-codes design, which could only represent
    /// "Yes" and not the required Yes/No/Unknown distinction. RiskFactorId references
    /// SettingsDatabase.RiskFactors and is not a database-enforced foreign key; validate at the
    /// repository layer.
    /// </summary>
    public class PatientRiskFactorValue : IAuditableEntity
    {
        public int PatientRiskFactorValueId { get; set; }
        public int PatientId { get; set; }
        public int RiskFactorId { get; set; }
        public string Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
