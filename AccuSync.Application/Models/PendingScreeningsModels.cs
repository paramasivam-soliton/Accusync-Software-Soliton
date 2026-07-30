// --------------------------------------------------------------------------------
// <copyright file="PendingScreeningsModels.cs" company="Natus Sensory">
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

    // Risk factor model for pending screenings
    public class PendingRiskFactor
    {
        public string Text { get; set; } = string.Empty;
        private string _background = "#FEF3C7";
        private string _foreground = "#92400E";

        public string Background
        {
            get => _background;
            set => _background = value;
        }

        public string Foreground
        {
            get => _foreground;
            set => _foreground = value;
        }
    }
}
