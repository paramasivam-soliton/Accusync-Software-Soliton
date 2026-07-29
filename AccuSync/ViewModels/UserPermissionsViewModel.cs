// --------------------------------------------------------------------------------
// <copyright file="UserPermissionsViewModel.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.ViewModels
{
    /// <summary>
    /// Navigation and feature permissions for a logged-in user.
    /// Populated from the user's assigned profile after authentication.
    /// Used by <c>SidebarNavigation.SetPermissions()</c> to show/hide nav items,
    /// and by views to enable/disable features.
    /// </summary>
    // NOTE: This replaces the AccountName == "Admin" string checks in
    //       LoginViewModel and ChangePasswordViewModel. Once those ViewModels
    //       use UserPermissionsViewModel.Role for routing, the case-sensitivity bugs
    //       flagged there go away.
    public class UserPermissionsViewModel
    {
        public string Role { get; set; } = "Screener";

        // Sidebar nav visibility
        public bool CanAccessDashboard { get; set; } = true;
        public bool CanAccessPatients { get; set; } = true;
        public bool CanAccessUsers { get; set; }
        public bool CanAccessSites { get; set; }
        public bool CanAccessDevices { get; set; }
        public bool CanAccessSysConfig { get; set; }
        public bool CanAccessSettings { get; set; }
        public bool CanAccessAbout { get; set; } = true;

        // Patient feature flags
        public bool CanAddPatient { get; set; } = true;
        public bool CanEditPatient { get; set; } = true;
        public bool CanDeletePatient { get; set; }
        public bool CanExportPatient { get; set; }
        public bool CanImportPatient { get; set; }
        public bool CanViewReports { get; set; }

        // Presets

        /// <summary>Full admin — all nav items and features enabled.</summary>
        public static UserPermissionsViewModel Admin() => new UserPermissionsViewModel
        {
            Role = "Admin",
            CanAccessUsers = true,
            CanAccessSites = true,
            CanAccessDevices = true,
            CanAccessSysConfig = true,
            CanAccessSettings = true,
            CanDeletePatient = true,
            CanExportPatient = true,
            CanImportPatient = true,
            CanViewReports = true,
        };

        /// <summary>Screener — patients only, no admin nav items.</summary>
        public static UserPermissionsViewModel Screener() => new UserPermissionsViewModel
        {
            Role = "Screener",
            CanAddPatient = true,
            CanEditPatient = true,
            CanExportPatient = true,
        };

        /// <summary>Read-only — can view patients but not modify.</summary>
        public static UserPermissionsViewModel ReadOnly() => new UserPermissionsViewModel
        {
            Role = "ReadOnly",
            CanAddPatient = false,
            CanEditPatient = false,
        };
    }
}