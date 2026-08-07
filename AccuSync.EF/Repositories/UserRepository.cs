// --------------------------------------------------------------------------------
// <copyright file="UserRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;
using AccuSync.EF.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF
{
    /// <summary>
    /// EF Core-backed data access for user accounts (SettingsDatabase.db). Names/account
    /// names are encrypted via <see cref="IEncryptionService"/> before storage and decrypted
    /// on read. Passwords are one-way hashed via <see cref="IPasswordHasher"/> — ProfilePassword
    /// and LastThreePasswords travel as hashes end-to-end and are never reversed. AccountName's
    /// encryption is non-deterministic, so uniqueness/lookup is carried by UsernameHash — a
    /// deterministic SHA-256 of the normalized username (see ComputeUsernameHash) — instead of
    /// the encrypted column itself (a "blind index" pattern).
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly SettingsDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly IPasswordHasher _passwordHasher;

        public UserRepository(SettingsDbContext context, IEncryptionService encryptionService, IPasswordHasher passwordHasher)
        {
            _context = context;
            _encryptionService = encryptionService;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Applies pending migrations — backing up the database file first and restoring it
        /// if the migration fails — then seeds the two default accounts on first run.
        /// Safe to call on every startup.
        /// </summary>
        public async Task InitializeDatabaseAsync()
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
        /// InitializeDatabaseAsync call site for why this is safe and necessary here.
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

        // FirstLogin = 1 forces a password change on first login (see
        // ChangePasswordViewModel), so the seeded "12345" is only ever a
        // one-time credential.
        private async Task CreateDefaultUsersAsync()
        {
            await InsertUserAsync(new User
            {
                AccountName = "Admin",
                FirstName = "Admin",
                LastName = "User",
                ProfileId = "Admin",
                ProfilePassword = _passwordHasher.Hash("12345")
            });

            await InsertUserAsync(new User
            {
                AccountName = "Screener",
                FirstName = "Screener",
                LastName = "User",
                ProfileId = "Screener",
                ProfilePassword = _passwordHasher.Hash("12345")
            });
        }

        // NOTE: Encryption is applied per-field here rather than in the User model.
        //       This means callers must always go through UserRepository — if anyone
        //       queries the database directly, they'll get encrypted values.
        //       ProfilePassword/LastThreePasswords are assumed already hashed by the caller.
        private async Task InsertUserAsync(User user)
        {
            var entity = new User
            {
                Guid = user.Guid,
                AccountName = _encryptionService.Encrypt(user.AccountName),
                UsernameHash = ComputeUsernameHash(user.AccountName),
                FirstName = _encryptionService.Encrypt(user.FirstName),
                LastName = _encryptionService.Encrypt(user.LastName),
                Status = user.Status,
                ProfileId = _encryptionService.Encrypt(user.ProfileId),
                ProfilePassword = user.ProfilePassword,
                FirstLogin = user.FirstLogin,
                FailedLoginAttemptCount = user.FailedLoginAttemptCount,
                FailedResetAttemptCount = user.FailedResetAttemptCount,
                FirstFailedLoginTime = user.FirstFailedLoginTime,
                FirstResetLoginTime = user.FirstResetLoginTime,
                PasswordModificationDate = user.PasswordModificationDate,
                LastThreePasswords = user.LastThreePasswords
                // CreationDate/ModificationDate are set by TimestampInterceptor on save.
            };

            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deterministic SHA-256 hash of the normalized (trimmed, lowercased) username.
        /// Used only for uniqueness enforcement and login lookup — AccountName itself is
        /// encrypted non-deterministically (see <see cref="IEncryptionService"/>) and can
        /// no longer be compared directly (a "blind index" pattern).
        /// </summary>
        private static string ComputeUsernameHash(string accountName)
        {
            string normalized = (accountName ?? string.Empty).Trim().ToLowerInvariant();
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            return Convert.ToBase64String(hash);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var stored = await _context.Users.AsNoTracking().ToListAsync();
            return stored.Select(Decrypt).ToList();
        }

        // Looked up by UsernameHash (a SQL-queryable equality match on the deterministic
        // blind index) rather than decrypting every row — AccountName's encryption is
        // non-deterministic and can't be compared directly. Only the matched row is decrypted.
        public async Task<User> GetUserByAccountNameAsync(string accountName)
        {
            string usernameHash = ComputeUsernameHash(accountName);
            var stored = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UsernameHash == usernameHash);
            return stored == null ? null : Decrypt(stored);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var tracked = await _context.Users.FindAsync(user.Guid);
                if (tracked == null) return false;

                tracked.AccountName = _encryptionService.Encrypt(user.AccountName);
                tracked.UsernameHash = ComputeUsernameHash(user.AccountName);
                tracked.FirstName = _encryptionService.Encrypt(user.FirstName);
                tracked.LastName = _encryptionService.Encrypt(user.LastName);
                tracked.Status = user.Status;
                tracked.ProfileId = _encryptionService.Encrypt(user.ProfileId);
                tracked.ProfilePassword = user.ProfilePassword;
                tracked.FirstLogin = user.FirstLogin;
                tracked.FailedLoginAttemptCount = user.FailedLoginAttemptCount;
                tracked.FailedResetAttemptCount = user.FailedResetAttemptCount;
                tracked.FirstFailedLoginTime = user.FirstFailedLoginTime;
                tracked.FirstResetLoginTime = user.FirstResetLoginTime;
                tracked.PasswordModificationDate = user.PasswordModificationDate;
                tracked.LastThreePasswords = user.LastThreePasswords;
                // ModificationDate is set by TimestampInterceptor, not here — see class TODO history.

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                // TODO: Silent failure — caller has no way to know what went wrong.
                //       At minimum log the exception.
                return false;
            }
        }

        public async Task<bool> UpdateUserRoleAsync(string userGuid, UserRole role)
        {
            try
            {
                var tracked = await _context.Users.FindAsync(userGuid);
                if (tracked == null) return false;

                tracked.ProfileId = _encryptionService.Encrypt(role.ToString());

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetUserActiveStatusAsync(string userGuid, bool isActive)
        {
            try
            {
                var tracked = await _context.Users.FindAsync(userGuid);
                if (tracked == null) return false;

                tracked.Status = isActive;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                await InsertUserAsync(new User
                {
                    Guid = user.Guid,
                    AccountName = user.AccountName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Status = user.Status,
                    ProfileId = user.ProfileId,
                    ProfilePassword = _passwordHasher.Hash(user.ProfilePassword),
                    FirstLogin = user.FirstLogin,
                    FailedLoginAttemptCount = user.FailedLoginAttemptCount,
                    FailedResetAttemptCount = user.FailedResetAttemptCount,
                    FirstFailedLoginTime = user.FirstFailedLoginTime,
                    FirstResetLoginTime = user.FirstResetLoginTime,
                    PasswordModificationDate = user.PasswordModificationDate,
                    LastThreePasswords = user.LastThreePasswords
                });
                return true;
            }
            catch
            {
                // TODO: Silent failure — same concern as UpdateUserAsync.
                return false;
            }
        }

        private User Decrypt(User stored)
        {
            return new User
            {
                Guid = stored.Guid,
                AccountName = _encryptionService.Decrypt(stored.AccountName),
                UsernameHash = stored.UsernameHash,
                FirstName = _encryptionService.Decrypt(stored.FirstName),
                LastName = _encryptionService.Decrypt(stored.LastName),
                Status = stored.Status,
                ProfileId = _encryptionService.Decrypt(stored.ProfileId),
                ProfilePassword = stored.ProfilePassword,
                FirstLogin = stored.FirstLogin,
                FailedLoginAttemptCount = stored.FailedLoginAttemptCount,
                FailedResetAttemptCount = stored.FailedResetAttemptCount,
                FirstFailedLoginTime = stored.FirstFailedLoginTime,
                FirstResetLoginTime = stored.FirstResetLoginTime,
                CreationDate = stored.CreationDate,
                ModificationDate = stored.ModificationDate,
                PasswordModificationDate = stored.PasswordModificationDate,
                LastThreePasswords = stored.LastThreePasswords
            };
        }
    }
}
