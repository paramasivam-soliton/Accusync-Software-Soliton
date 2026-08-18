// --------------------------------------------------------------------------------
// <copyright file="PendingRiskFactor.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    // Risk factor model for pending screenings
    /// <summary>
    /// A risk factor badge shown on a pending screening's card.
    /// </summary>
    public class PendingRiskFactor
    {
        /// <summary>The badge's display text.</summary>
        public string Text { get; set; } = string.Empty;
        private string _background = "#FEF3C7";
        private string _foreground = "#92400E";

        /// <summary>The badge's background color.</summary>
        public string Background
        {
            get => _background;
            set => _background = value;
        }

        /// <summary>The badge's text (foreground) color.</summary>
        public string Foreground
        {
            get => _foreground;
            set => _foreground = value;
        }
    }
}
