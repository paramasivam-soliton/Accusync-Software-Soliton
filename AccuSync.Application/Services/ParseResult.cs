// --------------------------------------------------------------------------------
// <copyright file="ParseResult.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using AccuSync.Application.Models;

namespace AccuSync.Application.Services
{
    /// <summary>
    /// Result of parsing a file — patients with optional test previews and any
    /// parse-level errors. Errors are returned rather than thrown so the caller
    /// can display them in the import UI without catching exceptions.
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// Patients successfully parsed from the file, each with its test previews.
        /// </summary>
        public List<ImportPatientData> Patients { get; set; } = new();

        /// <summary>
        /// Human-readable errors encountered while parsing, for display in the import UI.
        /// </summary>
        public List<string> ParseErrors { get; set; } = new();

        /// <summary>
        /// File-level metadata extracted from the import file header.
        /// Used to populate the <c>ImportBatches</c> table when persisting.
        /// </summary>
        public ImportFileMetadata Metadata { get; set; }
    }
}
