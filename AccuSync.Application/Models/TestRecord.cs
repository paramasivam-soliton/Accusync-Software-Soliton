using System;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// A single hearing screening test result imported from a device.
    /// Each record represents one ear tested with one method (TEOAE, ABR, or DPOAE).
    /// </summary>
    // TODO: String comparisons for Ear, TestType, and TestResult are fragile —
    //       a typo or casing mismatch silently breaks the computed helpers.
    //       Consider enums for these fields.
    public class TestRecord
    {
        // Core fields
        public string Id { get; set; }
        public string TestType { get; set; }        // TEOAE, ABR, or DPOAE
        public string Ear { get; set; }             // "Left Ear" or "Right Ear"
        public string TestResult { get; set; }      // "Pass", "Refer", or "Incomplete"
        public DateTime TestDate { get; set; }
        public int DurationMs { get; set; }

        // ABR-specific — null for TEOAE/DPOAE tests
        public int? EegNoisePercent { get; set; }   // MyogenicNoiseIndication
        public int? ImpedanceWhite { get; set; }    // kΩ
        public int? ImpedanceRed { get; set; }      // kΩ

        // Device / probe
        public string DeviceSerial { get; set; }
        public string DeviceName { get; set; }
        public string FirmwareBuild { get; set; }
        public string ProbeSerial { get; set; }
        public string ProbeType { get; set; }
        public DateTime? ProbeCalibrationDate { get; set; }
        public DateTime? ProbeNextCalibrationDate { get; set; }

        // Facility / location
        public string TestFacility { get; set; }
        public string TestLocation { get; set; }
        public string Examiner { get; set; }

        // Computed helpers
        public bool IsLeftEar => Ear == "Left Ear";
        public bool IsRightEar => Ear == "Right Ear";
        public bool IsABR => TestType == "ABR";
        public bool IsTEOAE => TestType == "TEOAE";
        public bool IsDPOAE => TestType == "DPOAE";

        public string DurationFormatted
        {
            get
            {
                int totalSeconds = DurationMs / 1000;
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                return $"{minutes:D2}:{seconds:D2}";
            }
        }

        // TODO: Hardcoded US date format. Should respect the user's culture or
        //       a configurable application-wide format setting.
        public string TestDateFormatted
        {
            get
            {
                return TestDate.ToString("MM/dd/yyyy h:mm tt");
            }
        }

        public string ProbeCalibrationFormatted =>
            ProbeCalibrationDate?.ToString("MM/dd/yyyy") ?? "—";

        public string ProbeNextCalibrationFormatted =>
            ProbeNextCalibrationDate?.ToString("MM/dd/yyyy") ?? "—";
    }
}