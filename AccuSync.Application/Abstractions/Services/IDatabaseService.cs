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
        Task InitializeDatabaseAsync();
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByAccountNameAsync(string accountName);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> CreateUserAsync(User user);
    }
}
