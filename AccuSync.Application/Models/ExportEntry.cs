// --------------------------------------------------------------------------------
// <copyright file="ExportEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    /// <summary>
    /// A configurable export profile entry.
    /// </summary>
    public class ExportEntry
    {
        /// <summary>The export profile's display name.</summary>
        public string Name { get; set; } = "";
        /// <summary>A short description of the export profile.</summary>
        public string Description { get; set; } = "";
        /// <summary>Selected index into the export-format ComboBox.</summary>
        public int ExportFormatIndex { get; set; } = 0;
        /// <summary>Selected index into the export-data ComboBox.</summary>
        public int ExportDataIndex { get; set; } = 0;
        /// <summary>The destination folder for exported files.</summary>
        public string ExportFolder { get; set; } = "";
    }
}
