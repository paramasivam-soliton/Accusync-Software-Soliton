// --------------------------------------------------------------------------------
// <copyright file="AssignedPatientsModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows.Media;

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
        public SolidColorBrush AccentColor { get; set; }
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

    public class Badge
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
