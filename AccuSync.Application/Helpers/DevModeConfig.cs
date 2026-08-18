// --------------------------------------------------------------------------------
// <copyright file="DevModeConfig.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.IO;

namespace AccuSync.Application.Helpers
{
    /// <summary>
    /// Provides file-based dev mode bypass flags for login and dashboard screens.
    /// Each flag is an independent opt-in: you can skip login, the dashboard, or both.
    ///
    /// Flag files live in <c>C:\ProgramData\AccuSync\</c>:
    ///   <list type="bullet">
    ///     <item><c>skip_login.flag</c> — Bypasses login. Optional content: <c>Username|Role</c> (default: <c>DevUser|Admin</c>).</item>
    ///     <item><c>skip_dashboard.flag</c> — Bypasses dashboard. Optional content: target screen name (default: <c>PatientInformation</c>).</item>
    ///   </list>
    ///
    /// Create the flags manually or run <c>enable-dev.ps1</c> from the project root.
    /// </summary>
    public static class DevModeConfig
    {
        private static readonly string BasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "AccuSync");

        private static readonly string LoginFlagPath = Path.Combine(BasePath, "skip_login.flag");
        private static readonly string DashboardFlagPath = Path.Combine(BasePath, "skip_dashboard.flag");

        // TODO: These hit the filesystem on every access. Consider caching the result
        //       at startup if the checks become a performance concern.
        /// <summary>
        /// Gets a value indicating whether <c>skip_login.flag</c> is present.
        /// </summary>
        public static bool SkipLogin => File.Exists(LoginFlagPath);

        /// <summary>
        /// Gets a value indicating whether <c>skip_dashboard.flag</c> is present.
        /// </summary>
        public static bool SkipDashboard => File.Exists(DashboardFlagPath);

        /// <summary>
        /// Gets a value indicating whether any dev mode flag is currently active.
        /// </summary>
        public static bool IsAnyDevMode => SkipLogin || SkipDashboard;

        /// <summary>
        /// Reads dev user info from <c>skip_login.flag</c>.
        /// Expected format: <c>Username|Role</c> (e.g., <c>"DevUser|Admin"</c>).
        /// Falls back to defaults if the file is missing, empty, or malformed.
        /// </summary>
        public static (string Username, string Role) GetDevUser()
        {
            try
            {
                if (File.Exists(LoginFlagPath))
                {
                    string content = File.ReadAllText(LoginFlagPath).Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        var parts = content.Split('|');
                        string username = parts[0].Trim();
                        string role = parts.Length > 1 ? parts[1].Trim() : "Admin";
                        if (!string.IsNullOrEmpty(username))
                            return (username, role);
                    }
                }
            }
            catch (Exception)
            {
                // File read failed (permissions, locked, etc.) — fall through to defaults.
            }

            return ("DevUser", "Admin");
        }

        /// <summary>
        /// Reads the target screen name from <c>skip_dashboard.flag</c>.
        /// Falls back to <c>"PatientInformation"</c> if the file is missing or empty.
        /// </summary>
        public static string GetTargetScreen()
        {
            try
            {
                if (File.Exists(DashboardFlagPath))
                {
                    string content = File.ReadAllText(DashboardFlagPath).Trim();
                    if (!string.IsNullOrEmpty(content))
                        return content;
                }
            }
            catch (Exception)
            {
                // File read failed — fall through to default.
            }

            return "PatientInformation";
        }

        /// <summary>
        /// Builds a title-bar suffix showing which dev flags are active
        /// (e.g., <c>" [DEV: skip_login, skip_dashboard]"</c>).
        /// Returns <see cref="string.Empty"/> when no flags are set.
        /// </summary>
        public static string GetDevModeLabel()
        {
            if (!IsAnyDevMode) return string.Empty;

            var flags = new System.Collections.Generic.List<string>();
            if (SkipLogin) flags.Add("skip_login");
            if (SkipDashboard) flags.Add("skip_dashboard");

            return $" [DEV: {string.Join(", ", flags)}]";
        }

        /// <summary>
        /// Ensures the <c>ProgramData\AccuSync</c> directory exists.
        /// Safe to call multiple times; no-ops if the directory is already there.
        /// </summary>
        public static void EnsureDirectory()
        {
            if (!Directory.Exists(BasePath))
                Directory.CreateDirectory(BasePath);
        }
    }
}