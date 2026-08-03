// --------------------------------------------------------------------------------
// <copyright file="OcrParser.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Tesseract;

namespace AccuSync.DataParser.Services
{
    /// <summary>
    /// Extracts patient data from scanned image facesheets using Tesseract OCR.
    /// Supports the same hospital formats as <see cref="PdfParser"/>.
    /// </summary>
    // TODO: The extraction methods (ExtractPatientName, ExtractMotherName,
    //       ExtractWeight, ExtractRiskFactors, etc.) are near-identical copies
    //       of PdfParser. Extract a shared FacesheetFieldExtractor class that
    //       both parsers delegate to after obtaining their text.
    // TODO: Same PII-in-debug-output concern as PdfParser. Patient names, DOBs,
    //       and phone numbers should not be in debug output in production.
    public class OcrParser
    {
        private static readonly string TessdataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

        /// <summary>
        /// Entry point for OCR-based facesheet import. Validates that Tesseract
        /// language data is present before attempting extraction.
        /// </summary>
        public static PatientData ParseFacesheetOCR(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            if (!Directory.Exists(TessdataPath))
                throw new DirectoryNotFoundException(
                    $"Tessdata folder not found at: {TessdataPath}. " +
                    "Please create a 'tessdata' folder and add eng.traineddata file.");

            string engDataPath = Path.Combine(TessdataPath, "eng.traineddata");
            if (!File.Exists(engDataPath))
                throw new FileNotFoundException(
                    $"English language data not found at: {engDataPath}. " +
                    "Please download eng.traineddata from https://github.com/tesseract-ocr/tessdata");

            try
            {
                string ocrText = ExtractTextFromImage(filePath);
                return MapTextToPatientData(ocrText);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing OCR: {ex.Message}", ex);
            }
        }

