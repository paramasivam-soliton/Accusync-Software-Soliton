// --------------------------------------------------------------------------------
// <copyright file="DeviceFieldEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Per-device override of a patient data field's label and required/active state.
    /// </summary>
    public class DeviceFieldEntry
    {
        /// <summary>The underlying field name.</summary>
        public string FieldName { get; set; } = "";
        /// <summary>The system-level custom label, from FieldSetup.CustomLabel (read-only here).</summary>
        public string SystemCustomLabel { get; set; }  // from FieldSetup.CustomLabel (read-only here)
        /// <summary>The device-level custom label override — editable, DeviceFieldSetup.CustomLabel.</summary>
        public string DeviceCustomLabel { get; set; }  // editable — DeviceFieldSetup.CustomLabel
        /// <summary>Whether the field is active on this device.</summary>
        public bool IsActive { get; set; } = true;
        /// <summary>Whether the field is mandatory on this device.</summary>
        public bool IsMandatory { get; set; } = false;

        /// <summary>
        /// The label this device inherits when DeviceCustomLabel is null:
        /// system-level custom label if set, otherwise the default field name.
        /// </summary>
        public string InheritedLabel => SystemCustomLabel ?? FieldName;
    }
}
