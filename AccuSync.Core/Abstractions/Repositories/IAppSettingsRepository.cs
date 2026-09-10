// --------------------------------------------------------------------------------
// <copyright file="IAppSettingsRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace AccuSync.Core.Abstractions.Repositories
{
    /// <summary>
    /// Persistence contract for app-wide configuration values. Implemented by
    /// AccuSync.EF (EF Core, SQLite-backed).
    /// </summary>
    public interface IAppSettingsRepository
    {
        /// <summary>Returns the admin-configured account lockout duration, seeding the default if no row exists yet.</summary>
        /// <returns>The lockout duration, in minutes.</returns>
        Task<int> GetLockoutDurationMinutesAsync();

        /// <summary>
        /// Persists the admin-configured account lockout duration.
        /// </summary>
        /// <param name="minutes">The new lockout duration, in minutes.</param>
        /// <returns><c>true</c> if the update succeeded.</returns>
        Task<bool> SetLockoutDurationMinutesAsync(int minutes);
    }
}
