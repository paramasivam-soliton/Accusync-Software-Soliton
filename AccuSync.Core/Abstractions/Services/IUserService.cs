// --------------------------------------------------------------------------------
// <copyright file="IUserService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using AccuSync.Core.Entities;

namespace AccuSync.Core.Abstractions.Services
{
    /// <summary>
    /// User account use cases: encrypts/decrypts and hashes fields around the plain
    /// persistence <see cref="Repositories.IUserRepository"/>, so every other caller
    /// works only with plaintext <see cref="User"/> data.
    /// </summary>
    public interface IUserService
    {
        /// <summary>Returns every stored user account, decrypted.</summary>
        /// <returns>A list of all <see cref="User"/> records.</returns>
        Task<List<User>> GetAllUsersAsync();

        /// <summary>Looks up a user by account name.</summary>
        /// <param name="accountName">The account name to search for.</param>
        /// <returns>The matching <see cref="User"/>, decrypted, or <c>null</c> if none is found.</returns>
        Task<User> GetUserByAccountNameAsync(string accountName);

        /// <summary>
        /// Persists changes to an existing user.
        /// </summary>
        /// <param name="user">The user to update, with plaintext field values.</param>
        /// <returns><c>true</c> if the update succeeded; <c>false</c> if the account no longer exists or the update otherwise failed.</returns>
        Task<bool> UpdateUserAsync(User user);

        /// <summary>
        /// Creates a new user account.
        /// </summary>
        /// <param name="user">The user to create, with plaintext field values.</param>
        /// <returns><c>true</c> if the user was created; otherwise <c>false</c>.</returns>
        Task<bool> CreateUserAsync(User user);

        /// <summary>
        /// Assigns or changes a user's role. The new role takes effect on that user's
        /// next login, not live for any session already in progress.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update.</param>
        /// <param name="role">The role to assign.</param>
        /// <returns>
        /// A <see cref="RoleUpdateResult"/> indicating success, or failure with a
        /// user-facing <see cref="RoleUpdateResult.ErrorMessage"/> describing why.
        /// </returns>
        Task<RoleUpdateResult> UpdateUserRoleAsync(string userId, UserRole role);

        /// <summary>
        /// Activates or deactivates a user account. Deactivated accounts are blocked from
        /// authenticating regardless of password correctness (see <c>AuthenticationService</c>).
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update.</param>
        /// <param name="isActive">Whether the account should be able to authenticate.</param>
        /// <returns>
        /// An <see cref="ActiveStatusUpdateResult"/> indicating success, or failure with a
        /// user-facing <see cref="ActiveStatusUpdateResult.ErrorMessage"/> describing why.
        /// </returns>
        Task<ActiveStatusUpdateResult> SetUserActiveStatusAsync(string userId, bool isActive);

        /// <summary>
        /// Clears a lockout directly. Resets the failed-attempt streak so the account is
        /// fully reset rather than nominally "unlocked."
        /// </summary>
        /// <param name="userId">The unique identifier of the user to unlock.</param>
        /// <returns>
        /// An <see cref="UnlockUserResult"/> indicating success, or failure with a
        /// user-facing <see cref="UnlockUserResult.ErrorMessage"/> describing why.
        /// </returns>
        Task<UnlockUserResult> UnlockUserAsync(string userId);
    }
}
