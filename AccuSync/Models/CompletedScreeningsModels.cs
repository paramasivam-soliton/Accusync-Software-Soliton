// --------------------------------------------------------------------------------
// <copyright file="CompletedScreeningsModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace AccuSync.Models
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

    public class ScreeningBadge
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
