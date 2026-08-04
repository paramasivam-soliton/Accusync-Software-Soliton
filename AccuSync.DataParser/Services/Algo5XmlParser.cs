// --------------------------------------------------------------------------------
// <copyright file="Algo5XmlParser.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Models;
using AccuSync.Core.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace AccuSync.DataParser.Services
{
    /// <summary>
    /// Parses ALGO 5 XML export files into <see cref="ImportPatientData"/>
    /// records with per-ear <see cref="TestPreviewItem"/>s.
    ///
    /// Tests are ABR-only. Each Plugin produces two items (left + right ear).
    /// Plugin and TestResult elements are linked via <c>TestResultId</c> ↔ <c>Id</c>.
    ///
    /// Gender codes: 1 = Male, 2 = Female.
    /// Result codes: 0 = NotTested, 1 = Incomplete, 2 = Refer, 3 = Pass.
    /// Risk factor values: 0 = No, 1 = Yes, 2 = Unknown.
    /// Date format: <c>M-d-yyyy h:mm:ss tt</c> (US style).
    /// </summary>
    // NOTE: This parser and AlgoProJsonParser share the same domain logic
    //       (gender codes, result codes, risk factor pairing, date formats).
    //       If a third device format appears, extract shared Algo field mappers.
    public static class Algo5XmlParser
    {
        // All observed date formats across ALGO 5 export files.
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
        /// Parses an ALGO 5 XML file and returns one <see cref="ImportPatientData"/>
        /// per patient, each containing demographics and per-ear test previews.
        /// </summary>
        public static List<ImportPatientData> Parse(string filePath)
        {
            var doc = XDocument.Load(filePath);
            var results = new List<ImportPatientData>();

            foreach (var record in doc.Descendants("PatientTestRecord"))
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

        // Patient parsing

        private static PatientData ParsePatient(XElement record)
        {
            var p = new PatientData();
            var patientEl = record.Element("Patient");
            var demo = patientEl?.Element("DemographicData");
            var risks = patientEl?.Element("RiskFactors");

            if (demo == null) return p;

            p.SourceId = Val(demo, "Id");
            p.PatientId = Val(demo, "MedicalRecordNumber");
            p.FirstName = Val(demo, "FirstName");
            p.LastName = Val(demo, "LastName");
            p.MiddleInitial = Val(demo, "MiddleInitial");
            p.DateOfBirth = FormatDate(Val(demo, "Birthdate"));
            p.Gender = ParseGender(Val(demo, "Gender"));
            p.BirthLocation = Val(demo, "BirthLocation");
            p.Height = Val(demo, "Height");
            p.Weight = Val(demo, "Weight");
            p.Nationality = Val(demo, "Nationality");
            p.Discharged = FormatDate(Val(demo, "DischargeDate"));
            p.Comments = Val(demo, "Comment");
            p.SourceCreatedAt = Val(demo, "CreationDate");
            p.SourceModifiedAt = Val(demo, "LastModified");

            // ALGO 5 stores mother/address directly on DemographicData
            // (not in a nested Mother element)
            p.MotherFirstName = Val(demo, "MothersFirstName");
            p.MotherLastName = Val(demo, "MothersLastName");
            p.MotherDOB = FormatDate(Val(demo, "MothersBirthdate"));
            p.MotherAddress1 = Val(demo, "Address1");
            p.MotherCity = Val(demo, "City");
            p.MotherState = Val(demo, "State");
            p.MotherZip = Val(demo, "PostalCode");
            p.MotherCountry = Val(demo, "Country");
            p.MotherPhone = Val(demo, "Telephone");
            p.MotherMobile = Val(demo, "MobilePhone");

            p.CaregiverFirstName = Val(demo, "CaregiverFirstName");
            p.CaregiverLastName = Val(demo, "CaregiverLastName");

            if (risks != null)
                p.RiskFactors = ParseRiskFactors(risks);

            // Medical info lives under TestResult, not DemographicData
            var firstTestResult = record.Element("Test")?.Elements("TestResult").FirstOrDefault();
            if (firstTestResult != null)
            {
                p.Physician = Val(firstTestResult, "Physician");
                p.Medication = Val(firstTestResult, "Medication");
                p.ReferralFrom = Val(firstTestResult, "ReferredBy");
            }

            return p;
        }

        // Test parsing

        /// <summary>
        /// Extracts per-ear test preview items from all Plugins under the Test node.
        /// Each Plugin produces up to two items (left + right ear).
        /// </summary>
        private static List<TestPreviewItem> ParseTests(XElement record)
        {
            var items = new List<TestPreviewItem>();
            var testEl = record.Element("Test");
            if (testEl == null) return items;

            foreach (var plugin in testEl.Elements("Plugin"))
            {
                string type = plugin.Attribute("type")?.Value ?? "ABR";
                string startTimeRaw = Val(plugin, "StartTime");
                ParseDateTimeParts(startTimeRaw, out string date, out string time);

                var leftDetails = plugin.Element("LeftEarAABRDetails");
                if (leftDetails != null)
                {
                    items.Add(new TestPreviewItem
                    {
                        Date = date,
                        Time = time,
                        Ear = "Left",
                        Type = type,
                        Result = NormalizeResult(Val(leftDetails, "Result"))
                    });
                }

                var rightDetails = plugin.Element("RightEarAABRDetails");
                if (rightDetails != null)
                {
                    items.Add(new TestPreviewItem
                    {
                        Date = date,
                        Time = time,
                        Ear = "Right",
                        Type = type,
                        Result = NormalizeResult(Val(rightDetails, "Result"))
                    });
                }
            }

            return items;
        }

        // Risk factor parsing

        /// <summary>
        /// ALGO 5 stores risk factors as alternating sibling elements paired by
        /// position index. Values: 0 = No, 1 = Yes, 2 = Unknown.
        /// </summary>
        private static Dictionary<string, string> ParseRiskFactors(XElement risksEl)
        {
            var dict = new Dictionary<string, string>();
            var names = risksEl.Elements("RiskFactorName").ToList();
            var values = risksEl.Elements("RiskFactorValue").ToList();

            for (int i = 0; i < Math.Min(names.Count, values.Count); i++)
            {
                string name = names[i].Value?.Trim();
                string val = values[i].Value?.Trim();

                if (!string.IsNullOrEmpty(name))
                {
                    dict[name] = val switch
                    {
                        "0" => "No",
                        "1" => "Yes",
                        "2" => "Unknown",
                        _ => "Unknown"
                    };
                }
            }

            return dict;
        }

        // Helpers

        /// <summary>
        /// Reads a child element's trimmed text value.
        /// Returns empty string if the element is missing.
        /// </summary>
        private static string Val(XElement parent, string name)
        {
            return parent?.Element(name)?.Value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Converts ALGO 5 numeric gender code: 1 = Male, 2 = Female.
        /// </summary>
        private static string ParseGender(string code)
        {
            return code switch
            {
                "1" => "Male",
                "2" => "Female",
                _ => string.IsNullOrWhiteSpace(code) ? "Unknown" : code
            };
        }

        // TODO: Hardcoded US date output format (MM/dd/yyyy). Same issue flagged
        //       in TestRecord, XmlParser, and AlgoProJsonParser.
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

            DateTime dt;
            if (DateTime.TryParseExact(raw, DateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dt) ||
                DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dt))
            {
                date = dt.ToString("MM/dd/yyyy");
                time = dt.ToString("h:mm tt");
            }
        }

        /// <summary>
        /// Maps ear-level result strings to the standard three values used by
        /// <see cref="TestPreviewItem"/>: Pass, Refer, or Incomplete.
        /// "Halted" is treated as Incomplete.
        /// </summary>
        private static string NormalizeResult(string result)
        {
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