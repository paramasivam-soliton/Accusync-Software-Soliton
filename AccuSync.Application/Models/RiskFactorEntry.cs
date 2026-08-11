// --------------------------------------------------------------------------------
// <copyright file="RiskFactorEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// A configurable risk factor entry.
    /// </summary>
    public class RiskFactorEntry
    {
        /// <summary>The risk factor's display name.</summary>
        public string Name { get; set; } = "";
        /// <summary>A short description of the risk factor.</summary>
        public string Description { get; set; } = "";
        /// <summary>Whether the risk factor is active and available for selection.</summary>
        public bool Active { get; set; } = true;
        /// <summary>Whether the risk factor is currently referenced by an existing record.</summary>
        public bool InUse { get; set; } = false;
        /// <summary>Selected index into the active/inactive ComboBox.</summary>
        public int ActiveIndex { get; set; } = 0;

        // Translation data keyed by language ComboBox index
        /// <summary>Translated names keyed by language ComboBox index.</summary>
        public Dictionary<int, string> TranslationNames { get; set; } = new();
        /// <summary>Translated descriptions keyed by language ComboBox index.</summary>
        public Dictionary<int, string> TranslationDescriptions { get; set; } = new();
    }
}
