// --------------------------------------------------------------------------------
// <copyright file="AuthenticationResult.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using AccuSync.Application.Models;

namespace AccuSync.Application.Services
{
    /// <summary>
    /// Result of an authentication attempt. On failure, includes the reason
    /// and lockout details so the UI can display an appropriate message.
    /// </summary>
    public class AuthenticationResult
    {
        /// <summary>
        /// Whether authentication succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// User-facing message describing why authentication failed. Empty on success.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The authenticated user, populated only when <see cref="Success"/> is <c>true</c>.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Whether the account is currently locked out due to failed login attempts.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// How long the account remains locked out, when <see cref="IsLocked"/> is <c>true</c>.
        /// </summary>
        public TimeSpan RemainingLockTime { get; set; }
    }
}
