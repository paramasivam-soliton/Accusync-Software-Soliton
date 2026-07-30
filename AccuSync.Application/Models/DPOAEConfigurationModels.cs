// --------------------------------------------------------------------------------
// <copyright file="DPOAEConfigurationModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Windows.Media;

namespace AccuSync.Application.Models
{
    public class DPOAEProtocolEntry
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "Basic";
        public string Status { get; set; } = "Active";

        public int CategoryIndex { get; set; }
        public int StatusIndex { get; set; }
        public int L2Index { get; set; } = 1;              // 55 dB
        public int L1Index { get; set; }                    // Auto
        public bool[] Frequencies { get; set; } = { false, true, true, true, true, true, true }; // 1000=off, 1500-6000=on
        public int RetestFreqIndex { get; set; } = 1;      // No
        public int PassCriterionIndex { get; set; } = 3;   // 4 frequencies of 6
        public int AutoStopIndex { get; set; }              // Yes
        public int MinLevelIndex { get; set; } = 2;         // -5 dB
        public int SNRIndex { get; set; } = 1;              // 9 dB

        // NOTE: Creates a new Brush per access — same as ABRProtocolEntry.
        public SolidColorBrush StatusColor =>
            Status == "Active"
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065F46"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991B1B"));
    }
}
