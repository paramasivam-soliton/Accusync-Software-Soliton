// --------------------------------------------------------------------------------
// <copyright file="DeviceUpdateInfo.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Describes a pending firmware update for a device: its current version versus the required one.
    /// </summary>
    public class DeviceUpdateInfo
    {
        /// <summary>The device's display name.</summary>
        public string DeviceName { get; set; }
        /// <summary>The device's currently installed firmware version.</summary>
        public string CurrentVersion { get; set; }
        /// <summary>The firmware version the device needs to be updated to.</summary>
        public string RequiredVersion { get; set; }
    }
}
