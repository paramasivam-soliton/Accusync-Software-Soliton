// --------------------------------------------------------------------------------
// <copyright file="IProfileRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using AccuSync.Core.Entities;

namespace AccuSync.Core.Abstractions.Repositories
{
    /// <summary>
    /// Persistence contract for profiles. Implemented by AccuSync.EF (EF Core,
    /// SQLite-backed).
    /// </summary>
    public interface IProfileRepository
    {
        /// <summary>Returns every stored profile.</summary>
        Task<List<Profile>> GetAllProfilesAsync();

        /// <summary>Looks up a single profile by id.</summary>
        /// <param name="profileId">The profile's identifier.</param>
        /// <returns>The matching <see cref="Profile"/>, or <c>null</c> if none is found.</returns>
        Task<Profile> GetProfileByIdAsync(string profileId);

        /// <summary>Creates a new profile.</summary>
        /// <param name="profile">The profile to create, including its granted permission flags.</param>
        /// <returns><c>true</c> if the profile was created; otherwise <c>false</c>.</returns>
        Task<bool> CreateProfileAsync(Profile profile);

        /// <summary>
        /// Persists changes to an existing profile, including its granted
        /// permission flags. Returns <c>false</c> if the profile no longer exists.
        /// </summary>
        /// <param name="profile">The profile to update.</param>
        Task<bool> UpdateProfileAsync(Profile profile);
    }
}
