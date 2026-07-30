// --------------------------------------------------------------------------------
// <copyright file="ScreenerNotExportedModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Application.Models
{
    // Model class for screener not exported screenings
    public class ScreenerNotExportedScreening
    {
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime CompletedTime { get; set; }
        public string Result { get; set; } = string.Empty;
    }
}
