// --------------------------------------------------------------------------------
// <copyright file="AuthenticationConstants.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Services.Authentication
{
    /// <summary>
    /// Constant used by <see cref="EncryptionService"/>. Not user-facing text — kept as a
    /// plain constant rather than moved to Resources/Strings.resx with the rest of this
    /// namespace's messages, since it identifies a Data Protection key ring and must never
    /// vary by culture.
    /// </summary>
    internal static class AuthenticationConstants
    {
        /// <summary>Data Protection purpose string identifying the encrypted-fields key ring.</summary>
        internal const string EncryptionProtectorPurpose = "AccuSync.Users.EncryptedFields";
    }
}
