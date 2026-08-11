// --------------------------------------------------------------------------------
// <copyright file="DocxParser.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Models;
using AccuSync.Core.Entities;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AccuSync.Adapters.DataParser.Services
{
    /// <summary>
    /// Extracts patient data from Word (.docx) facesheet documents using
    /// OpenXml for text extraction and regex-based field mapping.
    /// Supports the same hospital formats as <see cref="PdfParser"/> and
    /// <see cref="OcrParser"/>, plus Markdown-formatted DOCX variations.
    /// </summary>
    /// <summary>
    /// Extracts patient data from Word (.docx) facesheet documents using
    /// OpenXml for text extraction and regex-based field mapping.
    /// Supports the same hospital formats as <see cref="PdfParser"/> and
    /// <see cref="OcrParser"/>, plus Markdown-formatted DOCX variations.
    /// </summary>
    // TODO: This is the third copy of the facesheet extraction logic (alongside
    //       PdfParser and OcrParser). Only the text-extraction step differs.
    //       Extract a shared FacesheetFieldExtractor and have each parser call it.
    // TODO: Same PII-in-debug-output concern as PdfParser and OcrParser.
    public class DocxParser
    {
        /// <summary>
        /// Parses a Word (.docx) facesheet into a <see cref="PatientData"/> record.
        /// </summary>
        /// <param name="filePath">Path to the DOCX facesheet to parse.</param>
        /// <returns>The extracted <see cref="PatientData"/>.</returns>
        public static PatientData ParseDocxFacesheet(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            try
            {
                string text = ExtractTextFromDocx(filePath);

                Debug.WriteLine("=== EXTRACTED DOCX TEXT ===");
                Debug.WriteLine(text);
                Debug.WriteLine("=== END EXTRACTED TEXT ===");

                if (IsTextMeaningful(text))
                {
                    Debug.WriteLine("DOCX: Successfully extracted text from Word document");
                    return MapTextToPatientData(text);
                }
                else
                {
                    Debug.WriteLine("DOCX: No meaningful text found in document");
                    throw new Exception("Document appears to be empty or unreadable");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing DOCX file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extracts text from both paragraphs and tables. Table cells are joined
        /// with spaces per row so labeled fields ("Weight: 3200g") stay on one line
        /// for the regex patterns to match.
        /// </summary>
        private static string ExtractTextFromDocx(string filePath)
        {
            var textBuilder = new StringBuilder();

            try
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
                {
                    var body = wordDoc.MainDocumentPart?.Document?.Body;
                    if (body == null)
                    {
                        Debug.WriteLine("DOCX: Document body is null");
                        return string.Empty;
                    }

                    var paragraphs = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>();
                    foreach (var para in paragraphs)
                    {
                        string paraText = para.InnerText;
                        if (!string.IsNullOrWhiteSpace(paraText))
                        {
                            textBuilder.AppendLine(paraText);
                        }
                    }

                    // Table content is extracted separately because InnerText on
                    // paragraphs alone misses text inside table cells.
                    var tables = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Table>();
                    foreach (var table in tables)
                    {
                        var rows = table.Descendants<DocumentFormat.OpenXml.Wordprocessing.TableRow>();
                        foreach (var row in rows)
                        {
                            var cells = row.Descendants<DocumentFormat.OpenXml.Wordprocessing.TableCell>();
                            var cellTexts = cells.Select(c => c.InnerText.Trim()).Where(t => !string.IsNullOrWhiteSpace(t));
                            textBuilder.AppendLine(string.Join(" ", cellTexts));
                        }
                    }

                    Debug.WriteLine($"DOCX: Extracted {textBuilder.Length} characters from document");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DOCX: Text extraction failed - {ex.Message}");
                throw;
            }

            return textBuilder.ToString();
        }

        /// <summary>
        /// Quick heuristic to detect empty or image-only documents.
        /// Same thresholds as <see cref="PdfParser.IsTextMeaningful"/>.
        /// </summary>
        private static bool IsTextMeaningful(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.WriteLine("DOCX: Text is null or whitespace");
                return false;
            }

            if (text.Length < 50)
            {
                Debug.WriteLine($"DOCX: Text too short ({text.Length} chars)");
                return false;
            }

            int alphanumericCount = text.Count(c => char.IsLetterOrDigit(c));
            bool meaningful = alphanumericCount > 20;

            Debug.WriteLine($"DOCX: Text has {alphanumericCount} alphanumeric chars - Meaningful: {meaningful}");
            return meaningful;
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
        // All extraction methods below are duplicated across PdfParser, OcrParser,
        // and DocxParser. DocxParser adds Markdown-aware patterns (** delimiters)
        // because some DOCX facesheets contain Markdown formatting.
        // See the class-level TODO about extracting a shared helper.

        private static string ExtractPatientId(string text)
        {
            var patterns = new[]
            {
                @"Medical\s+Record\s+#?\s*([A-Z]\d+)",
                @"Medical\s+Record[:\s*]+\*\*([A-Z0-9\-]+)\*\*",
                @"\*\*Medical\s+Record\*\*\s+\*\*([A-Z0-9\-]+)",
                @"Chart\s+ID\s+#?\s*([A-Z0-9\-]+)",
                @"Medical\s+Record[:\s]+([A-Z0-9\-]+)",
                @"Patient\s*ID[:\s]+([A-Z0-9\-]+)",
                @"MRN[:\s]+([A-Z0-9\-]+)"
            };

            string id = ExtractFirstMatch(text, patterns);

            if (!string.IsNullOrEmpty(id) && id.Contains("-"))
                id = id.Split('-')[0];

            Debug.WriteLine($"DEBUG: Patient ID extracted = '{id}'");
            return id;
        }

        // Patient name extraction handles both Markdown and plain-text formats.
        // Markdown patterns (Formats 1–2) come first since they're more specific.
        // See PdfParser.ExtractPatientName for format documentation.
        private static (string firstName, string lastName) ExtractPatientName(string text)
        {
            string topSection = text.Length > 400 ? text.Substring(0, 400) : text;

            // Format 1: "**Garcia-Mendez, Girl A Sophia P**" (Markdown bold newborn)
            var markdownComplexMatch = Regex.Match(topSection,
                @"\*\*([A-Z][a-z]+(?:-[A-Z][a-z]+)?),\s+(?:Girl|Boy)\s+[A-Z]\s+[A-Z][a-z]+",
                RegexOptions.Multiline);
            if (markdownComplexMatch.Success)
            {
                string lastName = markdownComplexMatch.Groups[1].Value.Trim();
                Debug.WriteLine($"DEBUG: Markdown complex format - Last: '{lastName}', First: 'Baby'");
                return ("Baby", lastName);
            }

            // Format 2: "**Smith, Boy A Crystal**" (Markdown newborn)
            var markdownBoyGirlMatch = Regex.Match(topSection,
                @"\*\*([A-Z][a-z]+),\s+(?:Boy|Girl)\s+[A-Z]",
                RegexOptions.Multiline);
            if (markdownBoyGirlMatch.Success)
            {
                string lastName = markdownBoyGirlMatch.Groups[1].Value.Trim();
                Debug.WriteLine($"DEBUG: Markdown Boy/Girl format - Last: '{lastName}', First: 'Baby'");
                return ("Baby", lastName);
            }

            // Format 3: "BAKER, BABY" (all-caps newborn)
            var allCapsMatch = Regex.Match(topSection, @"([A-Z]+),\s+(BABY|Baby)", RegexOptions.Multiline);
            if (allCapsMatch.Success)
            {
                string lastName = allCapsMatch.Groups[1].Value.Trim();
                string firstName = allCapsMatch.Groups[2].Value.Trim();
                lastName = char.ToUpper(lastName[0]) + lastName.Substring(1).ToLower();
                firstName = char.ToUpper(firstName[0]) + firstName.Substring(1).ToLower();
                Debug.WriteLine($"DEBUG: All caps format - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 4: "Garcia-Mendez, Girl A Sophia P" (plain text)
            var complexMatch = Regex.Match(topSection,
                @"([A-Z][a-z]+(?:-[A-Z][a-z]+)?),\s+(?:Girl|Boy)\s+[A-Z]\s+",
                RegexOptions.Multiline);
            if (complexMatch.Success)
            {
                string lastName = complexMatch.Groups[1].Value.Trim();
                Debug.WriteLine($"DEBUG: Complex name format - Last: '{lastName}', First: 'Baby'");
                return ("Baby", lastName);
            }

            // Format 5: "Clemence, Baby 1 day old"
            string veryTop = text.Length > 200 ? text.Substring(0, 200) : text;
            var simpleMatch = Regex.Match(veryTop, @"([A-Z][a-z]+),\s+([A-Z][a-z]+)\s+\d+\s+day", RegexOptions.Multiline);
            if (simpleMatch.Success)
            {
                string lastName = simpleMatch.Groups[1].Value.Trim();
                string firstName = simpleMatch.Groups[2].Value.Trim();
                Debug.WriteLine($"DEBUG: Simple name format - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 6: Standard labeled fields
            string firstName6 = ExtractFirstMatch(text, new[] {
                @"First\s*Name[:\s]+([A-Z][a-z]+)",
                @"Given\s*Name[:\s]+([A-Z][a-z]+)",
                @"Patient\s+First\s*Name[:\s]+([A-Z][a-z]+)"
            });

            string lastName6 = ExtractFirstMatch(text, new[] {
                @"Last\s*Name[:\s]+([A-Z][a-z]+)",
                @"Surname[:\s]+([A-Z][a-z]+)",
                @"Patient\s+Last\s*Name[:\s]+([A-Z][a-z]+)"
            });

            if (!string.IsNullOrEmpty(firstName6) || !string.IsNullOrEmpty(lastName6))
            {
                Debug.WriteLine($"DEBUG: Labeled format - Last: '{lastName6}', First: '{firstName6}'");
                return (firstName6, lastName6);
            }

            Debug.WriteLine("DEBUG: No name pattern matched");
            return (string.Empty, string.Empty);
        }

        private static string ExtractDateOfBirth(string text)
        {
            var patterns = new[]
            {
                @"\(DOB:\s*(\d{1,2}/\d{1,2}/\d{4})\)",
                @"DOB[:\s]+(\d{1,2}/\d{1,2}/\d{4})",
                @"(?:Date\s*of\s*Birth|Birth\s*Date)[:\s]+(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
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

                    // Skip "Subscriber DOB" matches (insurance section)
                    int subscriberIndex = text.IndexOf("Subscriber DOB", StringComparison.OrdinalIgnoreCase);

                    if (subscriberIndex == -1 || dobIndex < subscriberIndex)
                    {
                        Debug.WriteLine($"DEBUG: Patient DOB extracted = '{dob}'");
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
                @"Sex[:\s]+(Male|Female|M|F)",
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
                Debug.WriteLine($"DEBUG: Gender extracted = '{result}'");
                return result;
            }

            Debug.WriteLine("DEBUG: No gender found");
            return string.Empty;
        }

        /// <summary>
        /// Extracts birth weight with false-positive guards.
        /// See <see cref="PdfParser.ExtractWeight"/> for detailed documentation.
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

                if (int.TryParse(weight, out int weightValue))
                {
                    if (weightValue > 10000 || (weightValue > 50 && weightValue < 200))
                    {
                        Debug.WriteLine($"DEBUG: Rejecting weight '{weight}' - unrealistic value");
                        return string.Empty;
                    }
                }

                Debug.WriteLine($"DEBUG: Weight extracted = '{weight}'");
            }
            else
            {
                Debug.WriteLine("DEBUG: No weight found");
            }

            return weight;
        }

        private static string ExtractHeight(string text)
        {
            var patterns = new[]
            {
                @"(?:Birth\s*)?(?:Height|Length)[:\s]+(\d+\.?\d*)\s*(?:cm|in|inches)?",
                @"(?:Height|Length)[:\s]+(\d+\.?\d*)",
            };

            string height = ExtractFirstMatch(text, patterns);

            if (!string.IsNullOrEmpty(height))
                Debug.WriteLine($"DEBUG: Height extracted = '{height}'");
            else
                Debug.WriteLine("DEBUG: No height found");

            return height;
        }

        // Mother name extraction adds Markdown patterns (Formats 1–2, 6) on top of
        // the shared formats in PdfParser/OcrParser.
        private static (string firstName, string lastName) ExtractMotherName(string text)
        {
            // Format 1: "**Crystal Smith (Mother) - 789-999-4444**"
            var markdownParenMatch = Regex.Match(text,
                @"\*\*([A-Z][a-z]+)\s+([A-Z][a-z]+(?:-[A-Z][a-z]+)?)\s+\(Mother\)",
                RegexOptions.Multiline);
            if (markdownParenMatch.Success)
            {
                string firstName = markdownParenMatch.Groups[1].Value.Trim();
                string lastName = markdownParenMatch.Groups[2].Value.Trim();
                Debug.WriteLine($"DEBUG: Mother name (markdown paren) - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 2: "**Sophia P Garcia-Mendez (Mother) - 123-456-7890**"
            var markdownFullMatch = Regex.Match(text,
                @"\*\*([A-Z][a-z]+)\s+[A-Z]\s+([A-Z][a-z]+(?:-[A-Z][a-z]+)?)\s+\(Mother\)",
                RegexOptions.Multiline);
            if (markdownFullMatch.Success)
            {
                string firstName = markdownFullMatch.Groups[1].Value.Trim();
                string lastName = markdownFullMatch.Groups[2].Value.Trim();
                Debug.WriteLine($"DEBUG: Mother name (markdown full) - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 3: "BAKER, JILL            MOTHER         YES"
            var capsSpacesMatch = Regex.Match(text, @"([A-Z]{2,}),\s+([A-Z]+)\s+MOTHER", RegexOptions.Multiline);
            if (capsSpacesMatch.Success)
            {
                string lastName = capsSpacesMatch.Groups[1].Value.Trim();
                string firstName = capsSpacesMatch.Groups[2].Value.Trim();
                lastName = char.ToUpper(lastName[0]) + lastName.Substring(1).ToLower();
                firstName = char.ToUpper(firstName[0]) + firstName.Substring(1).ToLower();
                Debug.WriteLine($"DEBUG: Mother name (caps spaces) - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 4: "CLEMENCE, LAURA B         MOTHER"
            var capsMatch = Regex.Match(text, @"([A-Z]{2,}),\s+([A-Z]+)\s+[A-Z]\s+MOTHER", RegexOptions.Multiline);
            if (capsMatch.Success)
            {
                string lastName = capsMatch.Groups[1].Value.Trim();
                string firstName = capsMatch.Groups[2].Value.Trim();
                lastName = char.ToUpper(lastName[0]) + lastName.Substring(1).ToLower();
                firstName = char.ToUpper(firstName[0]) + firstName.Substring(1).ToLower();
                Debug.WriteLine($"DEBUG: Mother name (caps middle) - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 5: "Sophia P Garcia-Mendez (Mother)"
            var parenthesesMatch = Regex.Match(text,
                @"([A-Z][a-z]+)\s+[A-Z]\s+([A-Z][a-z]+(?:-[A-Z][a-z]+)?)\s+\(Mother\)",
                RegexOptions.Multiline);
            if (parenthesesMatch.Success)
            {
                string firstName = parenthesesMatch.Groups[1].Value.Trim();
                string lastName = parenthesesMatch.Groups[2].Value.Trim();
                Debug.WriteLine($"DEBUG: Mother name (parentheses) - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 6: "**Smith, Crystal**" in Guarantor section (Markdown)
            var guarantorMarkdownMatch = Regex.Match(text,
                @"Guarantor.*?\*\*([A-Z][a-z]+(?:-[A-Z][a-z]+)?),\s*([A-Z][a-z]+)\*\*",
                RegexOptions.Singleline);
            if (guarantorMarkdownMatch.Success)
            {
                string lastName = guarantorMarkdownMatch.Groups[1].Value.Trim();
                string firstName = guarantorMarkdownMatch.Groups[2].Value.Trim();
                Debug.WriteLine($"DEBUG: Mother name (guarantor markdown) - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 7: "Garcia-Mendez, Sophia P" in Guarantor section
            var guarantorMatch = Regex.Match(text,
                @"Guarantor.*?([A-Z][a-z]+(?:-[A-Z][a-z]+)?),\s+([A-Z][a-z]+)\s+[A-Z]",
                RegexOptions.Singleline);
            if (guarantorMatch.Success)
            {
                string lastName = guarantorMatch.Groups[1].Value.Trim();
                string firstName = guarantorMatch.Groups[2].Value.Trim();
                Debug.WriteLine($"DEBUG: Mother name (guarantor) - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 8: "BAKER, JILL L 2002002" in Guarantor section
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

            Debug.WriteLine("DEBUG: No mother name pattern matched");
            return (string.Empty, string.Empty);
        }

        // DocxParser adds Markdown-aware phone patterns on top of the shared set.
        private static string ExtractMotherPhone(string text)
        {
            var patterns = new[]
            {
                @"\(Mother\)\s*-\s*(\d{3}-\d{3}-\d{4})",
                @"Mother\)\s*-\s*\*\*\s*(\d{3}-\d{3}-\d{4})",
                @"Mother.*?(\d{3}-\d{3}-\d{4})",
                @"(?:Mother|Emergency\s+Contact).*?(\d{3}-\d{3}-\d{4})",
                @"Mobile\s+phone[:\s*]+\*\*(\d{3}-\d{3}-\d{4})\*\*",
                @"Mobile\s*phone[:\s]+(\d{3}-\d{3}-\d{4})",
                @"Mobile[:\s*]+\*\*(\d{3}-\d{3}-\d{4})\*\*",
                @"Mobile[:\s]+(\d{3}-\d{3}-\d{4})",
                @"(?:Mother'?s?\s*)?(?:Phone|Tel|Telephone)[:\s]+([\d\-\(\)\s]+)",
                @"Contact[:\s]+([\d\-\(\)\s]{10,})"
            };

            string phone = ExtractFirstMatch(text, patterns);

            if (!string.IsNullOrEmpty(phone))
            {
                phone = phone.Trim().Replace("**", "");
                if (phone.Equals("None", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine("DEBUG: Rejecting phone 'None'");
                    return string.Empty;
                }
                Debug.WriteLine($"DEBUG: Mother phone extracted = '{phone}'");
            }
            else
            {
                Debug.WriteLine("DEBUG: No mother phone found");
            }

            return phone;
        }

        /// <summary>
        /// Scans the full text for known risk factor keywords. See
        /// <see cref="PdfParser.ExtractRiskFactors"/> for detailed documentation.
        /// </summary>
        private static void ExtractRiskFactors(string text, Dictionary<string, string> riskFactors)
        {
            Debug.WriteLine("=== EXTRACTING RISK FACTORS ===");

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
            int foundCount = 0;

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
                        foundCount++;
                        Debug.WriteLine($"DEBUG: Risk '{riskFactor.Key}' = '{value}' (keyword: '{keyword}')");
                        break;
                    }
                }

                if (!found)
                    riskFactors[riskFactor.Key] = "Unknown";
            }

            Debug.WriteLine($"=== FOUND {foundCount} RISK FACTORS ===");
        }

        /// <summary>
        /// Looks at the ±50 character window around a keyword match to determine
        /// whether the risk factor is affirmed, denied, or indeterminate.
        /// </summary>
        private static string DetermineRiskValue(string text, string keyword)
        {
            int keywordIndex = text.ToLower().IndexOf(keyword);
            if (keywordIndex == -1) return "Unknown";

            int start = Math.Max(0, keywordIndex - 50);
            int length = Math.Min(100, text.Length - start);
            string context = text.Substring(start, length).ToLower();

            if (Regex.IsMatch(context, @"\b(yes|positive|present|confirmed|\+|✓|check|☑)\b"))
                return "Yes";

            if (Regex.IsMatch(context, @"\b(no|negative|absent|denied|none|-|✗|☐)\b"))
                return "No";

            return "Unknown";
        }

        private static string ExtractFirstMatch(string text, string[] patterns)
        {
            foreach (string pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
                if (match.Success && match.Groups.Count > 1)
                {
                    string result = match.Groups[1].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return result;
                    }
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
                return ExtractTextFromDocx(filePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error extracting DOCX text for preview: {ex.Message}");
                return $"[Error extracting text: {ex.Message}]";
            }
        }
    }
}