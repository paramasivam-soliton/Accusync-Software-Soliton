// --------------------------------------------------------------------------------
// <copyright file="IUserRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using AccuSync.Core.Entities;
using AccuSync.Core.Exceptions;

namespace AccuSync.Core.Abstractions.Repositories
{
    /// <summary>
    /// Plain persistence contract for user accounts — stores and returns <see cref="User"/>
    /// field values exactly as given, with no encryption, hashing, or other transformation.
    /// That business logic belongs to <see cref="Services.IUserService"/>, the only intended
    /// caller of this interface. Implemented by AccuSync.EF (EF Core, SQLite-backed).
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>Returns every stored user account, unmodified.</summary>
        /// <returns>A list of all <see cref="User"/> records.</returns>
        Task<List<User>> GetAllUsersAsync();

        /// <summary>Looks up a user by its deterministic username hash (blind index).</summary>
        /// <param name="usernameHash">The precomputed hash to match against <see cref="User.UsernameHash"/>.</param>
        /// <returns>The matching <see cref="User"/>, unmodified, or <c>null</c> if none is found.</returns>
        Task<User> GetUserByUsernameHashAsync(string usernameHash);

        /// <summary>
        /// Persists changes to an existing user.
        /// </summary>
        /// <param name="user">The user to update, with field values already in their storage form.</param>
        /// <returns><c>true</c> if the update succeeded.</returns>
        /// <exception cref="UserNotFoundException">No stored user matches <paramref name="user"/>'s <see cref="User.Id"/>.</exception>
        Task<bool> UpdateUserAsync(User user);

        /// <summary>
        /// Creates a new user account.
        /// </summary>
        /// <param name="user">The user to create, with field values already in their storage form.</param>
        /// <returns><c>true</c> if the user was created; otherwise <c>false</c>.</returns>
        Task<bool> CreateUserAsync(User user);

        /// <summary>
        /// Assigns a new value to a user's <see cref="User.ProfileId"/> field only —
        /// no other field is touched. Used for role reassignment.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update.</param>
        /// <param name="profileId">The new profile id to store.</param>
        /// <returns><c>true</c> if the update succeeded; <c>false</c> if <paramref name="profileId"/> matches no <see cref="Profile"/>.</returns>
        /// <exception cref="UserNotFoundException">No stored user matches <paramref name="userId"/>.</exception>
        Task<bool> UpdateUserProfileIdAsync(string userId, int profileId);

        /// <summary>
        /// Activates or deactivates a user account. Deactivated accounts are blocked from
        /// authenticating regardless of password correctness (see <c>AuthenticationService</c>).
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update.</param>
        /// <param name="isActive">Whether the account should be able to authenticate.</param>
        /// <returns><c>true</c> if the update succeeded.</returns>
        /// <exception cref="UserNotFoundException">No stored user matches <paramref name="userId"/>.</exception>
        Task<bool> SetUserActiveStatusAsync(string userId, bool isActive);

        /// <summary>
        /// Clears a lockout directly. Resets <c>FailedLoginAttemptCount</c> and
        /// <c>FirstFailedLoginTime</c> together, so the account is fully reset rather
        /// than nominally "unlocked."
        /// </summary>
        /// <param name="userId">The unique identifier of the user to unlock.</param>
        /// <returns><c>true</c> if the update succeeded.</returns>
        /// <exception cref="UserNotFoundException">No stored user matches <paramref name="userId"/>.</exception>
        Task<bool> UnlockUserAsync(string userId);
    }
}
