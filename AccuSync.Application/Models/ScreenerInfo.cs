// --------------------------------------------------------------------------------
// <copyright file="ScreenerInfo.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Windows.Media;

namespace AccuSync.Application.Models
{
    public class ScreenerInfo
    {
        public string Name { get; set; }
        public int ScreeningCount { get; set; }
        public string LastActivity { get; set; }
        public SolidColorBrush AvatarColor { get; set; }

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
