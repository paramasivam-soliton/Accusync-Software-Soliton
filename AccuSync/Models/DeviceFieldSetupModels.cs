// --------------------------------------------------------------------------------
// <copyright file="DeviceFieldSetupModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Models
{
    public class DeviceFieldEntry
    {
        public string FieldName { get; set; } = "";
        public string SystemCustomLabel { get; set; }  // from FieldSetup.CustomLabel (read-only here)
        public string DeviceCustomLabel { get; set; }  // editable — DeviceFieldSetup.CustomLabel
        public bool IsActive { get; set; } = true;
        public bool IsMandatory { get; set; } = false;

        /// <summary>
        /// The label this device inherits when DeviceCustomLabel is null:
        /// system-level custom label if set, otherwise the default field name.
        /// </summary>
        public string InheritedLabel => SystemCustomLabel ?? FieldName;
    }
}
