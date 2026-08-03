// --------------------------------------------------------------------------------
// <copyright file="PatientInfo.cs" company="Natus Sensory">
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
}
