// --------------------------------------------------------------------------------
// <copyright file="NameFormatter.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Globalization;

namespace AccuSync.Helpers
{
    /// <summary>
    /// Formatting helpers for patient and user display names.
    /// </summary>
    public static class NameFormatter
    {
        /// <summary>
        /// Title-cases each word in <paramref name="name"/> using the current culture.
        /// Returns <see cref="string.Empty"/> for null or whitespace input.
        /// </summary>
        /// <remarks>
        /// This lowercases each word before title-casing, so "O'BRIEN" becomes "O'brien"
        /// and "McDONALD" becomes "Mcdonald". Hyphenated surnames like "Smith-Jones" are
        /// treated as a single word and only the first letter is capitalized.
        /// </remarks>
        // TODO: Handle edge cases — Irish/Scottish prefixes (Mc, O'), hyphenated
        //       surnames, and suffixes like "Jr." or "III".
        public static string Capitalize(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

            string[] words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = textInfo.ToTitleCase(words[i].ToLower());
            }

            return string.Join(" ", words);
        }
    }
}