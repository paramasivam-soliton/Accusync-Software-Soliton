// --------------------------------------------------------------------------------
// <copyright file="DPOAEProtocolEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    /// <summary>
    /// A configurable DPOAE (Distortion Product Otoacoustic Emissions) test protocol entry.
    /// </summary>
    public class DPOAEProtocolEntry
    {
        /// <summary>The protocol's display name.</summary>
        public string Name { get; set; } = "";
        /// <summary>A short description of the protocol.</summary>
        public string Description { get; set; } = "";
        /// <summary>The protocol category (e.g., "Basic").</summary>
        public string Category { get; set; } = "Basic";
        /// <summary>Whether the protocol is "Active" or inactive.</summary>
        public string Status { get; set; } = "Active";

        /// <summary>Selected index into the category ComboBox.</summary>
        public int CategoryIndex { get; set; }
        /// <summary>Selected index into the status ComboBox.</summary>
        public int StatusIndex { get; set; }
        /// <summary>Selected index into the L2 stimulus level ComboBox.</summary>
        public int L2Index { get; set; } = 1;              // 55 dB
        /// <summary>Selected index into the L1 stimulus level ComboBox.</summary>
        public int L1Index { get; set; }                    // Auto
        /// <summary>Which of the 1000-6000 Hz test frequencies are enabled.</summary>
        public bool[] Frequencies { get; set; } = { false, true, true, true, true, true, true }; // 1000=off, 1500-6000=on
        /// <summary>Selected index into the retest-frequency ComboBox.</summary>
        public int RetestFreqIndex { get; set; } = 1;      // No
        /// <summary>Selected index into the pass-criterion ComboBox.</summary>
        public int PassCriterionIndex { get; set; } = 3;   // 4 frequencies of 6
        /// <summary>Selected index into the auto-stop ComboBox.</summary>
        public int AutoStopIndex { get; set; }              // Yes
        /// <summary>Selected index into the minimum-level ComboBox.</summary>
        public int MinLevelIndex { get; set; } = 2;         // -5 dB
        /// <summary>Selected index into the SNR (signal-to-noise ratio) ComboBox.</summary>
        public int SNRIndex { get; set; } = 1;              // 9 dB
    }
}
