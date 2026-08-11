// --------------------------------------------------------------------------------
// <copyright file="CommentEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// A predefined comment entry available for selection during screening.
    /// </summary>
    public class CommentEntry
    {
        /// <summary>The comment text.</summary>
        public string Comment { get; set; } = "";
        /// <summary>Whether the comment is active and available for selection.</summary>
        public bool Active { get; set; } = true;
        /// <summary>Whether the comment is currently referenced by an existing record.</summary>
        public bool InUse { get; set; } = false;

        /// <summary>Selected index into the active/inactive ComboBox (0=Yes, 1=No).</summary>
        public int ActiveIndex { get; set; } = 0; // 0=Yes, 1=No

        /// <summary>Translated comment text keyed by language ComboBox index.</summary>
        public Dictionary<int, string> Translations { get; set; } = new();
    }
}
