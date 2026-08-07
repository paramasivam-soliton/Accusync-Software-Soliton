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
        /// Assigns or changes a user's role directly, without requiring a full Users
        /// management UI. The new role takes effect on that user's next login, not
        /// live for any session already in progress.
        /// </summary>
        Task<bool> UpdateUserRoleAsync(string userGuid, UserRole role);

        /// <summary>
        /// Activates or deactivates a user account directly, without requiring the full
        /// Users management UI (out of scope for the Login epic — see
        /// LOGIN_EPIC_SPEC.md §2.6). Deactivated accounts are blocked from authenticating
        /// regardless of password correctness (see <c>AuthenticationService</c>).
        /// </summary>
        Task<bool> SetUserActiveStatusAsync(string userGuid, bool isActive);
    }
}
