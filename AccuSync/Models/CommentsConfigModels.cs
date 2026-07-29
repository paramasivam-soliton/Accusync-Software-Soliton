// --------------------------------------------------------------------------------
// <copyright file="CommentsConfigModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AccuSync.Models
{
    public class CommentEntry
    {
        public string Comment { get; set; } = "";
        public bool Active { get; set; } = true;
        public bool InUse { get; set; } = false;

        public int ActiveIndex { get; set; } = 0; // 0=Yes, 1=No

        public Dictionary<int, string> Translations { get; set; } = new();
    }
}
