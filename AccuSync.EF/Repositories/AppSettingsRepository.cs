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
    /// EF Core-backed data access for app-wide configuration values (a single row —
    /// see <see cref="AppSettings"/> for why it has no exposed identifier to query by).
    /// Nothing here is sensitive, so unlike <see cref="UserRepository"/> there's no
    /// encryption/hashing involved.
    /// </summary>
    public class AppSettingsRepository : IAppSettingsRepository
    {
        private readonly IDbContextFactory<SettingsDbContext> _contextFactory;

        public AppSettingsRepository(IDbContextFactory<SettingsDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<int> GetLockoutDurationMinutesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var settings = await GetOrCreateSettingsAsync(context);
            return settings.LockoutDurationMinutes;
        }

        public async Task<bool> SetLockoutDurationMinutesAsync(int minutes)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var settings = await GetOrCreateSettingsAsync(context);
                settings.LockoutDurationMinutes = minutes;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<AppSettings> GetOrCreateSettingsAsync(SettingsDbContext context)
        {
            var settings = await context.AppSettings.FirstOrDefaultAsync();
            if (settings != null) return settings;

            settings = new AppSettings();
            context.AppSettings.Add(settings);
            await context.SaveChangesAsync();
            return settings;
        }
    }
}
