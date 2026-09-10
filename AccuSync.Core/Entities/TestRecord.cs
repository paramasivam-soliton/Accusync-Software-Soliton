using System;

namespace AccuSync.Core.Entities
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
        /// <summary>The test record's unique identifier.</summary>
        public string Id { get; set; }
        /// <summary>The test type: TEOAE, ABR, or DPOAE.</summary>
        public string TestType { get; set; }        // TEOAE, ABR, or DPOAE
        /// <summary>The ear tested: "Left Ear" or "Right Ear".</summary>
        public string Ear { get; set; }             // "Left Ear" or "Right Ear"
        /// <summary>The test result: "Pass", "Refer", or "Incomplete".</summary>
        public string TestResult { get; set; }      // "Pass", "Refer", or "Incomplete"
        /// <summary>When the test was performed.</summary>
        public DateTime TestDate { get; set; }
        /// <summary>How long the test took, in milliseconds.</summary>
        public int DurationMs { get; set; }

        // ABR-specific — null for TEOAE/DPOAE tests
        /// <summary>EEG myogenic noise percentage, for ABR tests only.</summary>
        public int? EegNoisePercent { get; set; }   // MyogenicNoiseIndication
        /// <summary>White electrode impedance in kΩ, for ABR tests only.</summary>
        public int? ImpedanceWhite { get; set; }    // kΩ
        /// <summary>Red electrode impedance in kΩ, for ABR tests only.</summary>
        public int? ImpedanceRed { get; set; }      // kΩ

        // Device / probe
        /// <summary>The serial number of the device that performed the test.</summary>
        public string DeviceSerial { get; set; }
        /// <summary>The display name of the device that performed the test.</summary>
        public string DeviceName { get; set; }
        /// <summary>The firmware build of the device that performed the test.</summary>
        public string FirmwareBuild { get; set; }
        /// <summary>The serial number of the probe used.</summary>
        public string ProbeSerial { get; set; }
        /// <summary>The type of probe used.</summary>
        public string ProbeType { get; set; }
        /// <summary>When the probe was last calibrated.</summary>
        public DateTime? ProbeCalibrationDate { get; set; }
        /// <summary>When the probe is next due for calibration.</summary>
        public DateTime? ProbeNextCalibrationDate { get; set; }

        // Facility / location
        /// <summary>The facility where the test was performed.</summary>
        public string TestFacility { get; set; }
        /// <summary>The location within the facility where the test was performed.</summary>
        public string TestLocation { get; set; }
        /// <summary>The examiner who performed the test.</summary>
        public string Examiner { get; set; }

        // Computed helpers
        /// <summary>Whether this test was performed on the left ear.</summary>
        public bool IsLeftEar => Ear == "Left Ear";
        /// <summary>Whether this test was performed on the right ear.</summary>
        public bool IsRightEar => Ear == "Right Ear";
        /// <summary>Whether this is an ABR test.</summary>
        public bool IsABR => TestType == "ABR";
        /// <summary>Whether this is a TEOAE test.</summary>
        public bool IsTEOAE => TestType == "TEOAE";
        /// <summary>Whether this is a DPOAE test.</summary>
        public bool IsDPOAE => TestType == "DPOAE";

        /// <summary>The test duration formatted as minutes:seconds.</summary>
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
        /// <summary>The test date and time formatted for display.</summary>
        public string TestDateFormatted
        {
            get
            {
                return TestDate.ToString("MM/dd/yyyy h:mm tt");
            }
        }

        /// <summary>The probe calibration date formatted for display, or an em dash if unknown.</summary>
        public string ProbeCalibrationFormatted =>
            ProbeCalibrationDate?.ToString("MM/dd/yyyy") ?? "—";

        /// <summary>The probe's next calibration date formatted for display, or an em dash if unknown.</summary>
        public string ProbeNextCalibrationFormatted =>
            ProbeNextCalibrationDate?.ToString("MM/dd/yyyy") ?? "—";
    }
}