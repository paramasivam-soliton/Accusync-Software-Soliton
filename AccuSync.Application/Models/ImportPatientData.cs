// --------------------------------------------------------------------------------
// <copyright file="ImportPatientData.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Input wrapper — one per patient found in the import file.
    /// Populated by the parser before calling LoadPreviewData.
    /// </summary>
    public class ImportPatientData
    {
        /// <summary>The patient information parsed from the import file.</summary>
        public PatientData Patient { get; set; }
        /// <summary>Whether this patient is new or a duplicate of an existing record.</summary>
        public ImportStatus Status { get; set; } = ImportStatus.New;
        /// <summary>The tests found for this patient in the import file.</summary>
        public List<TestPreviewItem> Tests { get; set; } = new();
        /// <summary>Details of how this patient differs from the existing record, if a duplicate.</summary>
        public DuplicateInfo DupInfo { get; set; }
    }
}
