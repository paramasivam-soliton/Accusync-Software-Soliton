// --------------------------------------------------------------------------------
// <copyright file="DevicesUpdateModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    public class DeviceUpdateInfo
    {
        public string DeviceName { get; set; }
        public string CurrentVersion { get; set; }
        public string RequiredVersion { get; set; }
    }
}
