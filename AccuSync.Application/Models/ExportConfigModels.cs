// --------------------------------------------------------------------------------
// <copyright file="ExportConfigModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    public class ExportEntry
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int ExportFormatIndex { get; set; } = 0;
        public int ExportDataIndex { get; set; } = 0;
        public string ExportFolder { get; set; } = "";
    }
}
