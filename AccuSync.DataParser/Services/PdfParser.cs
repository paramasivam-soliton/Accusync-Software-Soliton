// --------------------------------------------------------------------------------
// <copyright file="PdfParser.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Models;
using AccuSync.Core.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace AccuSync.DataParser.Services
{
    /// <summary>
    /// Extracts patient data from hospital facesheet PDFs using PdfPig (Apache 2.0)
    /// for text extraction and regex-based field mapping.
    /// Supports multiple hospital formats — each extraction method tries several
    /// patterns in priority order and returns the first match.
    /// </summary>
    // TODO: Debug.WriteLine calls throughout this file are useful during development
    //       but should be behind a verbose/trace flag or removed before release.
    //       Patient PII (names, DOB, SSN) should never be written to debug output
    //       in production.
    public class PdfParser
    {
        public static PatientData ParsePdfFacesheet(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            try
            {
                string text = ExtractTextFromPdf(filePath);

                Debug.WriteLine("=== EXTRACTED PDF TEXT ===");
                Debug.WriteLine(text);
                Debug.WriteLine("=== END EXTRACTED TEXT ===");

                // BUG: Both branches call MapTextToPatientData with the same input.
                // The scanned-PDF path should either return an empty PatientData with
                // a warning, or throw — right now it silently processes empty/garbage text.
                if (IsTextMeaningful(text))
                {
                    Debug.WriteLine("PDF: Using direct text extraction (text-based PDF)");
                    return MapTextToPatientData(text);
                }
                else
                {
                    Debug.WriteLine("PDF: No text found. This appears to be a scanned PDF.");
                    Debug.WriteLine("PDF: For scanned PDFs, use Image (OCR) import instead.");
                    return MapTextToPatientData(text);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing PDF file: {ex.Message}", ex);
            }
        }

        private static string ExtractTextFromPdf(string filePath)
        {
            var textBuilder = new StringBuilder();

            try
            {
                using (PdfDocument document = PdfDocument.Open(filePath))
                {
                    foreach (Page page in document.GetPages())
                    {
                        textBuilder.AppendLine(page.Text);
                    }
                }

                Debug.WriteLine($"PDF: Extracted {textBuilder.Length} characters from document");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PDF: Text extraction failed - {ex.Message}");
                return string.Empty;
            }

            return textBuilder.ToString();
        }

        /// <summary>
        /// Quick heuristic to distinguish text-based PDFs from scanned images.
        /// Requires at least 50 characters with 20+ alphanumeric to count as meaningful.
        /// </summary>
        private static bool IsTextMeaningful(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (text.Length < 50)
                return false;

            int alphanumericCount = text.Count(c => char.IsLetterOrDigit(c));
            return alphanumericCount > 20;
        }

        private static PatientData MapTextToPatientData(string text)
        {
            var patient = new PatientData
            {
                RiskFactors = new Dictionary<string, string>()
            };

            Debug.WriteLine("=== STARTING FIELD EXTRACTION ===");

            patient.PatientId = ExtractPatientId(text);

            var nameData = ExtractPatientName(text);
            patient.FirstName = nameData.firstName;
            patient.LastName = nameData.lastName;

            patient.DateOfBirth = ExtractDateOfBirth(text);
            patient.Gender = ExtractGender(text);
            patient.Weight = ExtractWeight(text);
            patient.Height = ExtractHeight(text);

            var motherNameData = ExtractMotherName(text);
            patient.MotherFirstName = motherNameData.firstName;
            patient.MotherLastName = motherNameData.lastName;

            patient.MotherPhone = ExtractMotherPhone(text);

            Debug.WriteLine("=== EXTRACTION RESULTS ===");
            Debug.WriteLine($"Patient ID: '{patient.PatientId}'");
            Debug.WriteLine($"First Name: '{patient.FirstName}'");
            Debug.WriteLine($"Last Name: '{patient.LastName}'");
            Debug.WriteLine($"DOB: '{patient.DateOfBirth}'");
            Debug.WriteLine($"Gender: '{patient.Gender}'");
            Debug.WriteLine($"Weight: '{patient.Weight}'");
            Debug.WriteLine($"Height: '{patient.Height}'");
            Debug.WriteLine($"Mother First: '{patient.MotherFirstName}'");
            Debug.WriteLine($"Mother Last: '{patient.MotherLastName}'");
            Debug.WriteLine($"Mother Phone: '{patient.MotherPhone}'");
            Debug.WriteLine("=== END EXTRACTION ===");

            ExtractRiskFactors(text, patient.RiskFactors);

            return patient;
        }

        #region Field Extraction Methods

        // Each Extract* method tries patterns in priority order (most specific first)
        // and returns the first successful match.

        private static string ExtractPatientId(string text)
        {
            var patterns = new[]
            {
                @"Medical\s+Record\s+#?([A-Z0-9\-]+)",
                @"Medical\s+Record[:\s]+([A-Z0-9\-]+)",
                @"Patient\s*ID[:\s]+([A-Z0-9\-]+)",
                @"MRN[:\s]+([A-Z0-9\-]+)",
                @"Chart\s+ID\s+([A-Z0-9\-]+)",
                @"(?:^|\n)\s*(\d{3}-\d{2}-\d{2}-\d{3})\s*$"
            };

            string id = ExtractFirstMatch(text, patterns);

            // Strip hospital-specific suffixes (e.g., "E2626262-svmc" → "E2626262")
            if (!string.IsNullOrEmpty(id) && id.Contains("-"))
                id = id.Split('-')[0];

            Debug.WriteLine($"DEBUG: Patient ID = '{id}'");
            return id;
        }

        // NOTE: Patient name extraction is the most complex part of the parser.
        //       Hospital facesheets vary wildly in format. Patterns are ordered from
        //       most specific (all-caps newborn) to most generic (labeled fields).
        //       When adding new hospital formats, add patterns before the labeled
        //       fallback (Format 4) and include a sample in the comment.
        private static (string firstName, string lastName) ExtractPatientName(string text)
        {
            string topSection = text.Length > 500 ? text.Substring(0, 500) : text;

            // Format 1: "BAKER, BABY" (all-caps newborn)
            var allCapsMatch = Regex.Match(topSection, @"([A-Z]{2,}),\s+(BABY|Baby)", RegexOptions.Multiline);
            if (allCapsMatch.Success)
            {
                string lastName = allCapsMatch.Groups[1].Value.Trim();
                string firstName = allCapsMatch.Groups[2].Value.Trim();
                lastName = char.ToUpper(lastName[0]) + lastName.Substring(1).ToLower();
                firstName = char.ToUpper(firstName[0]) + firstName.Substring(1).ToLower();
                Debug.WriteLine($"DEBUG: All caps format - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 2: "Garcia-Mendez, Girl A Sophia P" (complex newborn naming)
            var complexMatch = Regex.Match(topSection,
                @"([A-Z][a-z]+(?:-[A-Z][a-z]+)?),\s+(?:Girl|Boy)\s+[A-Z]\s+",
                RegexOptions.Multiline);
            if (complexMatch.Success)
            {
                string lastName = complexMatch.Groups[1].Value.Trim();
                Debug.WriteLine($"DEBUG: Complex name format - Last: '{lastName}', First: 'Baby'");
                return ("Baby", lastName);
            }

            // Format 3: "Clemence, Baby 1 day old"
            string veryTop = text.Length > 200 ? text.Substring(0, 200) : text;
            var simpleMatch = Regex.Match(veryTop,
                @"([A-Z][a-z]+),\s+([A-Z][a-z]+)\s+\d+\s+day",
                RegexOptions.Multiline);
            if (simpleMatch.Success)
            {
                string lastName = simpleMatch.Groups[1].Value.Trim();
                string firstName = simpleMatch.Groups[2].Value.Trim();
                Debug.WriteLine($"DEBUG: Simple name format - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 4: Standard labeled fields ("First Name: ...", "Last Name: ...")
            string firstName4 = ExtractFirstMatch(text, new[] {
                @"First\s*Name[:\s]+([A-Z][a-z]+)",
                @"Given\s*Name[:\s]+([A-Z][a-z]+)"
            });
            string lastName4 = ExtractFirstMatch(text, new[] {
                @"Last\s*Name[:\s]+([A-Z][a-z]+)",
                @"Surname[:\s]+([A-Z][a-z]+)"
            });

            if (!string.IsNullOrEmpty(firstName4) || !string.IsNullOrEmpty(lastName4))
            {
                Debug.WriteLine($"DEBUG: Labeled format - Last: '{lastName4}', First: '{firstName4}'");
                return (firstName4, lastName4);
            }

            Debug.WriteLine("DEBUG: No name pattern matched");
            return (string.Empty, string.Empty);
        }

        private static string ExtractDateOfBirth(string text)
        {
            var patterns = new[]
            {
                @"\(DOB:\s*(\d{1,2}/\d{1,2}/\d{4})\)",
                @"DOB\s+(\d{1,2}/\d{1,2}/\d{4})",
                @"(?:DOB|Date\s*of\s*Birth|Birth\s*Date)[:\s]+(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
                @"(?:DOB|Date\s*of\s*Birth|Birth\s*Date)[:\s]+((?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]*\s+\d{1,2},?\s+\d{4})",
                @"(?:DOB|Date\s*of\s*Birth|Birth\s*Date)[:\s]+(\d{1,2}\s+(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]*\s+\d{4})"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (match.Success && match.Groups.Count > 1)
                {
                    string dob = match.Groups[1].Value.Trim();
                    int dobIndex = match.Index;

                    // Avoid matching "Subscriber DOB" (insurance section) instead
                    // of the patient's own DOB.
                    int subscriberIndex = text.IndexOf("Subscriber DOB", StringComparison.OrdinalIgnoreCase);

                    if (subscriberIndex == -1 || dobIndex < subscriberIndex)
                    {
                        Debug.WriteLine($"DEBUG: Patient DOB (found) = '{dob}'");
                        return dob;
                    }
                }
            }

            Debug.WriteLine("DEBUG: No DOB found");
            return string.Empty;
        }

        private static string ExtractGender(string text)
        {
            var patterns = new[]
            {
                @"\d+\s+days?\s+old\s+(male|female)",
                @"\d+\s+days?\s+(male|female)",
                @"Sex\s+(Male|Female|M|F)",
                @"(?:Gender|Sex)[:\s]+(Male|Female|M|F)",
                @"(?:^|\s)(Male|Female)(?:\s|$)"
            };

            string match = ExtractFirstMatch(text, patterns);

            if (!string.IsNullOrEmpty(match))
            {
                match = match.Trim().ToUpper();
                string result = match switch
                {
                    "M" or "MALE" => "Male",
                    "F" or "FEMALE" => "Female",
                    _ => match
                };
                Debug.WriteLine($"DEBUG: Gender = '{result}'");
                return result;
            }

            return string.Empty;
        }

        /// <summary>
        /// Extracts birth weight with heuristic guards against false positives.
        /// Rejects values that look like years, ZIP codes, or unrealistic weights.
        /// </summary>
        private static string ExtractWeight(string text)
        {
            var patterns = new[]
            {
                @"(?:Birth\s*)?Weight[:\s]+(\d+\.?\d*)\s*(?:g|grams|kg|lbs|pounds)?",
                @"Weight[:\s]+(\d+\.?\d*)",
                @"(\d+\.?\d*)\s*(?:g|grams)\s*(?:birth\s*weight)?",
            };

            string weight = ExtractFirstMatch(text, patterns);

            if (!string.IsNullOrEmpty(weight))
            {
                // Facesheet text is unstructured, so weight patterns can match
                // unrelated numbers. These guards reject common false positives.
                if (weight.Length == 4 && int.TryParse(weight, out int year) && year >= 1900 && year <= 2100)
                {
                    Debug.WriteLine($"DEBUG: Rejecting weight '{weight}' - appears to be a year");
                    return string.Empty;
                }

                if (weight.Length == 5 && int.TryParse(weight, out int zip) && zip >= 10000)
                {
                    Debug.WriteLine($"DEBUG: Rejecting weight '{weight}' - appears to be ZIP code");
                    return string.Empty;
                }

                if (weight.Length > 6)
                {
                    Debug.WriteLine($"DEBUG: Rejecting weight '{weight}' - too long (likely artifact)");
                    return string.Empty;
                }

                if (int.TryParse(weight, out int val))
                {
                    if (val > 10000 || (val > 50 && val < 200))
                    {
                        Debug.WriteLine($"DEBUG: Rejecting weight '{weight}' - unrealistic value");
                        return string.Empty;
                    }
                }
            }

            Debug.WriteLine($"DEBUG: Weight extracted = '{weight}'");
            return weight;
        }

        private static string ExtractHeight(string text)
        {
            var patterns = new[]
            {
                @"(?:Birth\s*)?(?:Height|Length)[:\s]+(\d+\.?\d*)\s*(?:cm|in|inches)?",
                @"(?:Height|Length)[:\s]+(\d+\.?\d*)",
            };
            return ExtractFirstMatch(text, patterns);
        }

        // Mother name uses the same pattern-priority approach as patient name.
        // Formats 1–2 handle all-caps layouts, 3–5 handle mixed-case and
        // guarantor-section variations, 6 is the labeled fallback.
        private static (string firstName, string lastName) ExtractMotherName(string text)
        {
            // Format 1: "BAKER, JILL            MOTHER         YES"
            var capsSpacesMatch = Regex.Match(text,
                @"([A-Z]{2,}),\s+([A-Z]+)\s+MOTHER",
                RegexOptions.Multiline);
            if (capsSpacesMatch.Success)
            {
                string lastName = capsSpacesMatch.Groups[1].Value.Trim();
                string firstName = capsSpacesMatch.Groups[2].Value.Trim();
                lastName = char.ToUpper(lastName[0]) + lastName.Substring(1).ToLower();
                firstName = char.ToUpper(firstName[0]) + firstName.Substring(1).ToLower();
                Debug.WriteLine($"DEBUG: Mother name (caps spaces) - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 2: "CLEMENCE, LAURA B         MOTHER"
            var capsMatch = Regex.Match(text,
                @"([A-Z]{2,}),\s+([A-Z]+)\s+[A-Z]\s+MOTHER",
                RegexOptions.Multiline);
            if (capsMatch.Success)
            {
                string lastName = capsMatch.Groups[1].Value.Trim();
                string firstName = capsMatch.Groups[2].Value.Trim();
                lastName = char.ToUpper(lastName[0]) + lastName.Substring(1).ToLower();
                firstName = char.ToUpper(firstName[0]) + firstName.Substring(1).ToLower();
                Debug.WriteLine($"DEBUG: Mother name (caps format) - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 3: "Sophia P Garcia-Mendez (Mother)"
            var parenthesesMatch = Regex.Match(text,
                @"([A-Z][a-z]+)\s+[A-Z]\s+([A-Z][a-z]+(?:-[A-Z][a-z]+)?)\s+\(Mother\)",
                RegexOptions.Multiline);
            if (parenthesesMatch.Success)
            {
                string firstName = parenthesesMatch.Groups[1].Value.Trim();
                string lastName = parenthesesMatch.Groups[2].Value.Trim();
                Debug.WriteLine($"DEBUG: Mother name (parentheses format) - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 4: "Garcia-Mendez, Sophia P" in Guarantor section
            var guarantorMatch = Regex.Match(text,
                @"Guarantor.*?([A-Z][a-z]+(?:-[A-Z][a-z]+)?),\s+([A-Z][a-z]+)\s+[A-Z]",
                RegexOptions.Singleline);
            if (guarantorMatch.Success)
            {
                string lastName = guarantorMatch.Groups[1].Value.Trim();
                string firstName = guarantorMatch.Groups[2].Value.Trim();
                Debug.WriteLine($"DEBUG: Mother name (guarantor format) - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 5: "BAKER, JILL L 2002002" in Guarantor section
            var guarantorIdMatch = Regex.Match(text,
                @"Guarantor.*?([A-Z]{2,}),\s+([A-Z]+)\s+[A-Z]",
                RegexOptions.Singleline);
            if (guarantorIdMatch.Success)
            {
                string lastName = guarantorIdMatch.Groups[1].Value.Trim();
                string firstName = guarantorIdMatch.Groups[2].Value.Trim();
                lastName = char.ToUpper(lastName[0]) + lastName.Substring(1).ToLower();
                firstName = char.ToUpper(firstName[0]) + firstName.Substring(1).ToLower();
                Debug.WriteLine($"DEBUG: Mother name (guarantor ID) - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 6: Standard labeled fields
            string firstName6 = ExtractFirstMatch(text, new[] {
                @"Mother'?s?\s*First\s*Name[:\s]+([A-Z][a-z]+)",
                @"Mother[:\s]+([A-Z][a-z]+)\s+[A-Z][a-z]+"
            });
            string lastName6 = ExtractFirstMatch(text, new[] {
                @"Mother'?s?\s*Last\s*Name[:\s]+([A-Z][a-z]+)",
                @"Mother[:\s]+[A-Z][a-z]+\s+([A-Z][a-z]+)"
            });

            if (!string.IsNullOrEmpty(firstName6) || !string.IsNullOrEmpty(lastName6))
            {
                Debug.WriteLine($"DEBUG: Mother name (labeled format) - Last: '{lastName6}', First: '{firstName6}'");
                return (firstName6, lastName6);
            }

            Debug.WriteLine("DEBUG: No mother name pattern matched");
            return (string.Empty, string.Empty);
        }

        private static string ExtractMotherPhone(string text)
        {
            var patterns = new[]
            {
                @"MOTHER\s+YES.*?(\d{3}-\d{3}-\d{4})",
                @"(?:Mother|Emergency\s+Contact).*?(\d{3}-\d{3}-\d{4})",
                @"Mobile\s+phone\s+(\d{3}-\d{3}-\d{4})",
                @"Mobile\s+(\d{3}-\d{3}-\d{4})",
                @"(?:Mother'?s?\s*)?(?:Phone|Tel|Telephone|Mobile)[:\s]+([\d\-\(\)\s]+)",
                @"Contact[:\s]+([\d\-\(\)\s]{10,})",
                @"Guarantor.*?Home\s+(\d{3}-\d{3}-\d{4})"
            };

            string phone = ExtractFirstMatch(text, patterns);

            if (!string.IsNullOrEmpty(phone))
            {
                phone = phone.Trim();
                if (phone.Equals("None", StringComparison.OrdinalIgnoreCase))
                    return string.Empty;

                Debug.WriteLine($"DEBUG: Mother phone = '{phone}'");
            }

            return phone;
        }

        /// <summary>
        /// Scans the full text for known risk factor keywords and determines
        /// Yes/No/Unknown by looking at the surrounding context (±50 characters).
        /// </summary>
        // TODO: Keyword matching against the full document text can produce false
        //       positives (e.g., "meningitis" mentioned in an unrelated section).
        //       Restricting matches to a risk factors section would be more reliable.
        private static void ExtractRiskFactors(string text, Dictionary<string, string> riskFactors)
        {
            var riskKeywords = new Dictionary<string, string[]>
            {
                { "Family History of Permanent Childhood Hearing Loss", new[] { "family history", "hearing loss family", "family hearing" } },
                { "Low Birth Weight", new[] { "low birth weight", "low weight", "underweight" } },
                { "Hyperbilirubinemia with Exchange Transfusion", new[] { "hyperbilirubinemia", "jaundice", "bilirubin", "exchange transfusion" } },
                { "Asphyxia- Hypoxic Ischemic Encephalopathy (HIE)", new[] { "asphyxia", "hie", "hypoxic", "ischemic" } },
                { "Craniofacial Anomalies-Microtia/Atresia, Clefting", new[] { "craniofacial", "microtia", "atresia", "cleft", "clefting" } },
                { "Syndromes & Genetic Disorders of Hearing Loss", new[] { "syndrome", "genetic disorder", "genetic", "chromosomal" } },
                { "In Utero Infections such as CMV & Zika", new[] { "in utero", "cmv", "cytomegalovirus", "zika", "torch" } },
                { "Bacterial Meningitis", new[] { "meningitis", "bacterial meningitis" } },
                { "Ototoxic Medications", new[] { "ototoxic", "ototoxicity", "toxic medication" } },
                { "Perinatal or Postnatal Infection", new[] { "perinatal", "postnatal", "neonatal infection" } },
                { "Prolonged Ventilation", new[] { "ventilation", "ventilator", "intubation", "mechanical ventilation" } },
                { "Aminoglycosides for >5 Days", new[] { "aminoglycoside", "gentamicin", "tobramycin" } },
                { "Caregiver Concern", new[] { "caregiver concern", "parent concern", "family concern" } },
                { "NICU >5 Days", new[] { "nicu", "intensive care", "icu" } },
                { "Exposure to Head Trauma or Chemotherapy", new[] { "head trauma", "chemotherapy", "chemo", "trauma" } },
                { "ECMO-Extracorporeal Membrane Oxygenation", new[] { "ecmo", "extracorporeal", "membrane oxygenation" } }
            };

            string textLower = text.ToLower();

            foreach (var riskFactor in riskKeywords)
            {
                bool found = false;
                foreach (string keyword in riskFactor.Value)
                {
                    if (textLower.Contains(keyword))
                    {
                        string value = DetermineRiskValue(text, keyword);
                        riskFactors[riskFactor.Key] = value;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    riskFactors[riskFactor.Key] = "Unknown";
            }
        }

        /// <summary>
        /// Looks at the ±50 character window around a keyword match to determine
        /// whether the risk factor is affirmed, denied, or indeterminate.
        /// </summary>
        private static string DetermineRiskValue(string text, string keyword)
        {
            int idx = text.ToLower().IndexOf(keyword);
            if (idx == -1) return "Unknown";

            int start = Math.Max(0, idx - 50);
            int length = Math.Min(100, text.Length - start);
            string context = text.Substring(start, length).ToLower();

            if (Regex.IsMatch(context, @"\b(yes|positive|present|confirmed)\b|(\+|✓|check|☑)"))
                return "Yes";

            if (Regex.IsMatch(context, @"\b(no|negative|absent|denied|none)\b|(\-|✗|☐)"))
                return "No";

            return "Unknown";
        }

        /// <summary>
        /// Tries each regex pattern against <paramref name="text"/> and returns
        /// the first non-empty capture group match.
        /// </summary>
        private static string ExtractFirstMatch(string text, string[] patterns)
        {
            foreach (string pattern in patterns)
            {
                var match = Regex.Match(text, pattern,
                    RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
                if (match.Success && match.Groups.Count > 1)
                {
                    string result = match.Groups[1].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(result))
                        return result;
                }
            }
            return string.Empty;
        }

        #endregion

        /// <summary>
        /// Returns extracted text for the facesheet review step so users can
        /// verify what the parser is working with before committing the import.
        /// </summary>
        public static string GetExtractedText(string filePath)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            try
            {
                return ExtractTextFromPdf(filePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error extracting PDF text for preview: {ex.Message}");
                return $"[Error extracting text: {ex.Message}]";
            }
        }
    }
}