        private static string ExtractTextFromImage(string imagePath)
        {
            using (var engine = new TesseractEngine(TessdataPath, "eng", EngineMode.Default))
            {
                // Whitelist restricts Tesseract to expected characters, reducing
                // garbage output. Includes # for "Medical Record #" fields.
                engine.SetVariable("tessedit_char_whitelist",
                    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,/-:()#*");

                using (var img = Pix.LoadFromFile(imagePath))
                using (var page = engine.Process(img))
                {
                    string text = page.GetText();
                    float confidence = page.GetMeanConfidence();
                    Debug.WriteLine($"OCR Confidence: {confidence:P}");
                    return text;
                }
            }
        }

        private static PatientData MapTextToPatientData(string ocrText)
        {
            var patient = new PatientData
            {
                RiskFactors = new Dictionary<string, string>()
            };

            Debug.WriteLine("=== STARTING OCR FIELD EXTRACTION ===");

            patient.PatientId = ExtractPatientId(ocrText);

            var nameData = ExtractPatientName(ocrText);
            patient.FirstName = nameData.firstName;
            patient.LastName = nameData.lastName;

            patient.DateOfBirth = ExtractDateOfBirth(ocrText);
            patient.Gender = ExtractGender(ocrText);
            patient.Weight = ExtractWeight(ocrText);
            patient.Height = ExtractHeight(ocrText);

            var motherNameData = ExtractMotherName(ocrText);
            patient.MotherFirstName = motherNameData.firstName;
            patient.MotherLastName = motherNameData.lastName;

            patient.MotherPhone = ExtractMotherPhone(ocrText);

            Debug.WriteLine("=== OCR EXTRACTION RESULTS ===");
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
            Debug.WriteLine("=== END OCR EXTRACTION ===");

            ExtractRiskFactors(ocrText, patient.RiskFactors);

            return patient;
        }

        #region Field Extraction Methods
        // All extraction methods below are duplicated from PdfParser.
        // See the class-level TODO about extracting a shared helper.

        private static string ExtractPatientId(string text)
        {
            var patterns = new[]
            {
                @"Medical\s+Record\s+#?\s*([A-Z]\d+)",
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

        // Same pattern-priority approach as PdfParser. See PdfParser.ExtractPatientName
        // for detailed format documentation.
        private static (string firstName, string lastName) ExtractPatientName(string text)
        {
            string topSection = text.Length > 400 ? text.Substring(0, 400) : text;

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

            // Format 2: "Garcia-Mendez, Girl A Sophia P"
            var complexMatch = Regex.Match(topSection,
                @"([A-Z][a-z]+(?:-[A-Z][a-z]+)?),\s+(?:Girl|Boy)\s+[A-Z]\s+",
                RegexOptions.Multiline);
            if (complexMatch.Success)
            {
                string lastName = complexMatch.Groups[1].Value.Trim();
                Debug.WriteLine($"DEBUG: Complex newborn format - Last: '{lastName}', First: 'Baby'");
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
                Debug.WriteLine($"DEBUG: Simple comma format - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 4: Standard labeled fields
            string firstName4 = ExtractFirstMatch(text, new[] {
                @"First\s*Name[:\s]+([A-Z][a-z]+)",
                @"Given\s*Name[:\s]+([A-Z][a-z]+)",
                @"Patient\s+First\s*Name[:\s]+([A-Z][a-z]+)",
                @"Patient\s*Name[:\s]+([A-Z][a-z]+)"
            });

            string lastName4 = ExtractFirstMatch(text, new[] {
                @"Last\s*Name[:\s]+([A-Z][a-z]+)",
                @"Surname[:\s]+([A-Z][a-z]+)",
                @"Family\s*Name[:\s]+([A-Z][a-z]+)",
                @"Patient\s+Last\s*Name[:\s]+([A-Z][a-z]+)"
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
        /// OCR is especially prone to concatenated-digit artifacts, so the
        /// length-6 guard is more important here than in the PDF parser.
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
                    Debug.WriteLine($"DEBUG: Rejecting weight '{weight}' - too long (likely OCR artifact)");
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
            var capsMiddleMatch = Regex.Match(text,
                @"([A-Z]{2,}),\s+([A-Z]+)\s+[A-Z]\s+MOTHER",
                RegexOptions.Multiline);
            if (capsMiddleMatch.Success)
            {
                string lastName = capsMiddleMatch.Groups[1].Value.Trim();
                string firstName = capsMiddleMatch.Groups[2].Value.Trim();
                lastName = char.ToUpper(lastName[0]) + lastName.Substring(1).ToLower();
                firstName = char.ToUpper(firstName[0]) + firstName.Substring(1).ToLower();
                Debug.WriteLine($"DEBUG: Mother name (caps middle) - Last: '{lastName}', First: '{firstName}'");
                return (firstName, lastName);
            }

            // Format 3: "Sophia P Garcia-Mendez (Mother)"
            var parenMatch = Regex.Match(text,
                @"([A-Z][a-z]+)\s+[A-Z]\s+([A-Z][a-z]+(?:-[A-Z][a-z]+)?)\s+\(Mother\)",
                RegexOptions.Multiline);
            if (parenMatch.Success)
            {
                string firstName = parenMatch.Groups[1].Value.Trim();
                string lastName = parenMatch.Groups[2].Value.Trim();
                Debug.WriteLine($"DEBUG: Mother name (parentheses) - Last: '{lastName}', First: '{firstName}'");
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
                Debug.WriteLine($"DEBUG: Mother name (guarantor) - Last: '{lastName}', First: '{firstName}'");
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
            string fn = ExtractFirstMatch(text, new[] {
                @"Mother'?s?\s*First\s*Name[:\s]+([A-Z][a-z]+)",
                @"Mother[:\s]+([A-Z][a-z]+)\s+[A-Z][a-z]+"
            });
            string ln = ExtractFirstMatch(text, new[] {
                @"Mother'?s?\s*Last\s*Name[:\s]+([A-Z][a-z]+)",
                @"Mother[:\s]+[A-Z][a-z]+\s+([A-Z][a-z]+)"
            });

            if (!string.IsNullOrEmpty(fn) || !string.IsNullOrEmpty(ln))
            {
                Debug.WriteLine($"DEBUG: Mother name (labeled) - Last: '{ln}', First: '{fn}'");
                return (fn, ln);
            }

            Debug.WriteLine("DEBUG: No mother name pattern matched");
            return (string.Empty, string.Empty);
        }

        private static string ExtractMotherPhone(string text)
        {
            var patterns = new[]
            {
                @"\(Mother\)\s*-\s*(\d{3}-\d{3}-\d{4})",
                @"Mother.*?(\d{3}-\d{3}-\d{4})",
                @"(?:Mother|Emergency\s+Contact).*?(\d{3}-\d{3}-\d{4})",
                @"MOTHER\s+YES.*?(\d{3}-\d{3}-\d{4})",
                @"Mobile\s+phone[:\s]+(\d{3}-\d{3}-\d{4})",
                @"Mobile[:\s]+(\d{3}-\d{3}-\d{4})",
                @"(?:Mother'?s?\s*)?(?:Phone|Tel|Telephone)[:\s]+([\d\-\(\)\s]+)",
                @"Contact[:\s]+([\d\-\(\)\s]{10,})"
            };

            string phone = ExtractFirstMatch(text, patterns);

            if (!string.IsNullOrEmpty(phone))
            {
                phone = phone.Trim();
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

        private static void ExtractRiskFactors(string text, Dictionary<string, string> riskFactors)
        {
            Debug.WriteLine("=== EXTRACTING RISK FACTORS (OCR) ===");

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
            int idx = text.ToLower().IndexOf(keyword);
            if (idx == -1) return "Unknown";

            int start = Math.Max(0, idx - 50);
            int length = Math.Min(100, text.Length - start);
            string context = text.Substring(start, length).ToLower();

            if (Regex.IsMatch(context, @"\b(yes|positive|present|confirmed)\b|(\+|✓|☑)"))
                return "Yes";

            if (Regex.IsMatch(context, @"\b(no|negative|absent|denied|none)\b|(\-|✗|☐)"))
                return "No";

            return "Unknown";
        }

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

        #region Public Preview/Confidence Methods

        /// <summary>
        /// Returns the overall OCR confidence score for display in the import review UI.
        /// </summary>
        public static Dictionary<string, float> GetFieldConfidences(string filePath)
        {
            var confidences = new Dictionary<string, float>();
            try
            {
                using (var engine = new TesseractEngine(TessdataPath, "eng", EngineMode.Default))
                using (var img = Pix.LoadFromFile(filePath))
                using (var page = engine.Process(img))
                {
                    confidences["Overall"] = page.GetMeanConfidence();
                }
            }
            catch
            {
                confidences["Overall"] = 0f;
            }
            return confidences;
        }

        /// <summary>
        /// Returns extracted text for the facesheet review step so users can
        /// verify what the OCR engine produced before committing the import.
        /// </summary>
        public static string GetExtractedText(string imagePath)
        {
            if (!File.Exists(imagePath))
                return string.Empty;

            try
            {
                if (!Directory.Exists(TessdataPath))
                    return "[Tessdata folder not found. Please ensure 'tessdata' folder exists in application directory.]";

                string trainedDataPath = Path.Combine(TessdataPath, "eng.traineddata");
                if (!File.Exists(trainedDataPath))
                    return "[English trained data file not found. Please download 'eng.traineddata' from https://github.com/tesseract-ocr/tessdata]";

                using (var engine = new TesseractEngine(TessdataPath, "eng", EngineMode.Default))
                using (var img = Pix.LoadFromFile(imagePath))
                using (var page = engine.Process(img))
                {
                    string text = page.GetText();
                    Debug.WriteLine($"OCR: Extracted {text.Length} characters for preview");
                    return text;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OCR: Error extracting text for preview - {ex.Message}");
                return $"[Error extracting text: {ex.Message}]";
            }
        }

        #endregion
    }
}