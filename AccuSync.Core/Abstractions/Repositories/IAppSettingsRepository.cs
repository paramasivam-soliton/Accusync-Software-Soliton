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
        Task<int> GetLockoutDurationMinutesAsync();

        /// <summary>
        /// Admin-configurable (SRS GID-254911). No dedicated UI yet — the Users/System
        /// Configuration screens stay mock for this epic (LOGIN_EPIC_SPEC.md §2.6) — so
        /// this is the real, testable capability until that UI exists.
        /// </summary>
        Task<bool> SetLockoutDurationMinutesAsync(int minutes);
    }
}
