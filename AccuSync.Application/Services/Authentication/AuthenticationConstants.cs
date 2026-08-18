// --------------------------------------------------------------------------------
// <copyright file="AuthenticationConstants.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Services.Authentication
{
    /// <summary>
    /// Constants used by <see cref="EncryptionService"/> and <see cref="AuthenticationService"/>
    /// that are not user-facing text, so — unlike this namespace's messages — they stay here
    /// rather than move to Resources/Strings.resx.
    /// </summary>
    internal static class AuthenticationConstants
    {
        /// <summary>Data Protection purpose string identifying the encrypted-fields key ring. Must never vary by culture.</summary>
        internal const string EncryptionProtectorPurpose = "AccuSync.Users.EncryptedFields";

        /// <summary>
        /// Consecutive failed login attempts before an account locks — the same fixed
        /// threshold for every account, admin-unconfigurable by design (unlike the lockout
        /// duration itself, which <see cref="AccuSync.Core.Abstractions.Repositories.IAppSettingsRepository"/>
        /// does let an Admin set).
        /// </summary>
        internal const int MaxFailedAttempts = 5;
    }
}
