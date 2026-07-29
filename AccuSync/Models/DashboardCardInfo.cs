// --------------------------------------------------------------------------------
// <copyright file="DashboardCardInfo.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Windows;

namespace AccuSync.Models
{
    /// <summary>
    /// Display model for the dashboard's overview stat cards and quick-action cards.
    /// Drives the ItemsControl templates in DashboardContentView so the repeated
    /// card markup lives in one place. <see cref="Value"/> is used by stat cards only.
    /// </summary>
    public class DashboardCardInfo
    {
        public string Tag { get; set; }
        public string IconBackground { get; set; }
        public string IconData { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
        public Thickness CardMargin { get; set; }
    }
}
