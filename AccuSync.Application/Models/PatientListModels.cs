// --------------------------------------------------------------------------------
// <copyright file="PatientListModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace AccuSync.Application.Models
{
    public class PatientInfo
    {
        public string MRN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public DateTime LastScreenDate { get; set; }
        public SolidColorBrush AccentColor { get; set; }
        public List<RiskFactor> RiskFactors { get; set; }
    }

    public class RiskFactor
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
