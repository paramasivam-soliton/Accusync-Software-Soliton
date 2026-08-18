// --------------------------------------------------------------------------------
// <copyright file="DuplicateInfo.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Describes how an incoming import record differs from an existing patient record.
    /// </summary>
    public class DuplicateInfo
    {
        /// <summary>Descriptions of the patient information fields that changed.</summary>
        public List<string> InfoChanges { get; set; } = new();
        
        /// <summary>The number of new test results included in the import.</summary>
        public int NewTests { get; set; }
    }
}
