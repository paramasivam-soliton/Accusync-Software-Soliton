// --------------------------------------------------------------------------------
// <copyright file="CompletedScreening.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace AccuSync.Presentation.Models
{
    /// <summary>
    /// A screening that has been completed, shown on the completed-screenings list.
    /// </summary>
    public class CompletedScreening
    {
        /// <summary>The patient's identifier.</summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>The patient's full name.</summary>
        public string PatientName { get; set; } = string.Empty;

        /// <summary>The patient's date of birth.</summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>The date of the patient's most recent screening.</summary>
        public DateTime LastScreenDate { get; set; }

        /// <summary>When the screening was completed.</summary>
        public DateTime CompletedTime { get; set; }

        /// <summary>The screening result.</summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>Display text for how long the screening took.</summary>
        public string Duration { get; set; } = string.Empty;

        /// <summary>Badges shown on the screening's card.</summary>
        public List<ScreeningBadge> Badges { get; set; } = new List<ScreeningBadge>();
    }
}
