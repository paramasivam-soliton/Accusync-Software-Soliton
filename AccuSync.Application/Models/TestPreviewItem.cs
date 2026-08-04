// --------------------------------------------------------------------------------
// <copyright file="TestPreviewItem.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    public class TestPreviewItem
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string Ear { get; set; }
        public string Type { get; set; }
        public string Result { get; set; }                     // "Pass", "Refer", "Incomplete"
        public bool IsExisting { get; set; }
    }
}
