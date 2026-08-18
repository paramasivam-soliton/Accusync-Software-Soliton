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
        /// <summary>
        /// The originating device/software: ALGO 5, AccuLink, AccuSync, or ALGO Pro.
        /// </summary>
        public string SourceSystem { get; set; }    // ALGO 5, AccuLink, AccuSync, or ALGO Pro

        /// <summary>
        /// Version of the source system that produced the export file, when available.
        /// </summary>
        public string SourceVersion { get; set; }

        /// <summary>
        /// Timestamp recorded by the source system when the file was exported.
        /// </summary>
        public string ExportTimestamp { get; set; }

        /// <summary>
        /// Base language/culture of the export file (e.g., <c>"en-US"</c>).
        /// </summary>
        public string BaseLanguage { get; set; }
    }
}
