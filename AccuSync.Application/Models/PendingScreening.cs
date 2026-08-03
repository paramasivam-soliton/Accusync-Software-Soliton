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
    public class PendingScreening
    {
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime AssignedTime { get; set; }
        public string WaitingTime { get; set; } = string.Empty;
        public List<PendingRiskFactor> RiskFactors { get; set; } = new List<PendingRiskFactor>();
    }
}
