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
        Task InitializeDatabaseAsync();
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByAccountNameAsync(string accountName);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> CreateUserAsync(User user);

        /// <summary>
        /// Assigns or changes a user's role directly, without requiring the full Users
        /// management UI (which stays out of scope for the Login epic — see
        /// LOGIN_EPIC_SPEC.md §2.6). The new role takes effect on that user's next login,
        /// not live for any session already in progress.
        /// </summary>
        Task<bool> UpdateUserRoleAsync(string userGuid, UserRole role);
    }
}
