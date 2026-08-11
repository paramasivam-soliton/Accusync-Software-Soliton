// --------------------------------------------------------------------------------
// <copyright file="TestPreviewItem.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    /// <summary>
    /// A single test result row shown in the import preview table.
    /// </summary>
    public class TestPreviewItem
    {
        /// <summary>The test date.</summary>
        public string Date { get; set; }
        /// <summary>The test time.</summary>
        public string Time { get; set; }
        /// <summary>The ear tested.</summary>
        public string Ear { get; set; }
        /// <summary>The test type (e.g., TEOAE, ABR, DPOAE).</summary>
        public string Type { get; set; }
        /// <summary>The test result ("Pass", "Refer", or "Incomplete").</summary>
        public string Result { get; set; }                     // "Pass", "Refer", "Incomplete"
        /// <summary>Whether this test already exists in the database.</summary>
        public bool IsExisting { get; set; }
    }
}
