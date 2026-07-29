// --------------------------------------------------------------------------------
// <copyright file="ImportService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Models;
using AccuSync.Views.PatientsTests;
using System;
using System.Collections.Generic;
using System.IO;


namespace AccuSync.Services
{
    /// <summary>
    /// Result of parsing a file — patients with optional test previews and any
    /// parse-level errors. Errors are returned rather than thrown so the caller
    /// can display them in the import UI without catching exceptions.
    /// </summary>
    public class ParseResult
    {
        public List<ImportPatientData> Patients { get; set; } = new();
        public List<string> ParseErrors { get; set; } = new();

        /// <summary>
        /// File-level metadata extracted from the import file header.
        /// Used to populate the <c>ImportBatches</c> table when persisting.
        /// </summary>
        public ImportFileMetadata Metadata { get; set; }
    }

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

    /// <summary>
    /// Orchestrates file parsing and import execution. Routes to format-specific
    /// parsers based on the format tag from <c>ImportFileDialog</c>.
    /// </summary>
    public class ImportService
    {
        // File parsing

        /// <summary>
        /// Parses a file and returns patients + errors.
        /// Format tags match the <c>ImportFileDialog</c> ComboBox <c>Tag</c> values.
        /// </summary>
        public ParseResult ParseFile(string filePath, string format)
        {
            if (string.IsNullOrEmpty(filePath))
                return new ParseResult { ParseErrors = { "No file path provided." } };

            if (!File.Exists(filePath))
                return new ParseResult { ParseErrors = { $"File not found: {filePath}" } };

            try
            {
                return format switch
                {
                    // Device export formats
                    "algo5-xml" => ParseAlgo5Xml(filePath),
                    "acculink-xml" => ParseAccuLinkXml(filePath),
                    "accusync-xml" => ParseAccuSyncXml(filePath),
                    "accusync-json" => ParseAccuSyncJson(filePath),
                    "algopro-json" => ParseAlgoProJson(filePath),

                    // Facesheet formats (single patient, no test data)
                    "facesheet-image" => ParseFacesheetImage(filePath),
                    "facesheet-pdf" => ParseFacesheetPdf(filePath),
                    "facesheet-docx" => ParseFacesheetDocx(filePath),

                    _ => new ParseResult { ParseErrors = { $"Unsupported format: {format}" } }
                };
            }
            catch (Exception ex)
            {
                return new ParseResult { ParseErrors = { $"Error reading file: {ex.Message}" } };
            }
        }

        // Device format parsers

        private ParseResult ParseAlgo5Xml(string filePath)
        {
            var result = new ParseResult();

            try
            {
                result.Patients = Algo5XmlParser.Parse(filePath);
                result.Metadata = new ImportFileMetadata
                {
                    SourceSystem = "ALGO 5",
                    BaseLanguage = GetAlgo5Culture(filePath)
                };
            }
            catch (Exception ex)
            {
                result.ParseErrors.Add($"ALGO 5 XML parse error: {ex.Message}");
            }

            return result;
        }

        private ParseResult ParseAccuLinkXml(string filePath)
        {
            var result = new ParseResult();

            try
            {
                result.Patients = AccuLinkParser.Parse(filePath);
                result.Metadata = GetAccuLinkMetadata(filePath);
            }
            catch (Exception ex)
            {
                result.ParseErrors.Add($"AccuLink XML parse error: {ex.Message}");
            }

            return result;
        }

        // TODO: Implement when AccuSync XML export format is designed.
        private ParseResult ParseAccuSyncXml(string filePath)
        {
            var result = new ParseResult();
            result.ParseErrors.Add("AccuSync XML import is not yet implemented.");
            return result;
        }

        // TODO: Implement when AccuSync JSON export format is designed.
        private ParseResult ParseAccuSyncJson(string filePath)
        {
            var result = new ParseResult();
            result.ParseErrors.Add("AccuSync JSON import is not yet implemented.");
            return result;
        }

        private ParseResult ParseAlgoProJson(string filePath)
        {
            var result = new ParseResult();

            try
            {
                result.Patients = AlgoProJsonParser.Parse(filePath);
                result.Metadata = new ImportFileMetadata
                {
                    SourceSystem = "ALGO Pro",
                    BaseLanguage = AlgoProJsonParser.GetCulture(filePath)
                };
            }
            catch (Exception ex)
            {
                result.ParseErrors.Add($"ALGO Pro JSON parse error: {ex.Message}");
            }

            return result;
        }

        // Facesheet parsers (single patient, no test data)

        private ParseResult ParseFacesheetImage(string filePath)
        {
            var result = new ParseResult();

            try
            {
                var patient = OcrParser.ParseFacesheetOCR(filePath);
                result.Patients.Add(WrapAsImportPatient(patient));
            }
            catch (Exception ex)
            {
                result.ParseErrors.Add($"OCR parsing failed: {ex.Message}");
            }

            return result;
        }

        private ParseResult ParseFacesheetPdf(string filePath)
        {
            var result = new ParseResult();

            try
            {
                var patient = PdfParser.ParsePdfFacesheet(filePath);
                result.Patients.Add(WrapAsImportPatient(patient));
            }
            catch (Exception ex)
            {
                result.ParseErrors.Add($"PDF parsing failed: {ex.Message}");
            }

            return result;
        }

