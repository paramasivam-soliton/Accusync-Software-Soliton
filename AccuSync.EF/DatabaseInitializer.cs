// --------------------------------------------------------------------------------
// <copyright file="DatabaseInitializer.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Data.Common;
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
    /// EF Core-backed <see cref="IDatabaseInitializer"/> for SettingsDatabase.db — applies
    /// pending migrations and seeds the two default accounts on first run. The general
    /// connector for this database; individual repositories only read/write their entities.
    /// </summary>
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly SettingsDbContext _context;
        private readonly IUserRepository _userRepository;

        public DatabaseInitializer(SettingsDbContext context, IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Applies pending migrations — backing up the database file first and restoring it
        /// if the migration fails — then seeds the two default accounts on first run.
        /// Safe to call on every startup.
        /// </summary>
        public async Task InitializeAsync()
        {
            string databasePath = _context.Database.GetDbConnection().DataSource;
            bool databaseExisted = File.Exists(databasePath);
            string backupPath = databasePath + ".bak";

            if (databaseExisted)
            {
                File.Copy(databasePath, backupPath, overwrite: true);
            }

            try
            {
                // EF Core's migration lock (__EFMigrationsLock) exists to stop two
                // *concurrent* instances from migrating at once. This app is single-instance
                // desktop software, so any row found here at startup is not a live lock —
                // it's a leftover from a previous run that was killed mid-migration (crash,
                // force-quit, debugger stop). Left alone, MigrateAsync polls for that row to
                // clear roughly once a second, forever, since the process that owned it is
                // gone — bricking every future launch until someone edits the database file
                // by hand. Clearing it first is what makes migration recoverable from a
                // mid-migration interruption instead of a one-way failure.
                await ClearStaleMigrationsLockAsync();
                await _context.Database.MigrateAsync();
            }
            catch
            {
                if (databaseExisted)
                {
                    File.Copy(backupPath, databasePath, overwrite: true);
                }

                throw;
            }

            if (!await _context.Users.AnyAsync())
            {
                await CreateDefaultUsersAsync();
            }
        }

        /// <summary>
        /// Deletes any row in EF Core's migrations lock table. See the comment at the
        /// InitializeAsync call site for why this is safe and necessary here.
        /// </summary>
        private async Task ClearStaleMigrationsLockAsync()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"__EFMigrationsLock\";");
            }
            catch (DbException)
            {
                // Table doesn't exist yet — this is the very first run, nothing to clear.
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
