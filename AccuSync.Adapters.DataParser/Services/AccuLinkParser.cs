// --------------------------------------------------------------------------------
// <copyright file="AccuLinkParser.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Models;
using AccuSync.Core.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace AccuSync.Adapters.DataParser.Services
{
    /// <summary>
    /// Parses AccuLink XML export files (also used by AccuScreen devices)
    /// into <see cref="ImportPatientData"/> records with per-test
    /// <see cref="TestPreviewItem"/>s.
    ///
    /// Key differences from ALGO 5:
    ///   - ISO 8601 dates (<c>2026-02-11T09:35:22</c>) instead of US-style.
    ///   - Patient name lives in a Contact element with <c>ContactType="Patient"</c>.
    ///   - Tests are split by type (AbrTests/TeTests/DpTests); each test is one ear.
    ///   - TestResult is a human-readable string ("Pass"/"Refer"/"Incomplete").
    ///   - <c>xsi:nil="true"</c> marks null elements.
    ///   - Contains base64 TestDetail blobs and Instrument/Transducer per test.
    /// </summary>
    public static class AccuLinkParser
    {
        private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";

        // Public API

        /// <summary>
        /// Parses an AccuLink XML file and returns one <see cref="ImportPatientData"/>
        /// per patient, each containing demographics and per-test previews.
        /// </summary>
        public static List<ImportPatientData> Parse(string filePath)
        {
            var doc = XDocument.Load(filePath);
            var results = new List<ImportPatientData>();

            var patientsContainer = doc.Root?.Element("Patients");
            if (patientsContainer == null) return results;

            foreach (var patientEl in patientsContainer.Elements("Patient"))
            {
                var patient = ParsePatient(patientEl);
                var tests = ParseTests(patientEl);

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

        private static PatientData ParsePatient(XElement el)
        {
            var p = new PatientData();

            p.SourceId = Val(el, "Id");
            p.PatientId = Val(el, "PatientRecordNumber");
            p.HospitalId = Val(el, "HospitalId");

            p.NICU = Val(el, "NicuStatus");
            p.ConsentState = Val(el, "ConsentState");
            p.TrackingConsent = Val(el, "TrackingConsent");
            p.ScreeningConsent = Val(el, "ScreeningConsent");
            p.Discharged = FormatDate(Val(el, "Discharged"));
            p.Medication = Val(el, "Medication");

            // AccuLink stores Deceased as a boolean-ish string; normalize to
            // empty (alive) or the raw value (date or "true").
            string deceasedRaw = Val(el, "Deceased");
            p.Deceased = (!string.IsNullOrEmpty(deceasedRaw) && deceasedRaw != "false") ? deceasedRaw : string.Empty;

            p.GestationalAge = Val(el, "GestationalAge");
            p.ReferralFrom = Val(el, "ReferralFrom");
            p.ReferralTo = Val(el, "ReferralTo");
            p.ReferralPhone = Val(el, "ReferralPhone");
            p.Comments = Val(el, "FreeText1");
            p.SourceCreatedAt = Val(el, "Created");
            p.SourceModifiedAt = Val(el, "Modified");

            // AccuLink uses typed Contact elements rather than flat fields.
            // Patient demographics, mother info, and caregiver info each come
            // from a separate Contact with a different ContactType.
            var contacts = el.Element("Contacts")?.Elements("Contact");
            if (contacts != null)
            {
                foreach (var contact in contacts)
                {
                    string contactType = Val(contact, "ContactType");
                    switch (contactType)
                    {
                        case "Patient":
                            ParsePatientContact(contact, p);
                            break;
                        case "Mother":
                            ParseMotherContact(contact, p);
                            break;
                        case "Father":
                            // TODO: Father contact not mapped. Extend PatientData if needed.
                            break;
                        case "Caregiver":
                            ParseCaregiverContact(contact, p);
                            break;
                    }
                }
            }

            // TODO: Risk factor parsing is provisional — needs a sample file with
            //       populated risk factors to confirm the element structure.
            var riskEl = el.Element("PatientRiskFactors");
            if (riskEl != null && riskEl.HasElements)
                p.RiskFactors = ParseRiskFactors(riskEl);

            return p;
        }

        /// <summary>
        /// The "Patient" contact carries the patient's own name, DOB, and gender.
        /// Address is also stored here and used as a fallback if the Mother
        /// contact doesn't have one.
        /// </summary>
        private static void ParsePatientContact(XElement c, PatientData p)
        {
            p.FirstName = Val(c, "Forename1");
            p.LastName = Val(c, "Surname");

            // AccuLink uses Forename2 where ALGO 5 uses MiddleInitial
            string forename2 = Val(c, "Forename2");
            if (!string.IsNullOrEmpty(forename2))
                p.MiddleInitial = forename2;

            p.DateOfBirth = FormatDate(Val(c, "DateOfBirth"));
            p.Gender = ParseGender(Val(c, "Gender"));
            p.Height = Val(c, "Height");
            p.Weight = Val(c, "Weight");
            p.BirthLocation = Val(c, "BirthLocation");
            p.Nationality = Val(c, "NationalityCode");

            // Patient's address is used as fallback for mother's address
            // when the Mother contact doesn't include one.
            string addr = Val(c, "Address1");
            if (!string.IsNullOrEmpty(addr))
            {
                p.MotherAddress1 = string.IsNullOrEmpty(p.MotherAddress1) ? addr : p.MotherAddress1;
                p.MotherCity = string.IsNullOrEmpty(p.MotherCity) ? Val(c, "City") : p.MotherCity;
                p.MotherState = string.IsNullOrEmpty(p.MotherState) ? Val(c, "State") : p.MotherState;
                p.MotherZip = string.IsNullOrEmpty(p.MotherZip) ? Val(c, "Zip") : p.MotherZip;
                p.MotherCountry = string.IsNullOrEmpty(p.MotherCountry) ? Val(c, "Country") : p.MotherCountry;
            }
        }

        private static void ParseMotherContact(XElement c, PatientData p)
        {
            p.MotherTitle = Val(c, "Title");
            p.MotherFirstName = Val(c, "Forename1");
            p.MotherLastName = Val(c, "Surname");
            p.MotherDOB = FormatDate(Val(c, "DateOfBirth"));
            p.MotherSSN = Val(c, "SocialSecurityNumber");
            p.MotherId = Val(c, "IdNumber");
            p.MotherLanguage = Val(c, "LanguageCode");
            p.MotherAddress1 = Val(c, "Address1");
            p.MotherCity = Val(c, "City");
            p.MotherState = Val(c, "State");
            p.MotherZip = Val(c, "Zip");
            p.MotherCountry = Val(c, "Country");
            p.MotherPhone = Val(c, "Phone");
            p.MotherMobile = Val(c, "CellPhone");
            p.MotherFax = Val(c, "Fax");
        }

        private static void ParseCaregiverContact(XElement c, PatientData p)
        {
            p.CaregiverTitle = Val(c, "Title");
            p.CaregiverFirstName = Val(c, "Forename1");
            p.CaregiverLastName = Val(c, "Surname");
            p.CaregiverSSN = Val(c, "SocialSecurityNumber");
            p.CaregiverLanguage = Val(c, "LanguageCode");
            p.CaregiverAddress1 = Val(c, "Address1");
            p.CaregiverCity = Val(c, "City");
            p.CaregiverState = Val(c, "State");
            p.CaregiverZip = Val(c, "Zip");
            p.CaregiverCountry = Val(c, "Country");
            p.CaregiverPhone = Val(c, "Phone");
            p.CaregiverMobile = Val(c, "CellPhone");
            p.CaregiverFax = Val(c, "Fax");
        }

        // Test parsing

        /// <summary>
        /// Extracts <see cref="TestPreviewItem"/>s from all three test containers.
        /// Unlike ALGO 5 (where each Plugin has both ears), AccuLink stores one
        /// ear per test element, so each produces exactly one item.
        /// Results are sorted chronologically.
        /// </summary>
        private static List<TestPreviewItem> ParseTests(XElement patientEl)
        {
            var items = new List<TestPreviewItem>();
            var testsEl = patientEl.Element("Tests");
            if (testsEl == null) return items;

            var abrContainer = testsEl.Element("AbrTests");
            if (abrContainer != null)
            {
                foreach (var test in abrContainer.Elements("AbrTest"))
                    items.Add(ParseSingleTest(test, "ABR"));
            }

            var teContainer = testsEl.Element("TeTests");
            if (teContainer != null)
            {
                foreach (var test in teContainer.Elements("TeTest"))
                    items.Add(ParseSingleTest(test, "TEOAE"));
            }

            var dpContainer = testsEl.Element("DpTests");
            if (dpContainer != null)
            {
                foreach (var test in dpContainer.Elements("DpTest"))
                    items.Add(ParseSingleTest(test, "DPOAE"));
            }

            items.Sort((a, b) =>
            {
                int cmp = string.Compare(a.Date, b.Date, StringComparison.Ordinal);
                return cmp != 0 ? cmp : string.Compare(a.Time, b.Time, StringComparison.Ordinal);
            });

            return items;
        }

        /// <summary>
        /// Parses a single test element. <c>TestObject</c> indicates which ear
        /// ("Left Ear", "Right Ear", or "Binaural").
        /// </summary>
        private static TestPreviewItem ParseSingleTest(XElement test, string testType)
        {
            string testDateRaw = Val(test, "TestDate");
            ParseDateTimeParts(testDateRaw, out string date, out string time);

            string testObject = Val(test, "TestObject");
            string ear = testObject switch
            {
                "Left Ear" => "Left",
                "Right Ear" => "Right",
                "Binaural" => "Both",
                _ => testObject
            };

            string result = Val(test, "TestResult");

            return new TestPreviewItem
            {
                Date = date,
                Time = time,
                Ear = ear,
                Type = testType,
                Result = NormalizeResult(result)
            };
        }

        // Risk factor parsing

        /// <summary>
        /// Provisional implementation — reads child elements as key-value pairs.
        /// Needs validation against a sample file with populated risk factors.
        /// </summary>
        private static Dictionary<string, string> ParseRiskFactors(XElement riskEl)
        {
            var dict = new Dictionary<string, string>();

            foreach (var child in riskEl.Elements())
            {
                string name = child.Name.LocalName;
                string val = child.Value?.Trim();
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(val))
                    dict[name] = val;
            }

            return dict;
        }

        // Helpers

        /// <summary>
        /// Reads a child element's trimmed text value.
        /// Returns empty string if missing or <c>xsi:nil="true"</c>.
        /// </summary>
        private static string Val(XElement parent, string name)
        {
            var el = parent?.Element(name);
            if (el == null) return string.Empty;

            var nilAttr = el.Attribute(Xsi + "nil");
            if (nilAttr != null && nilAttr.Value == "true")
                return string.Empty;

            return el.Value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// AccuLink uses text gender values. Also handles numeric codes as fallback.
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

        // TODO: Hardcoded US date output format (MM/dd/yyyy). Same cross-cutting
        //       issue flagged in TestRecord, XmlParser, Algo5XmlParser, AlgoProJsonParser.
        private static string FormatDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dt))
                return dt.ToString("MM/dd/yyyy");

            return raw;
        }

        private static void ParseDateTimeParts(string raw, out string date, out string time)
        {
            date = string.Empty;
            time = string.Empty;
            if (string.IsNullOrWhiteSpace(raw)) return;

            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dt))
            {
                date = dt.ToString("MM/dd/yyyy");
                time = dt.ToString("h:mm tt");
            }
        }

        /// <summary>
        /// AccuLink results are already human-readable. Normalize to the
        /// standard three values for <see cref="TestPreviewItem"/>.
        /// </summary>
        private static string NormalizeResult(string result)
        {
            if (string.IsNullOrWhiteSpace(result)) return "Incomplete";

            return result switch
            {
                "Pass" => "Pass",
                "Refer" => "Refer",
                "Incomplete" => "Incomplete",
                _ => "Incomplete"
            };
        }
    }
}