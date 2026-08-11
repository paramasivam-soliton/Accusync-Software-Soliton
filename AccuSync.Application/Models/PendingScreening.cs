// --------------------------------------------------------------------------------
// <copyright file="PendingScreening.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace AccuSync.Application.Models
{
    // Model class for pending screenings
    /// <summary>
    /// A patient whose screening has not yet been performed, shown on the pending-screenings list.
    /// </summary>
    public class PendingScreening
    {
        /// <summary>The patient's identifier.</summary>
        public string PatientId { get; set; } = string.Empty;
        /// <summary>The patient's full name.</summary>
        public string PatientName { get; set; } = string.Empty;
        /// <summary>The patient's date of birth.</summary>
        public DateTime DateOfBirth { get; set; }
        /// <summary>When the patient was assigned for screening.</summary>
        public DateTime AssignedTime { get; set; }
        /// <summary>Display text for how long the patient has been waiting.</summary>
        public string WaitingTime { get; set; } = string.Empty;
        /// <summary>The patient's risk factors.</summary>
        public List<PendingRiskFactor> RiskFactors { get; set; } = new List<PendingRiskFactor>();
    }
}
