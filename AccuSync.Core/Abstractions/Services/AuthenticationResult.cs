// --------------------------------------------------------------------------------
// <copyright file="AuthenticationResult.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities;

namespace AccuSync.Core.Abstractions.Services
{
    /// <summary>
    /// Result of an authentication attempt. On failure, includes the reason
    /// and lockout details so the UI can display an appropriate message.
    /// </summary>
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public User User { get; set; }

        /// <summary>True when the failure is specifically account lockout (5 consecutive
        /// failed attempts) — auto-unlocks after the configured duration elapses, or
        /// sooner if an Admin unlocks it directly.</summary>
        public bool IsLocked { get; set; }
    }
}
