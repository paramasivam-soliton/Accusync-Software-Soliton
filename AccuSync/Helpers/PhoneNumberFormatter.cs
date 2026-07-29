// --------------------------------------------------------------------------------
// <copyright file="PhoneNumberFormatter.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace AccuSync.Helpers
{
    /// <summary>
    /// Formats phone number digits into country-specific display patterns.
    /// Designed for live formatting as the user types — each method handles
    /// partial input gracefully so intermediate states still look reasonable.
    /// </summary>
    // NOTE: Every FormatXxx method follows the same structure (progressively
    //       format as more digits arrive). Consider a table-driven approach
    //       using format pattern strings instead of one method per country.
    public static class PhoneNumberFormatter
    {
        /// <summary>
        /// Strips non-digit characters from <paramref name="phoneNumber"/> and
        /// formats the result according to the given <paramref name="dialCode"/>.
        /// Returns raw digits for unsupported dial codes.
        /// </summary>
        public static string Format(string phoneNumber, string dialCode)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            string cleaned = Regex.Replace(phoneNumber, @"\D", "");

            return dialCode switch
            {
                "+1" => FormatNorthAmerica(cleaned),   // (XXX) XXX-XXXX
                "+52" => FormatMexico(cleaned),         // XX XXXX XXXX
                "+44" => FormatUK(cleaned),             // XXXX XXX XXXX
                "+33" => FormatFrance(cleaned),         // X XX XX XX XX
                "+49" => FormatGermany(cleaned),        // XXX XXXXXXX
                "+81" => FormatJapan(cleaned),          // XX-XXXX-XXXX
                "+86" => FormatChina(cleaned),          // XXX XXXX XXXX
                "+91" => FormatIndia(cleaned),          // XXXXX XXXXX
                "+61" => FormatAustralia(cleaned),      // XXXX XXX XXX
                "+55" => FormatBrazil(cleaned),         // XX XXXXX-XXXX
                "+39" => FormatItaly(cleaned),          // XXX XXX XXXX
                "+34" => FormatSpain(cleaned),          // XXX XX XX XX
                "+7" => FormatRussia(cleaned),         // XXX XXX-XX-XX
                "+82" => FormatSouthKorea(cleaned),     // XX-XXXX-XXXX
                _ => cleaned
            };
        }

        private static string FormatNorthAmerica(string cleaned)
        {
            if (cleaned.Length <= 3) return cleaned;
            if (cleaned.Length <= 6)
                return $"({cleaned.Substring(0, 3)}) {cleaned.Substring(3)}";
            return $"({cleaned.Substring(0, 3)}) {cleaned.Substring(3, 3)}-{cleaned.Substring(6, Math.Min(4, cleaned.Length - 6))}";
        }

        private static string FormatMexico(string cleaned)
        {
            if (cleaned.Length <= 2) return cleaned;
            if (cleaned.Length <= 6)
                return $"{cleaned.Substring(0, 2)} {cleaned.Substring(2)}";
            return $"{cleaned.Substring(0, 2)} {cleaned.Substring(2, 4)} {cleaned.Substring(6, Math.Min(4, cleaned.Length - 6))}";
        }

        private static string FormatUK(string cleaned)
        {
            if (cleaned.Length <= 4) return cleaned;
            if (cleaned.Length <= 7)
                return $"{cleaned.Substring(0, 4)} {cleaned.Substring(4)}";
            return $"{cleaned.Substring(0, 4)} {cleaned.Substring(4, 3)} {cleaned.Substring(7, Math.Min(4, cleaned.Length - 7))}";
        }

        private static string FormatFrance(string cleaned)
        {
            if (cleaned.Length <= 1) return cleaned;
            string result = cleaned.Substring(0, 1);
            for (int i = 1; i < Math.Min(cleaned.Length, 9); i += 2)
            {
                result += " " + cleaned.Substring(i, Math.Min(2, cleaned.Length - i));
            }
            return result;
        }

        private static string FormatGermany(string cleaned)
        {
            if (cleaned.Length <= 3) return cleaned;
            return $"{cleaned.Substring(0, 3)} {cleaned.Substring(3, Math.Min(7, cleaned.Length - 3))}";
        }

        private static string FormatJapan(string cleaned)
        {
            if (cleaned.Length <= 2) return cleaned;
            if (cleaned.Length <= 6)
                return $"{cleaned.Substring(0, 2)}-{cleaned.Substring(2)}";
            return $"{cleaned.Substring(0, 2)}-{cleaned.Substring(2, 4)}-{cleaned.Substring(6, Math.Min(4, cleaned.Length - 6))}";
        }

        private static string FormatChina(string cleaned)
        {
            if (cleaned.Length <= 3) return cleaned;
            if (cleaned.Length <= 7)
                return $"{cleaned.Substring(0, 3)} {cleaned.Substring(3)}";
            return $"{cleaned.Substring(0, 3)} {cleaned.Substring(3, 4)} {cleaned.Substring(7, Math.Min(4, cleaned.Length - 7))}";
        }

        private static string FormatIndia(string cleaned)
        {
            if (cleaned.Length <= 5) return cleaned;
            return $"{cleaned.Substring(0, 5)} {cleaned.Substring(5, Math.Min(5, cleaned.Length - 5))}";
        }

        private static string FormatAustralia(string cleaned)
        {
            if (cleaned.Length <= 4) return cleaned;
            if (cleaned.Length <= 7)
                return $"{cleaned.Substring(0, 4)} {cleaned.Substring(4)}";
            return $"{cleaned.Substring(0, 4)} {cleaned.Substring(4, 3)} {cleaned.Substring(7, Math.Min(3, cleaned.Length - 7))}";
        }

        private static string FormatBrazil(string cleaned)
        {
            if (cleaned.Length <= 2) return cleaned;
            if (cleaned.Length <= 7)
                return $"{cleaned.Substring(0, 2)} {cleaned.Substring(2)}";
            return $"{cleaned.Substring(0, 2)} {cleaned.Substring(2, 5)}-{cleaned.Substring(7, Math.Min(4, cleaned.Length - 7))}";
        }

        private static string FormatItaly(string cleaned)
        {
            if (cleaned.Length <= 3) return cleaned;
            if (cleaned.Length <= 6)
                return $"{cleaned.Substring(0, 3)} {cleaned.Substring(3)}";
            return $"{cleaned.Substring(0, 3)} {cleaned.Substring(3, 3)} {cleaned.Substring(6, Math.Min(4, cleaned.Length - 6))}";
        }

        private static string FormatSpain(string cleaned)
        {
            if (cleaned.Length <= 3) return cleaned;
            if (cleaned.Length <= 5)
                return $"{cleaned.Substring(0, 3)} {cleaned.Substring(3)}";
            if (cleaned.Length <= 7)
                return $"{cleaned.Substring(0, 3)} {cleaned.Substring(3, 2)} {cleaned.Substring(5)}";
            return $"{cleaned.Substring(0, 3)} {cleaned.Substring(3, 2)} {cleaned.Substring(5, 2)} {cleaned.Substring(7, Math.Min(2, cleaned.Length - 7))}";
        }

        private static string FormatRussia(string cleaned)
        {
            if (cleaned.Length <= 3) return cleaned;
            if (cleaned.Length <= 6)
                return $"{cleaned.Substring(0, 3)} {cleaned.Substring(3)}";
            if (cleaned.Length <= 8)
                return $"{cleaned.Substring(0, 3)} {cleaned.Substring(3, 3)}-{cleaned.Substring(6)}";
            return $"{cleaned.Substring(0, 3)} {cleaned.Substring(3, 3)}-{cleaned.Substring(6, 2)}-{cleaned.Substring(8, Math.Min(2, cleaned.Length - 8))}";
        }

        private static string FormatSouthKorea(string cleaned)
        {
            if (cleaned.Length <= 2) return cleaned;
            if (cleaned.Length <= 6)
                return $"{cleaned.Substring(0, 2)}-{cleaned.Substring(2)}";
            return $"{cleaned.Substring(0, 2)}-{cleaned.Substring(2, 4)}-{cleaned.Substring(6, Math.Min(4, cleaned.Length - 6))}";
        }

        /// <summary>
        /// Strips all non-digit characters from <paramref name="input"/>.
        /// </summary>
        public static string ExtractDigits(string input)
        {
            return Regex.Replace(input ?? "", @"\D", "");
        }

        /// <summary>
        /// Returns a placeholder example for the given <paramref name="dialCode"/>
        /// (e.g., <c>"(555) 123-4567"</c> for <c>"+1"</c>).
        /// Used as watermark text in the phone number input field.
        /// </summary>
        public static string GetFormatExample(string dialCode)
        {
            return dialCode switch
            {
                "+1" => "(555) 123-4567",
                "+52" => "55 1234 5678",
                "+44" => "0207 123 4567",
                "+33" => "1 23 45 67 89",
                "+49" => "030 12345678",
                "+81" => "03-1234-5678",
                "+86" => "138 0013 8000",
                "+91" => "98765 43210",
                "+61" => "0412 345 678",
                "+55" => "11 98765-4321",
                "+39" => "06 1234 5678",
                "+34" => "912 34 56 78",
                "+7" => "495 123-45-67",
                "+82" => "02-1234-5678",
                _ => "Enter phone number"
            };
        }

        /// <summary>
        /// Returns the maximum number of digits allowed for the given <paramref name="dialCode"/>.
        /// Defaults to 15 (the ITU-T E.164 maximum) for unsupported codes.
        /// </summary>
        public static int GetMaxDigits(string dialCode)
        {
            return dialCode switch
            {
                "+1" or "+52" or "+44" or "+49" or "+81" or "+91" or "+39" or "+34" or "+82" => 10,
                "+86" or "+55" or "+7" => 11,
                "+33" or "+61" => 9,
                _ => 15
            };
        }
    }
}