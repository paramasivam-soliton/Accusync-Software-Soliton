// --------------------------------------------------------------------------------
// <copyright file="IImportService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System;
using AccuSync.Application.Models;
using AccuSync.Application.Services.Parsing;
using AccuSync.Core.Entities;

namespace AccuSync.Application.Abstractions.Parsing
{
    /// <summary>
    /// Orchestrates file parsing and import execution. Implemented by
    /// <c>AccuSync.Adapters.DataParser</c>, which routes to format-specific parsers.
    /// </summary>
    public interface IImportService
    {
        /// <summary>
        /// Parses a file and returns patients + errors.
        /// Format tags match the <c>ImportFileDialog</c> ComboBox <c>Tag</c> values.
        /// </summary>
        ParseResult ParseFile(string filePath, string format);

        /// <summary>
        /// Returns an import function that validates, deduplicates, and saves
        /// selected patients. Each patient produces an <see cref="ImportResultItem"/>
        /// categorized as Imported, Duplicate, or Error.
        /// </summary>
        Func<List<PatientData>, List<ImportOutcome>> GetImportFunction(
            Func<PatientData, bool> existsInDatabase = null,
            Func<PatientData, bool> saveToDatabase = null);

        /// <summary>
        /// Returns <c>true</c> if the format requires a facesheet review step
        /// before import (so the user can verify extracted data).
        /// </summary>
        bool IsFacesheetFormat(string format);

        /// <summary>
        /// Returns raw extracted text for the facesheet preview panel.
        /// Returns <c>null</c> for images (the UI shows the image directly instead).
        /// </summary>
        string GetFacesheetPreviewText(string filePath, string format);
    }
}
