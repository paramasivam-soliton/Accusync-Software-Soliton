// --------------------------------------------------------------------------------
// <copyright file="UnlockUserResult.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Core.Abstractions.Services
{
    /// <summary>
    /// Result of an account-unlock action. On failure, <see cref="ErrorMessage"/> carries
    /// a user-facing reason instead of the caller having to infer one from a bare <c>false</c>.
    /// </summary>
    public class UnlockUserResult
    {
        /// <summary>
        /// Whether the account was unlocked.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// User-facing message describing why the unlock failed. Empty on success.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
