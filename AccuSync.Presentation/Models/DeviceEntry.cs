// --------------------------------------------------------------------------------
// <copyright file="DeviceEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Presentation.Models
{
    /// <summary>
    /// A configurable screening device entry shown in the device management list.
    /// </summary>
    public class DeviceEntry
    {
        /// <summary>The device's display name.</summary>
        public string Name { get; set; } = "";

        /// <summary>The device's serial number.</summary>
        public string Serial { get; set; } = "";

        /// <summary>The device's code.</summary>
        public string Code { get; set; } = "";

        /// <summary>The site the device is assigned to.</summary>
        public string Site { get; set; } = "";

        /// <summary>Selected index into the language ComboBox.</summary>
        public int LanguageIndex { get; set; }

        /// <summary>Selected index into the test-result-terms ComboBox.</summary>
        public int TestResultTermsIndex { get; set; }

        /// <summary>Minutes of inactivity before the device powers off.</summary>
        public int PowerTimeout { get; set; } = 5;

        /// <summary>Minutes of inactivity before the device's display turns off.</summary>
        public int DisplayTimeout { get; set; } = 3;

        /// <summary>Seconds paused between calibration steps.</summary>
        public int CalibrationPause { get; set; } = 10;

        /// <summary>Selected index into the data-deletion policy ComboBox.</summary>
        public int DataDeletionIndex { get; set; }

        /// <summary>Selected index into the ABR autostart ComboBox.</summary>
        public int ABRAutostartIndex { get; set; }

        /// <summary>Selected index into the TEOAE probe-fit ComboBox.</summary>
        public int TEOAEProbeFitIndex { get; set; }

        /// <summary>When the device was last seen online.</summary>
        public string LastSeen { get; set; } = "";

        /// <summary>When the device's configuration was last updated.</summary>
        public string LastUpdated { get; set; } = "";

        /// <summary>The device's hardware version.</summary>
        public string HardwareVersion { get; set; } = "";

        /// <summary>The device's firmware version.</summary>
        public string FirmwareVersion { get; set; } = "";

        /// <summary>The date portion of <see cref="LastSeen"/>, or an em dash if unknown.</summary>
        public string LastSeenDisplay =>
            string.IsNullOrEmpty(LastSeen) ? "—" : LastSeen.Split(' ')[0];
    }
}
