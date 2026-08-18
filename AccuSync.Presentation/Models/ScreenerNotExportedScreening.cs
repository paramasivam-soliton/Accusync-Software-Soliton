// --------------------------------------------------------------------------------
// <copyright file="ScreenerNotExportedScreening.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Presentation.Models
{
    // Model class for screener not exported screenings
    /// <summary>
    /// A completed screening, owned by a specific screener, that has not yet been exported.
    /// </summary>
    public class ScreenerNotExportedScreening
    {
        /// <summary>The patient's identifier.</summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>The patient's full name.</summary>
        public string PatientName { get; set; } = string.Empty;

        /// <summary>When the screening was completed.</summary>
        public DateTime CompletedTime { get; set; }

        /// <summary>The screening result.</summary>
        public string Result { get; set; } = string.Empty;
    }
}
