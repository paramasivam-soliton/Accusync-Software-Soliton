// --------------------------------------------------------------------------------
// <copyright file="AssignedPatient.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace AccuSync.Presentation.Models
{
    /// <summary>
    /// A patient assigned to a screener, shown on the assigned-patients worklist.
    /// </summary>
    public class AssignedPatient
    {
        /// <summary>The patient's medical record number.</summary>
        public string MRN { get; set; } = string.Empty;

        /// <summary>The patient's first name.</summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>The patient's last name.</summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>The patient's date of birth.</summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>When the patient was assigned to the screener.</summary>
        public DateTime AssignedTime { get; set; }

        /// <summary>When the patient was last screened, if screened.</summary>
        public DateTime? LastScreenDate { get; set; }

        /// <summary>Whether the patient has already been screened.</summary>
        public bool IsScreened { get; set; }

        /// <summary>The result of the patient's screening, if screened.</summary>
        public string ScreeningResult { get; set; } = string.Empty;

        /// <summary>Accent color used for the card's left border.</summary>
        public string AccentColor { get; set; }

        /// <summary>Badges shown on the patient's card.</summary>
        public List<Badge> Badges { get; set; } = new List<Badge>();

        /// <summary>Display text showing either the last screen date or the assigned time.</summary>
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
