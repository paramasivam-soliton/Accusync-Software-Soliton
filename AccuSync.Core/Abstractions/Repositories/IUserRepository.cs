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
    }
}
