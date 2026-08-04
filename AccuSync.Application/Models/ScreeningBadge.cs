// --------------------------------------------------------------------------------
// <copyright file="ScreeningBadge.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
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
