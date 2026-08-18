// --------------------------------------------------------------------------------
// <copyright file="DatabaseInitializer.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;
using AccuSync.EF.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF
{
    /// <summary>
    /// EF Core-backed <see cref="IDatabaseInitializer"/> for both SettingsDatabase.db and
    /// PatientDatabase.db — applies pending migrations to each, backing up and restoring on
    /// failure, then seeds the two default accounts on first run. The general connector for
    /// these databases; individual repositories only read/write their entities. Registered as
    /// a single instance so <c>SplashViewModel</c>'s one "Initializing database…" step covers
    /// both databases, per the acceptance criteria.
    /// </summary>
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly IDbContextFactory<SettingsDbContext> _settingsContextFactory;
        private readonly IDbContextFactory<PatientDbContext> _patientContextFactory;
        private readonly IUserRepository _userRepository;

        /// <summary>Creates the initializer backed by the given context factories and user repository.</summary>
        public DatabaseInitializer(
            IDbContextFactory<SettingsDbContext> settingsContextFactory,
            IDbContextFactory<PatientDbContext> patientContextFactory,
            IUserRepository userRepository)
        {
            _settingsContextFactory = settingsContextFactory;
            _patientContextFactory = patientContextFactory;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Applies pending migrations to both databases — backing up each database file first
        /// and restoring it if its migration fails — then seeds the two default accounts on
        /// first run. Safe to call on every startup.
        /// </summary>
        public async Task InitializeAsync()
        {
            using var settingsContext = await _settingsContextFactory.CreateDbContextAsync();
            using var patientContext = await _patientContextFactory.CreateDbContextAsync();

            await MigrateWithBackupAsync(settingsContext);
            await MigrateWithBackupAsync(patientContext);

            if (!await settingsContext.Users.AnyAsync())
            {
                await CreateDefaultUsersAsync();
            }
        }

        /// <summary>
        /// Applies pending migrations for the given context — backing up the database file
        /// first and restoring it if the migration fails.
        /// </summary>
        private static async Task MigrateWithBackupAsync(DbContext context)
        {
            string databasePath = context.Database.GetDbConnection().DataSource;
            bool databaseExisted = File.Exists(databasePath);
            string backupPath = databasePath + ".bak";

            if (databaseExisted)
            {
                File.Copy(databasePath, backupPath, overwrite: true);
            }

            try
            {
                await ClearStaleMigrationsLockAsync(context);
                await context.Database.MigrateAsync();
            }
            catch
            {
                if (databaseExisted)
                {
                    File.Copy(backupPath, databasePath, overwrite: true);
                }

                throw;
            }
        }

        /// <summary>
        /// Deletes any row in EF Core's migrations lock table (__EFMigrationsLock), if present.
        /// This app is single-instance, so a row found here at startup is always stale — left
        /// over from a run killed mid-migration. Left in place, MigrateAsync polls for it to
        /// clear forever, bricking every future launch. No-ops on first run, before the table
        /// exists.
        /// </summary>
        private static async Task ClearStaleMigrationsLockAsync(DbContext context)
        {
            bool tableExists = await context.Database.SqlQueryRaw<string>(
                "SELECT name FROM sqlite_master WHERE type = 'table' AND name = '__EFMigrationsLock'")
                .AnyAsync();

            if (tableExists)
            {
                await context.Database.ExecuteSqlRawAsync("DELETE FROM \"__EFMigrationsLock\";");
            }
        }

        // TODO: Default password "12345" is hardcoded. These accounts should
        //       force a password change on first login (FirstLogin = 1 handles
        //       this, but verify the UI enforces it).
        private async Task CreateDefaultUsersAsync()
        {
            await _userRepository.CreateUserAsync(new User
            {
                AccountName = "Admin",
                FirstName = "Admin",
                LastName = "User",
                ProfileId = "Admin",
                ProfilePassword = "12345"
            });

            await _userRepository.CreateUserAsync(new User
            {
                AccountName = "Screener",
                FirstName = "Screener",
                LastName = "User",
                ProfileId = "Screener",
                ProfilePassword = "12345"
            });
        }
    }
}
