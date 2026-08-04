// --------------------------------------------------------------------------------
// <copyright file="AssignedPatient.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace AccuSync.Application.Models
{
    public class AssignedPatient
    {
        public string MRN { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime AssignedTime { get; set; }
        public DateTime? LastScreenDate { get; set; }
        public bool IsScreened { get; set; }
        public string ScreeningResult { get; set; } = string.Empty;
        public string AccentColor { get; set; }
        public List<Badge> Badges { get; set; } = new List<Badge>();

        public string DateStatusLabel
        {
            get
            {
                if (IsScreened && LastScreenDate.HasValue)
                    return $"Last screen: {LastScreenDate.Value:d}";
                else
                    return $"Assigned: {AssignedTime:t}";
            }
        }
    }
}
