// --------------------------------------------------------------------------------
// <copyright file="AuthenticationConstants.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Services.Authentication
{
    /// <summary>
    /// Constant strings used by <see cref="EncryptionService"/> and
    /// <see cref="AuthenticationService"/>, kept in one place instead of hardcoded inline.
    /// </summary>
    internal static class AuthenticationConstants
    {
        /// <summary>Data Protection purpose string identifying the encrypted-fields key ring.</summary>
        internal const string EncryptionProtectorPurpose = "AccuSync.Users.EncryptedFields";

        internal const string ErrorCredentialsRequired = "Username and password are required.";

        // Same message whether the account exists or not, so attackers can't enumerate valid account names.
        internal const string ErrorInvalidCredentials = "Invalid username or password.";

        internal const string ErrorInvalidCredentialsWithAttemptsFormat = "Invalid username or password. {0} attempt(s) remaining.";

        internal const string ErrorAccountLockedFormat = "Account is locked. Please try again in {0} minutes.";

        internal const string ErrorAccountNowLockedFormat = "Account is now locked due to {0} failed attempts. Please try again in {1} minutes.";

        internal const string ErrorPasswordExpired = "Your password has expired. Please contact your administrator.";
    }
}