        private ParseResult ParseFacesheetDocx(string filePath)
        {
            var result = new ParseResult();

            try
            {
                var patient = DocxParser.ParseDocxFacesheet(filePath);
                result.Patients.Add(WrapAsImportPatient(patient));
            }
            catch (Exception ex)
            {
                result.ParseErrors.Add($"DOCX parsing failed: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Wraps a <see cref="PatientData"/> from facesheet parsers into an
        /// <see cref="ImportPatientData"/> with no tests and "new" status.
        /// </summary>
        private static ImportPatientData WrapAsImportPatient(PatientData patient)
        {
            return new ImportPatientData
            {
                Patient = patient,
                Status = ImportStatus.New,
                Tests = new(),
                DupInfo = null
            };
        }

        // Facesheet helpers

        /// <summary>
        /// Returns <c>true</c> if the format requires a facesheet review step
        /// before import (so the user can verify extracted data).
        /// </summary>
        public static bool IsFacesheetFormat(string format)
        {
            return format is "facesheet-image" or "facesheet-pdf" or "facesheet-docx";
        }

        /// <summary>
        /// Returns raw extracted text for the facesheet preview panel.
        /// Returns <c>null</c> for images (the UI shows the image directly instead).
        /// </summary>
        public static string GetFacesheetPreviewText(string filePath, string format)
        {
            return format switch
            {
                "facesheet-pdf" => PdfParser.GetExtractedText(filePath),
                "facesheet-docx" => DocxParser.GetExtractedText(filePath),
                "facesheet-image" => null,
                _ => null
            };
        }

        // Metadata extraction

        private static string GetAlgo5Culture(string filePath)
        {
            try
            {
                var doc = System.Xml.Linq.XDocument.Load(filePath);
                return doc.Root?.Element("Culture")?.Value?.Trim() ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// Reads metadata from the <c>AccuLink.XiMpLe</c> root element attributes.
        /// </summary>
        private static ImportFileMetadata GetAccuLinkMetadata(string filePath)
        {
            try
            {
                var doc = System.Xml.Linq.XDocument.Load(filePath);
                var root = doc.Root;
                return new ImportFileMetadata
                {
                    SourceSystem = "AccuLink",
                    SourceVersion = root?.Attribute("Version")?.Value ?? string.Empty,
                    ExportTimestamp = root?.Attribute("ExportTimestamp")?.Value ?? string.Empty,
                    BaseLanguage = root?.Attribute("BaseLanguage")?.Value ?? string.Empty
                };
            }
            catch
            {
                return new ImportFileMetadata { SourceSystem = "AccuLink" };
            }
        }

        // Import execution

        /// <summary>
        /// Returns an import function that validates, deduplicates, and saves
        /// selected patients. Each patient produces an <see cref="ImportResultItem"/>
        /// categorized as Imported, Duplicate, or Error.
        /// </summary>
        // NOTE: This returns a Func rather than executing directly because
        //       PatientsView.WrapImportFunction() adapts it to the newer
        //       ImportRequest-aware delegate signature.
        public Func<List<PatientData>, List<ImportResultItem>> GetImportFunction(
            Func<PatientData, bool> existsInDatabase = null,
            Func<PatientData, bool> saveToDatabase = null)
        {
            return (selectedPatients) =>
            {
                var results = new List<ImportResultItem>();

                foreach (var patient in selectedPatients)
                {
                    var validationError = ValidatePatient(patient);
                    if (validationError != null)
                    {
                        results.Add(new ImportResultItem
                        {
                            Name = FormatName(patient),
                            PatientId = patient.PatientId ?? "",
                            Detail = $"Error:{validationError}",
                            Patient = patient
                        });
                        continue;
                    }

                    if (existsInDatabase != null && existsInDatabase(patient))
                    {
                        results.Add(new ImportResultItem
                        {
                            Name = FormatName(patient),
                            PatientId = patient.PatientId ?? "",
                            Detail = "Duplicate: Patient already exists in database",
                            Patient = patient
                        });
                        continue;
                    }

                    bool saved = saveToDatabase != null ? saveToDatabase(patient) : true;

                    results.Add(new ImportResultItem
                    {
                        Name = FormatName(patient),
                        PatientId = patient.PatientId ?? "",
                        Detail = saved ? "Imported" : "Error:Failed to save to database",
                        Patient = patient
                    });
                }

                return results;
            };
        }

        // Validation

        /// <summary>
        /// Validates required fields. Returns <c>null</c> if valid, or an error description.
        /// </summary>
        private string ValidatePatient(PatientData patient)
        {
            if (string.IsNullOrWhiteSpace(patient.FirstName) && string.IsNullOrWhiteSpace(patient.LastName))
                return "Name: Missing first and last name";

            if (string.IsNullOrWhiteSpace(patient.DateOfBirth))
                return "DOB: Missing date of birth";

            if (!string.IsNullOrWhiteSpace(patient.DateOfBirth) && !IsValidDate(patient.DateOfBirth))
                return $"DOB: Invalid format \"{patient.DateOfBirth}\"";

            return null;
        }

        private bool IsValidDate(string dateString)
        {
            return DateTime.TryParse(dateString, out _);
        }

        private static string FormatName(PatientData p)
        {
            var last = p.LastName?.Trim() ?? "";
            var first = p.FirstName?.Trim() ?? "";
            if (!string.IsNullOrEmpty(last) && !string.IsNullOrEmpty(first))
                return $"{last}, {first}";
            return last + first;
        }
    }
}