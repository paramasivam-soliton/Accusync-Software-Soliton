// --------------------------------------------------------------------------------
// <copyright file="AuthenticationConstants.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Core.Constants
{
    /// <summary>
    /// Fixed authentication policy values shared across layers, rather than being
    /// duplicated as private constants local to a single class.
    /// </summary>
    public static class AuthenticationConstants
    {
        /// <summary>
        /// Password age, in days, at which a password is treated as expired. Fixed.
        /// </summary>
        public const int PasswordExpiryDays = 90;

        /// <summary>
        /// Consecutive failed login attempts before an account locks — the same fixed
        /// threshold for every account, admin-unconfigurable by design (unlike the lockout
        /// duration itself, which an Admin can set).
        /// </summary>
        public const int MaxFailedAttempts = 5;
    }
}
