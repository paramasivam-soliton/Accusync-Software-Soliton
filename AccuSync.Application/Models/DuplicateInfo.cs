// --------------------------------------------------------------------------------
// <copyright file="DuplicateInfo.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AccuSync.Application.Models
{
    public class DuplicateInfo
    {
        public List<string> InfoChanges { get; set; } = new();
        public int NewTests { get; set; }
    }
}
