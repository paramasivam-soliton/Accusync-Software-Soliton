// --------------------------------------------------------------------------------
// <copyright file="ABRConfigurationModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Windows.Media;

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

        // NOTE: Creates a new Brush on every access. Fine for a small list,
        // but would need caching if protocol count grows.
        public SolidColorBrush StatusColor =>
            Status == "Active"
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065F46"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991B1B"));
    }
}
