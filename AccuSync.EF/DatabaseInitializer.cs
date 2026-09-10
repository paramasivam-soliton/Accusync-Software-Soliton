// --------------------------------------------------------------------------------
// <copyright file="DatabaseInitializer.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;
using AccuSync.EF.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF
{
    /// <summary>
    /// EF Core-backed <see cref="IDatabaseInitializer"/> for SettingsDatabase.db — applies
    /// pending migrations and seeds the two default accounts on first run. The general
    /// connector for this database; individual repositories only read/write their entities.
    /// </summary>
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly IDbContextFactory<SettingsDbContext> _contextFactory;
        private readonly IUserService _userService;

        /// <param name="contextFactory">Used to run migrations and check whether any user already exists.</param>
        /// <param name="userService">
        /// Used to seed default accounts — <see cref="IUserService"/> rather than
        /// <see cref="Core.Abstractions.Repositories.IUserRepository"/> directly, so the
        /// seeded passwords are hashed and reversible fields encrypted the same way as
        /// every other account, instead of landing in storage as plaintext.
        /// </param>
        public DatabaseInitializer(IDbContextFactory<SettingsDbContext> contextFactory, IUserService userService)
        {
            _contextFactory = contextFactory;
            _userService = userService;
        }

        /// <summary>
        /// Applies pending migrations — backing up the database file first and restoring it
        /// if the migration fails — then seeds the two default accounts on first run.
        /// Safe to call on every startup.
        /// </summary>
        public async Task InitializeAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

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

            if (!await context.Users.AnyAsync())
            {
                await CreateDefaultUsersAsync();
            }
        }

        /// <summary>
        /// Deletes any row in EF Core's migrations lock table (__EFMigrationsLock), if present.
        /// This app is single-instance, so a row found here at startup is always stale — left
        /// over from a run killed mid-migration. Left in place, MigrateAsync polls for it to
        /// clear forever, bricking every future launch. No-ops on first run, before the table
        /// exists.
        /// </summary>
        private async Task ClearStaleMigrationsLockAsync(SettingsDbContext context)
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
            await _userService.CreateUserAsync(new User
            {
                AccountName = "Admin",
                FirstName = "Admin",
                LastName = "User",
                ProfileId = (int)UserRole.Admin,
                ProfilePassword = "12345"
            });

            await _userService.CreateUserAsync(new User
            {
                AccountName = "Screener",
                FirstName = "Screener",
                LastName = "User",
                ProfileId = (int)UserRole.Screener,
                ProfilePassword = "12345"
            });
        }
    }
}
