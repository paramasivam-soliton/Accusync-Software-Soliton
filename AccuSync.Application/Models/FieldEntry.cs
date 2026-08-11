// --------------------------------------------------------------------------------
// <copyright file="FieldEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Configuration for a patient data field: its label, active/required state, and QR inclusion.
    /// </summary>
    public class FieldEntry
    {
        /// <summary>The field's underlying name.</summary>
        public string FieldName { get; set; } = "";
        /// <summary>Custom display label; null means use <see cref="FieldName"/> as the display label.</summary>
        public string CustomLabel { get; set; }  // null = use FieldName as display label
        /// <summary>Whether the field is active.</summary>
        public bool IsActive { get; set; } = true;
        /// <summary>Whether the field is mandatory.</summary>
        public bool IsMandatory { get; set; } = false;
        /// <summary>Whether the field is included when generating patient QR codes.</summary>
        public bool IncludeInQR { get; set; } = false;
    }
}
