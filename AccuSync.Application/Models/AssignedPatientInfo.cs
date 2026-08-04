// --------------------------------------------------------------------------------
// <copyright file="AssignedPatientInfo.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Application.Models
{
    public class AssignedPatientInfo
    {
        public string MRN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public string ResultText { get; set; }  // "Pass", "Refer", "Incomplete"
        public string ResultBackground { get; set; }
        public string ResultForeground { get; set; }
        public string AccentColor { get; set; }  // Left border color, hex
        public string CompletionTime { get; set; }
    }
}
