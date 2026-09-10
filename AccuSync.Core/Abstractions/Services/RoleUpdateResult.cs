// --------------------------------------------------------------------------------
// <copyright file="RoleUpdateResult.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Core.Abstractions.Services
{
    /// <summary>
    /// Result of a role reassignment. On failure, <see cref="ErrorMessage"/> carries a
    /// user-facing reason instead of the caller having to infer one from a bare
    /// <c>false</c>.
    /// </summary>
    public class RoleUpdateResult
    {
        /// <summary>
        /// Whether the role was reassigned.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// User-facing message describing why the update failed. Empty on success.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
