// --------------------------------------------------------------------------------
// <copyright file="UserRoleParser.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using AccuSync.Core.Entities;

namespace AccuSync.Application.Helpers
{
    /// <summary>
    /// Interprets <see cref="User.ProfileId"/> as a <see cref="UserRole"/>. Anything
    /// other than an exact (case-insensitive) "Admin" match — including null, empty,
    /// or an unrecognized value — resolves to Screener, the least-privileged role.
    /// </summary>
    public static class UserRoleParser
    {
        private const string AdminProfileId = "Admin";

        /// <summary>Resolves a stored <see cref="User.ProfileId"/> value to a <see cref="UserRole"/>.</summary>
        public static UserRole Parse(string profileId)
        {
            return string.Equals(profileId, AdminProfileId, StringComparison.OrdinalIgnoreCase)
                ? UserRole.Admin
                : UserRole.Screener;
        }
    }
}
