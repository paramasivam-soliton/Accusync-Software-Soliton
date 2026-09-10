// --------------------------------------------------------------------------------
// <copyright file="PatientInfo.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace AccuSync.Presentation.Models
{
    /// <summary>
    /// Display model for a patient with risk factors, used on patient list and detail views.
    /// </summary>
    public class PatientInfo
    {
        /// <summary>The patient's medical record number.</summary>
        public string MRN { get; set; }

        /// <summary>The patient's first name.</summary>
        public string FirstName { get; set; }

        /// <summary>The patient's last name.</summary>
        public string LastName { get; set; }

        /// <summary>The patient's date of birth.</summary>
        public DateTime DOB { get; set; }

        /// <summary>The date of the patient's most recent screening.</summary>
        public DateTime LastScreenDate { get; set; }

        /// <summary>Accent color used for the card's left border.</summary>
        public string AccentColor { get; set; }

        /// <summary>The patient's risk factors.</summary>
        public List<RiskFactor> RiskFactors { get; set; }
    }
}
