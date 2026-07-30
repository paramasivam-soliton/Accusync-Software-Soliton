// --------------------------------------------------------------------------------
// <copyright file="FieldSetupConfigModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    public class FieldEntry
    {
        public string FieldName { get; set; } = "";
        public string CustomLabel { get; set; }  // null = use FieldName as display label
        public bool IsActive { get; set; } = true;
        public bool IsMandatory { get; set; } = false;
        public bool IncludeInQR { get; set; } = false;
    }
}
