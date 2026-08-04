// --------------------------------------------------------------------------------
// <copyright file="ABRProtocolEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    public class ABRProtocolEntry
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "Basic";
        public string Status { get; set; } = "Active";

        public int CategoryIndex { get; set; }
        public int StatusIndex { get; set; }
        public int ABRLevelIndex { get; set; } = 1;       // 35 dB
        public int NotchFilterIndex { get; set; }          // 50 Hz
        public int StimulusDuringPauseIndex { get; set; }  // Yes
    }
}
