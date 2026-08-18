// --------------------------------------------------------------------------------
// <copyright file="ScreenerInfo.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Summary information for a screener, shown on screener lists and leaderboards.
    /// </summary>
    public class ScreenerInfo
    {
        /// <summary>The screener's full name.</summary>
        public string Name { get; set; }
        /// <summary>The number of screenings performed by the screener.</summary>
        public int ScreeningCount { get; set; }
        /// <summary>Display text for the screener's last activity.</summary>
        public string LastActivity { get; set; }
        /// <summary>Color used for the screener's avatar.</summary>
        public string AvatarColor { get; set; }

        /// <summary>The screener's initials, derived from <see cref="Name"/>.</summary>
        public string Initials
        {
            get
            {
                var parts = Name.Split(' ');
                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[1][0]}";
                return parts[0].Substring(0, Math.Min(2, parts[0].Length));
            }
        }
    }
}
