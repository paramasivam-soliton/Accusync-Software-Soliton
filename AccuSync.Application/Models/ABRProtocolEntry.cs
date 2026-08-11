// --------------------------------------------------------------------------------
// <copyright file="ABRProtocolEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Windows.Media;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// A configurable ABR (Auditory Brainstem Response) test protocol entry.
    /// </summary>
    public class ABRProtocolEntry
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
        /// <summary>Selected index into the ABR level ComboBox.</summary>
        public int ABRLevelIndex { get; set; } = 1;       // 35 dB
        /// <summary>Selected index into the notch filter ComboBox.</summary>
        public int NotchFilterIndex { get; set; }          // 50 Hz
        /// <summary>Selected index into the stimulus-during-pause ComboBox.</summary>
        public int StimulusDuringPauseIndex { get; set; }  // Yes

        // NOTE: Creates a new Brush on every access. Fine for a small list,
        // but would need caching if protocol count grows.
        /// <summary>Brush reflecting <see cref="Status"/> for the status badge.</summary>
        public SolidColorBrush StatusColor =>
            Status == "Active"
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065F46"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991B1B"));
    }
}
