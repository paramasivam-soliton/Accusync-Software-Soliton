// --------------------------------------------------------------------------------
// <copyright file="DashboardCardInfo.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Presentation.Models
{
    /// <summary>
    /// Display model for the dashboard's overview stat cards and quick-action cards.
    /// Drives the ItemsControl templates in DashboardContentView so the repeated
    /// card markup lives in one place. <see cref="Value"/> is used by stat cards only.
    /// </summary>
    public class DashboardCardInfo
    {
        /// <summary>Identifier used to distinguish the card for command binding.</summary>
        public string Tag { get; set; }

        /// <summary>Background color for the card's icon.</summary>
        public string IconBackground { get; set; }

        /// <summary>Geometry data for the card's icon.</summary>
        public string IconData { get; set; }

        /// <summary>The card's label text.</summary>
        public string Label { get; set; }

        /// <summary>The stat value shown on the card. Used by stat cards only.</summary>
        public string Value { get; set; }

        /// <summary>Margin applied around the card, as "left,top,right,bottom" (parsed by StringToThicknessConverter).</summary>
        public string CardMargin { get; set; }
    }
}
