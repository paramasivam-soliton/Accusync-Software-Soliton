// --------------------------------------------------------------------------------
// <copyright file="DevicesContentModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Models
{
    public class DeviceEntry
    {
        public string Name { get; set; } = "";
        public string Serial { get; set; } = "";
        public string Code { get; set; } = "";
        public string Site { get; set; } = "";
        public int LanguageIndex { get; set; }
        public int TestResultTermsIndex { get; set; }
        public int PowerTimeout { get; set; } = 5;
        public int DisplayTimeout { get; set; } = 3;
        public int CalibrationPause { get; set; } = 10;
        public int DataDeletionIndex { get; set; }
        public int ABRAutostartIndex { get; set; }
        public int TEOAEProbeFitIndex { get; set; }
        public string LastSeen { get; set; } = "";
        public string LastUpdated { get; set; } = "";
        public string HardwareVersion { get; set; } = "";
        public string FirmwareVersion { get; set; } = "";

        public string LastSeenDisplay =>
            string.IsNullOrEmpty(LastSeen) ? "—" : LastSeen.Split(' ')[0];
    }

    public struct DeviceInfo
    {
        public string Name;
        public string FirmwareVersion;
        public string HardwareVersion;
    }
}
