// --------------------------------------------------------------------------------
// <copyright file="AssignedPatientInfo.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Display model for a patient assigned to a screener, shown with completion result styling.
    /// </summary>
    public class AssignedPatientInfo
    {
        /// <summary>The patient's medical record number.</summary>
        public string MRN { get; set; }
        /// <summary>The patient's first name.</summary>
        public string FirstName { get; set; }
        /// <summary>The patient's last name.</summary>
        public string LastName { get; set; }
        /// <summary>The patient's date of birth.</summary>
        public DateTime DOB { get; set; }
        /// <summary>The screening result text ("Pass", "Refer", "Incomplete").</summary>
        public string ResultText { get; set; }  // "Pass", "Refer", "Incomplete"
        /// <summary>Background color for the result badge.</summary>
        public string ResultBackground { get; set; }
        /// <summary>Foreground color for the result badge.</summary>
        public string ResultForeground { get; set; }
        /// <summary>Left border accent color.</summary>
        public string AccentColor { get; set; }  // Left border color, hex
        /// <summary>Display text for when the screening was completed.</summary>
        public string CompletionTime { get; set; }
    }
}
