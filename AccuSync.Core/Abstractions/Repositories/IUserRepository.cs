// --------------------------------------------------------------------------------
// <copyright file="IUserRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using AccuSync.Core.Entities;

namespace AccuSync.Core.Abstractions.Repositories
{
    /// <summary>
    /// Persistence contract for user accounts. Implemented by AccuSync.EF
    /// (EF Core, SQLite-backed).
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>Returns every user account.</summary>
        Task<List<User>> GetAllUsersAsync();

        /// <summary>Looks up a single user by account name (case-insensitive).</summary>
        Task<User> GetUserByAccountNameAsync(string accountName);

        /// <summary>Persists changes to an existing user.</summary>
        Task<bool> UpdateUserAsync(User user);

        /// <summary>Creates a new user account.</summary>
        Task<bool> CreateUserAsync(User user);

        /// <summary>
        /// Assigns or changes a user's role directly, without requiring a full Users
        /// management UI. The new role takes effect on that user's next login, not
        /// live for any session already in progress.
        /// </summary>
        Task<bool> UpdateUserRoleAsync(string userGuid, UserRole role);
    }
}
