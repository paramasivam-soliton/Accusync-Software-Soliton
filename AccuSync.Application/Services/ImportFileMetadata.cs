// --------------------------------------------------------------------------------
// <copyright file="ImportFileMetadata.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Services
{
    /// <summary>
    /// Header-level metadata from the import file, stored in the
    /// <c>ImportBatches</c> database row for audit/traceability.
    /// </summary>
    public class ImportFileMetadata
    {
        public string SourceSystem { get; set; }    // ALGO 5, AccuLink, AccuSync, or ALGO Pro
        public string SourceVersion { get; set; }
        public string ExportTimestamp { get; set; }
        public string BaseLanguage { get; set; }
    }
}
