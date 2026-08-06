// --------------------------------------------------------------------------------
// <copyright file="AppSettingsRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading.Tasks;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Entities;
using AccuSync.EF.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF
{
    /// <summary>
    /// EF Core-backed data access for app-wide configuration values (single row,
    /// identified by <see cref="AppSettings.Id"/>). Nothing here is sensitive, so
    /// unlike <see cref="UserRepository"/> there's no encryption/hashing involved.
    /// </summary>
    public class AppSettingsRepository : IAppSettingsRepository
    {
        private const string SettingsRowId = "Default";

        private readonly SettingsDbContext _context;

        public AppSettingsRepository(SettingsDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetLockoutDurationMinutesAsync()
        {
            var settings = await GetOrCreateSettingsAsync();
            return settings.LockoutDurationMinutes;
        }

        public async Task<bool> SetLockoutDurationMinutesAsync(int minutes)
        {
            try
            {
                var settings = await GetOrCreateSettingsAsync();
                settings.LockoutDurationMinutes = minutes;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<AppSettings> GetOrCreateSettingsAsync()
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync(s => s.Id == SettingsRowId);
            if (settings != null) return settings;

            settings = new AppSettings();
            _context.AppSettings.Add(settings);
            await _context.SaveChangesAsync();
            return settings;
        }
    }
}
