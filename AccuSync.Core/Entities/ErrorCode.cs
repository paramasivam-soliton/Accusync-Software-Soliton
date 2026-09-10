// --------------------------------------------------------------------------------
// <copyright file="ErrorCode.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Core.Entities
{
    /// <summary>
    /// A short, stable code shown alongside a user-facing error message so a user can quote it
    /// to an administrator for troubleshooting, without needing raw exception details.
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>An unclassified failure caught by a generic catch-all handler.</summary>
        Unexpected = 0x102,
    }
}
