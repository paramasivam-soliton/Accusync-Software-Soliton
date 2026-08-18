// --------------------------------------------------------------------------------
// <copyright file="AlgoProJsonParser.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Models;
using AccuSync.Core.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace AccuSync.Adapters.DataParser.Services
{
    /// <summary>
    /// Parses ALGO Pro JSON export files into <see cref="ImportPatientData"/>
    /// records with per-ear <see cref="TestPreview"/>s.
    ///
    /// Structurally similar to ALGO 5 XML but in JSON, with key differences:
    ///   - Plugin and TestResult are arrays (not repeated XML elements).
    ///   - Gender is text ("Male"/"Female"), not a numeric code.
    ///   - TestedEar is numeric (1 = Left, 2 = Right) in ear details.
    ///   - LE_Result/RE_Result can be numeric or string.
    ///   - RiskFactors uses duplicate JSON keys — System.Text.Json keeps only
    ///     the last value, so risk factor parsing is best-effort.
    ///
    /// Date format: <c>M-d-yyyy h:mm:ss tt</c> (US style).
    /// Result codes: 0 = NotTested, 1 = Incomplete, 2 = Refer, 3 = Pass, 4 = NotScreened.
    /// Risk factor values: 0 = No, 1 = Yes, 2 = Unknown.
    /// </summary>
    public static class AlgoProJsonParser
    {
        // All observed date formats across ALGO Pro export files.
        private static readonly string[] DateFormats =
        {
            "M-d-yyyy h:mm:ss tt",
            "M-d-yyyy",
            "MM-dd-yyyy h:mm:ss tt",
            "M/d/yyyy h:mm:ss tt",
            "MM/dd/yyyy h:mm:ss tt",
            "M-d-yyyy H:mm:ss",
            "MM-dd-yyyy H:mm:ss"
        };

        // Public API

        /// <summary>
        /// Parses an ALGO Pro JSON file and returns one <see cref="ImportPatientData"/>
        /// per patient, each containing demographics and per-ear test previews.
        /// </summary>
        public static List<ImportPatientData> Parse(string filePath)
        {
            string json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var results = new List<ImportPatientData>();

            if (!root.TryGetProperty("PatientTestRecord", out var records))
                return results;

            foreach (var record in records.EnumerateArray())
            {
                var patient = ParsePatient(record);
                var tests = ParseTests(record);

                results.Add(new ImportPatientData
                {
                    Patient = patient,
                    Status = ImportStatus.New,
                    Tests = tests
                });
            }

            return results;
        }

        /// <summary>
        /// Reads the Culture value from the file root (e.g., <c>"en-US"</c>).
        /// </summary>
        public static string GetCulture(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                using var doc = JsonDocument.Parse(json);
                return Str(doc.RootElement, "Culture");
            }
            catch { return string.Empty; }
        }

        // Patient parsing

        private static PatientData ParsePatient(JsonElement record)
        {
            var p = new PatientData();

            if (!record.TryGetProperty("Patient", out var patientEl))
                return p;

            if (patientEl.TryGetProperty("DemographicData", out var demo))
            {
                p.SourceId = Str(demo, "Id");
                p.PatientId = Str(demo, "MedicalRecordNumber");
                p.FirstName = Str(demo, "FirstName");
                p.LastName = Str(demo, "LastName");
                p.MiddleInitial = Str(demo, "MiddleInitial");
                p.DateOfBirth = FormatDate(Str(demo, "Birthdate"));
                p.Gender = ParseGender(Str(demo, "Gender"));
                p.BirthLocation = Str(demo, "BirthLocation");
                p.Height = Str(demo, "Height");
                p.Weight = Str(demo, "Weight");
                p.Nationality = Str(demo, "Nationality");
                p.Discharged = FormatDate(Str(demo, "DischargeDate"));
                p.Comments = Str(demo, "Comment");
                p.SourceCreatedAt = Str(demo, "CreationDate");
                p.SourceModifiedAt = Str(demo, "LastModified");

                p.MotherFirstName = Str(demo, "MothersFirstName");
                p.MotherLastName = Str(demo, "MothersLastName");
                p.MotherDOB = FormatDate(Str(demo, "MothersBirthdate"));
                p.MotherEmail = Str(demo, "MothersEmail");
                p.MotherAddress1 = Str(demo, "Address1");
                p.MotherCity = Str(demo, "City");
                p.MotherState = Str(demo, "State");
                p.MotherZip = Str(demo, "PostalCode");
                p.MotherCountry = Str(demo, "Country");
                p.MotherPhone = Str(demo, "Telephone");
                p.MotherMobile = Str(demo, "MobilePhone");

                p.CaregiverFirstName = Str(demo, "CaregiverFirstName");
                p.CaregiverLastName = Str(demo, "CaregiverLastName");

                p.Physician = Str(demo, "CurrentPediatrician");
            }

            // Duplicate JSON keys mean System.Text.Json only keeps the last
            // RiskFactorName/Value pair. See class summary for details.
            if (patientEl.TryGetProperty("RiskFactors", out var risks))
                p.RiskFactors = ParseRiskFactors(risks);

            // Pull Physician/Medication/ReferredBy from the first TestResult
            // if DemographicData didn't have them.
            if (record.TryGetProperty("Test", out var testEl) &&
                testEl.TryGetProperty("TestResult", out var testResults))
            {
                foreach (var tr in testResults.EnumerateArray())
                {
                    string physician = Str(tr, "Physician");
                    if (!string.IsNullOrEmpty(physician)) p.Physician = physician;

                    string medication = Str(tr, "Medication");
                    if (!string.IsNullOrEmpty(medication)) p.Medication = medication;

                    string referredBy = Str(tr, "ReferredBy");
                    if (!string.IsNullOrEmpty(referredBy)) p.ReferralFrom = referredBy;

                    break;
                }
            }

            return p;
        }

        // Test parsing

        /// <summary>
        /// Extracts per-ear test preview items from all Plugins.
        /// Each Plugin produces up to two items (left + right ear).
        /// </summary>
        private static List<TestPreview> ParseTests(JsonElement record)
        {
            var items = new List<TestPreview>();

            if (!record.TryGetProperty("Test", out var testEl))
                return items;

            if (!testEl.TryGetProperty("Plugin", out var plugins))
                return items;

            foreach (var plugin in plugins.EnumerateArray())
            {
                string startTimeRaw = Str(plugin, "StartTime");
                ParseDateTimeParts(startTimeRaw, out string date, out string time);

                // TODO: Hardcoded to ABR. ALGO Pro may support DPOAE/TEOAE in
                //       future firmware — detect from Plugin properties when
                //       sample files become available.
                string type = "ABR";

                if (plugin.TryGetProperty("LeftEarAABRDetails", out var leftDetails))
                {
                    items.Add(new TestPreview
                    {
                        Date = date,
                        Time = time,
                        Ear = "Left",
                        Type = type,
                        Result = NormalizeEarResult(leftDetails)
                    });
                }

                if (plugin.TryGetProperty("RightEarAABRDetails", out var rightDetails))
                {
                    items.Add(new TestPreview
                    {
                        Date = date,
                        Time = time,
                        Ear = "Right",
                        Type = type,
                        Result = NormalizeEarResult(rightDetails)
                    });
                }
            }

            return items;
        }

        // Risk factor parsing

        /// <summary>
        /// Best-effort parsing. Iterates properties in order and pairs each
        /// RiskFactorName with the RiskFactorValue that follows it.
        /// Due to duplicate JSON keys this only captures whatever
        /// System.Text.Json retained — typically the last pair.
        /// </summary>
        private static Dictionary<string, string> ParseRiskFactors(JsonElement risks)
        {
            var dict = new Dictionary<string, string>();

            string lastName = null;
            foreach (var prop in risks.EnumerateObject())
            {
                if (prop.Name == "RiskFactorName")
                {
                    lastName = prop.Value.GetString()?.Trim();
                }
                else if (prop.Name == "RiskFactorValue" && lastName != null)
                {
                    string val = prop.Value.ValueKind == JsonValueKind.Number
                        ? prop.Value.GetInt32().ToString()
                        : prop.Value.GetString() ?? "";

                    dict[lastName] = val switch
                    {
                        "0" => "No",
                        "1" => "Yes",
                        "2" => "Unknown",
                        _ => "Unknown"
                    };
                    lastName = null;
                }
            }

            return dict;
        }

        // Helpers

        /// <summary>
        /// Safe string reader that handles string, number, and null JSON values.
        /// </summary>
        private static string Str(JsonElement el, string name)
        {
            if (!el.TryGetProperty(name, out var val))
                return string.Empty;

            return val.ValueKind switch
            {
                JsonValueKind.String => val.GetString()?.Trim() ?? string.Empty,
                JsonValueKind.Number => val.GetRawText(),
                JsonValueKind.Null => string.Empty,
                _ => val.GetRawText()
            };
        }

        /// <summary>
        /// ALGO Pro uses text gender values ("Male"/"Female") unlike ALGO 5's
        /// numeric codes. Also handles numeric codes as a fallback.
        /// </summary>
        private static string ParseGender(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Unknown";

            return value.ToLower() switch
            {
                "male" or "m" => "Male",
                "female" or "f" => "Female",
                "1" => "Male",
                "2" => "Female",
                _ => string.IsNullOrWhiteSpace(value) ? "Unknown" : value
            };
        }

        // TODO: Hardcoded US date output format (MM/dd/yyyy). Same issue flagged
        //       in TestRecord and XmlParser.
        private static string FormatDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            if (DateTime.TryParseExact(raw, DateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dt))
                return dt.ToString("MM/dd/yyyy");

            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dt))
                return dt.ToString("MM/dd/yyyy");

            return raw;
        }

        private static void ParseDateTimeParts(string raw, out string date, out string time)
        {
            date = string.Empty;
            time = string.Empty;
            if (string.IsNullOrWhiteSpace(raw)) return;

            if (DateTime.TryParseExact(raw, DateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dt) ||
                DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dt))
            {
                date = dt.ToString("MM/dd/yyyy");
                time = dt.ToString("h:mm tt");
            }
        }

        /// <summary>
        /// Normalizes the ear-level result string. Maps "Halted" to "Incomplete"
        /// since the UI only understands Pass/Refer/Incomplete.
        /// </summary>
        private static string NormalizeEarResult(JsonElement earDetails)
        {
            string result = Str(earDetails, "Result");
            if (string.IsNullOrWhiteSpace(result)) return "Incomplete";

            return result switch
            {
                "Pass" => "Pass",
                "Refer" => "Refer",
                "Incomplete" => "Incomplete",
                "Halted" => "Incomplete",
                _ => "Incomplete"
            };
        }
    }
}
