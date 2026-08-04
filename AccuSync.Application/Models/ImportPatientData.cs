// --------------------------------------------------------------------------------
// <copyright file="ImportPatientData.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using AccuSync.Core.Entities;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Input wrapper — one per patient found in the import file.
    /// Populated by the parser before calling LoadPreviewData.
    /// </summary>
    public class ImportPatientData
    {
        public PatientData Patient { get; set; }
        public ImportStatus Status { get; set; } = ImportStatus.New;
        public List<TestPreviewItem> Tests { get; set; } = new();
        public DuplicateInfo DupInfo { get; set; }
    }
}
