// --------------------------------------------------------------------------------
// <copyright file="CompletedScreening.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace AccuSync.Application.Models
{
    public class CompletedScreening
    {
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime LastScreenDate { get; set; }
        public DateTime CompletedTime { get; set; }
        public string Result { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public List<ScreeningBadge> Badges { get; set; } = new List<ScreeningBadge>();
    }
}
