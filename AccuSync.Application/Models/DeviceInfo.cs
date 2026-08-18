// --------------------------------------------------------------------------------
// <copyright file="DeviceInfo.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Basic identity and version information for a connected screening device.
    /// </summary>
    public struct DeviceInfo
    {
        /// <summary>The device's display name.</summary>
        public string Name;
        /// <summary>The device's firmware version.</summary>
        public string FirmwareVersion;
        /// <summary>The device's hardware version.</summary>
        public string HardwareVersion;
    }
}
