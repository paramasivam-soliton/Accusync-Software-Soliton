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
    /// Interprets <see cref="User.ProfileId"/> as a <see cref="UserRole"/>. Any value that
    /// isn't a defined <see cref="UserRole"/> member — including an id with no matching
    /// <see cref="Profile"/> row — resolves to Screener, the least-privileged role.
    /// </summary>
    public static class UserRoleParser
    {
        /// <summary>Resolves a stored <see cref="User.ProfileId"/> value to a <see cref="UserRole"/>.</summary>
        /// <param name="profileId">The stored profile id to resolve.</param>
        /// <returns>The matching <see cref="UserRole"/>, or <see cref="UserRole.Screener"/> if none is defined.</returns>
        public static UserRole Parse(int profileId)
        {
            return Enum.IsDefined(typeof(UserRole), profileId) ? (UserRole)profileId : UserRole.Screener;
        }
    }
}
