// --------------------------------------------------------------------------------
// <copyright file="UserRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;
using AccuSync.EF.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF
{
    /// <summary>
    /// EF Core-backed data access for user accounts (SettingsDatabase.db). Sensitive fields
    /// (names, account names, previous passwords) are encrypted via <see cref="IEncryptionService"/>
    /// before storage and decrypted on read. ProfilePassword travels encrypted end-to-end —
    /// it is never decrypted back onto a <see cref="User"/> instance.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IDbContextFactory<SettingsDbContext> _contextFactory;
        private readonly IEncryptionService _encryptionService;

        public UserRepository(IDbContextFactory<SettingsDbContext> contextFactory, IEncryptionService encryptionService)
        {
            _contextFactory = contextFactory;
            _encryptionService = encryptionService;
        }

        // NOTE: Encryption is applied per-field here rather than in the User model.
        //       This means callers must always go through UserRepository — if anyone
        //       queries the database directly, they'll get encrypted values.
        //       ProfilePassword is assumed already encrypted by the caller.
        private async Task InsertUserAsync(User user)
        {
            var entity = new User
            {
                Guid = user.Guid,
                AccountName = _encryptionService.Encrypt(user.AccountName),
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
                LastThreePasswords = _encryptionService.Encrypt(user.LastThreePasswords)
                // CreationDate/ModificationDate are set by TimestampInterceptor on save.
            };

            using var context = await _contextFactory.CreateDbContextAsync();
            context.Users.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var stored = await context.Users.AsNoTracking().ToListAsync();
            return stored.Select(Decrypt).ToList();
        }

        // BUG: Decrypts every user just to find one by name. On top of being
        //      slow at scale, encrypted AccountName can't be queried with SQL WHERE,
        //      so this will always be a full table scan + decrypt.
        //      Consider storing a hash of AccountName alongside the encrypted value
        //      for indexed lookups.
        public async Task<User> GetUserByAccountNameAsync(string accountName)
        {
            var allUsers = await GetAllUsersAsync();
            return allUsers.FirstOrDefault(u =>
                u.AccountName.Equals(accountName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var tracked = await context.Users.FindAsync(user.Guid);
                if (tracked == null) return false;

                tracked.AccountName = _encryptionService.Encrypt(user.AccountName);
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
                tracked.LastThreePasswords = _encryptionService.Encrypt(user.LastThreePasswords);
                // ModificationDate is set by TimestampInterceptor, not here — see class TODO history.

                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                // TODO: Silent failure — caller has no way to know what went wrong.
                //       At minimum log the exception.
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
                    ProfilePassword = _encryptionService.Encrypt(user.ProfilePassword),
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
                LastThreePasswords = _encryptionService.Decrypt(stored.LastThreePasswords)
            };
        }
    }
}
