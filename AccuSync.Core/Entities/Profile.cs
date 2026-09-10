// --------------------------------------------------------------------------------
// <copyright file="Profile.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Core.Entities
{
    /// <summary>
    /// A role assignable to a <see cref="User"/> via <see cref="User.ProfileId"/>.
    /// <see cref="ProfileId"/> is defined to equal the corresponding <see cref="UserRole"/>
    /// enum value, so resolving a user's role from a stored profile id is a direct cast
    /// rather than a lookup.
    /// </summary>
    public class Profile
    {
        /// <summary>The profile's identifier; equal to the matching <see cref="UserRole"/> enum value.</summary>
        public int ProfileId { get; set; }

        /// <summary>The profile's display name (e.g. "Admin", "Screener").</summary>
        public string Name { get; set; }
    }
}
