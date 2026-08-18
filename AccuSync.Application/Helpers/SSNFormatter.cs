// --------------------------------------------------------------------------------
// <copyright file="SSNFormatter.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace AccuSync.Application.Helpers
{
    /// <summary>
    /// Formats US Social Security Numbers as <c>XXX-XX-XXXX</c>.
    /// Handles partial input for live formatting as the user types.
    /// </summary>
    // TODO: SSNs are sensitive PII. Ensure values are masked in the UI (e.g., •••-••-1234)
    //       and never written to logs or debug output.
    public static class SSNFormatter
    {
        /// <summary>
        /// Strips non-digit characters from <paramref name="ssn"/> and inserts
        /// dashes progressively. Safe to call on partial input.
        /// </summary>
        public static string Format(string ssn)
        {
            if (string.IsNullOrWhiteSpace(ssn))
                return string.Empty;

            string cleaned = Regex.Replace(ssn, @"\D", "");

            if (cleaned.Length <= 3) return cleaned;
            if (cleaned.Length <= 5)
                return $"{cleaned.Substring(0, 3)}-{cleaned.Substring(3)}";
            return $"{cleaned.Substring(0, 3)}-{cleaned.Substring(3, 2)}-{cleaned.Substring(5, Math.Min(4, cleaned.Length - 5))}";
        }

        /// <summary>
        /// Strips all non-digit characters from <paramref name="input"/>.
        /// </summary>
        public static string ExtractDigits(string input)
        {
            return Regex.Replace(input ?? "", @"\D", "");
        }
    }
}