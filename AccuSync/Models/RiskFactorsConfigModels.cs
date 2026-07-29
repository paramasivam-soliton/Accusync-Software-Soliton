// --------------------------------------------------------------------------------
// <copyright file="RiskFactorsConfigModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AccuSync.Models
{
    public class RiskFactorEntry
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool Active { get; set; } = true;
        public bool InUse { get; set; } = false;
        public int ActiveIndex { get; set; } = 0;

        // Translation data keyed by language ComboBox index
        public Dictionary<int, string> TranslationNames { get; set; } = new();
        public Dictionary<int, string> TranslationDescriptions { get; set; } = new();
    }
}
