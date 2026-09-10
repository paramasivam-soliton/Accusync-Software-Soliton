// --------------------------------------------------------------------------------
// <copyright file="ErrorCodeExtensions.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Core.Entities
{
    /// <summary>
    /// Formatting helper for <see cref="ErrorCode"/>.
    /// </summary>
    public static class ErrorCodeExtensions
    {
        /// <summary>
        /// Renders <paramref name="code"/> as the hexadecimal form shown to users, e.g.
        /// <see cref="ErrorCode.Unexpected"/> becomes <c>"0x102"</c>.
        /// </summary>
        /// <param name="code">The code to render.</param>
        public static string ToDisplayCode(this ErrorCode code) => $"0x{(int)code:X3}";
    }
}
