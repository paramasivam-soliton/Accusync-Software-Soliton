// --------------------------------------------------------------------------------
// <copyright file="IDatabaseService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using AccuSync.Application.Models;

namespace AccuSync.Application.Abstractions.Services
{
    /// <summary>
    /// Persistence contract for user accounts. Implemented by
    /// AccuSync.Application.Services.DatabaseService (currently SQLite-backed;
    /// a future EF Core implementation only needs to satisfy this same contract).
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// Creates the underlying storage (tables, default accounts, etc.) if it
        /// doesn't already exist. Safe to call on every startup.
        /// </summary>
        Task InitializeDatabaseAsync();

        /// <summary>
        /// Returns every stored user account.
        /// </summary>
        /// <returns>A list of all <see cref="User"/> records.</returns>
        Task<List<User>> GetAllUsersAsync();

        /// <summary>
        /// Looks up a user by account name.
        /// </summary>
        /// <param name="accountName">The account name to search for.</param>
        /// <returns>The matching <see cref="User"/>, or <c>null</c> if none is found.</returns>
        Task<User> GetUserByAccountNameAsync(string accountName);

        /// <summary>
        /// Persists changes to an existing user.
        /// </summary>
        /// <param name="user">The user to update.</param>
        /// <returns><c>true</c> if the update succeeded; otherwise <c>false</c>.</returns>
        Task<bool> UpdateUserAsync(User user);

        /// <summary>
        /// Creates a new user account.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <returns><c>true</c> if the user was created; otherwise <c>false</c>.</returns>
        Task<bool> CreateUserAsync(User user);
    }
}
