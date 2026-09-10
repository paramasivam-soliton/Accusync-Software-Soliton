// --------------------------------------------------------------------------------
// <copyright file="UserRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Entities;
using AccuSync.Core.Exceptions;
using AccuSync.EF.Contexts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF
{
    /// <summary>
    /// EF Core-backed plain persistence for user accounts (SettingsDatabase.db). Stores
    /// and returns <see cref="User"/> field values exactly as given — encryption, hashing,
    /// and the username blind-index computation are <see cref="AccuSync.Core.Abstractions.Services.IUserService"/>'s
    /// responsibility, not this repository's.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        // SQLITE_CONSTRAINT_UNIQUE — specifically the UsernameHash unique-index violation,
        // not any other constraint (e.g. a NOT NULL failure, which is genuinely unexpected).
        private const int SqliteUniqueConstraintViolationErrorCode = 2067;

        // SQLITE_CONSTRAINT_FOREIGNKEY — a ProfileId with no matching Profiles row.
        private const int SqliteForeignKeyConstraintViolationErrorCode = 787;

        private readonly IDbContextFactory<SettingsDbContext> _contextFactory;

        public UserRepository(IDbContextFactory<SettingsDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users.AsNoTracking().ToListAsync();
        }

        // Looked up by UsernameHash (a SQL-queryable equality match on the deterministic
        // blind index) rather than the encrypted AccountName column, which can't be
        // compared directly — see IUserService for why.
        public async Task<User> GetUserByUsernameHashAsync(string usernameHash)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UsernameHash == usernameHash);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var tracked = await context.Users.FindAsync(user.Id);
            if (tracked == null)
            {
                throw new UserNotFoundException($"No user found with Id '{user.Id}'.");
            }

            tracked.AccountName = user.AccountName;
            tracked.UsernameHash = user.UsernameHash;
            tracked.FirstName = user.FirstName;
            tracked.LastName = user.LastName;
            tracked.IsActive = user.IsActive;
            tracked.ProfileId = user.ProfileId;
            tracked.ProfilePassword = user.ProfilePassword;
            tracked.FirstLogin = user.FirstLogin;
            tracked.FailedLoginAttemptCount = user.FailedLoginAttemptCount;
            tracked.FailedResetAttemptCount = user.FailedResetAttemptCount;
            tracked.FirstFailedLoginTime = user.FirstFailedLoginTime;
            tracked.FirstResetLoginTime = user.FirstResetLoginTime;
            tracked.PasswordModificationDate = user.PasswordModificationDate;
            tracked.LastThreePasswords = user.LastThreePasswords;
            // ModificationDate is set by TimestampInterceptor, not here — see class TODO history.

            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // Renaming this account to a username already taken by a different
                // account — an expected outcome (username collision), not a system failure.
                return false;
            }
            // TODO: Any other exception here (disk I/O, a locked/corrupted database file,
            //       etc.) is genuinely unexpected. It is intentionally left to propagate
            //       to the caller instead of being swallowed — log it here once a logger
            //       is introduced into the solution.
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            var entity = new User
            {
                Id = user.Id,
                AccountName = user.AccountName,
                UsernameHash = user.UsernameHash,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                ProfileId = user.ProfileId,
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

            using var context = await _contextFactory.CreateDbContextAsync();
            context.Users.Add(entity);

            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // Username already taken — an expected outcome, not a system failure.
                return false;
            }
            // TODO: Any other exception here is genuinely unexpected and is intentionally
            //       left to propagate to the caller instead of being swallowed — log it
            //       here once a logger is introduced into the solution.
        }

        // Narrow, single-field update — a role change is not a disguised full profile
        // overwrite.
        public async Task<bool> UpdateUserProfileIdAsync(string userId, int profileId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var tracked = await context.Users.FindAsync(userId);
            if (tracked == null)
            {
                throw new UserNotFoundException($"No user found with Id '{userId}'.");
            }

            tracked.ProfileId = profileId;

            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex) when (IsForeignKeyConstraintViolation(ex))
            {
                // profileId doesn't match any row in Profiles — an invalid role
                // assignment, not a system failure.
                return false;
            }
            // TODO: Any other exception here is genuinely unexpected and is intentionally
            //       left to propagate to the caller instead of being swallowed — log it
            //       here once a logger is introduced into the solution.
        }

        // Narrow, single-field update — mirrors UpdateUserProfileIdAsync. No encryption
        // involved (IsActive is a plain bool), but still routed only through IUserService
        // so no caller reaches this repository directly.
        public async Task<bool> SetUserActiveStatusAsync(string userId, bool isActive)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var tracked = await context.Users.FindAsync(userId);
            if (tracked == null)
            {
                throw new UserNotFoundException($"No user found with Id '{userId}'.");
            }

            tracked.IsActive = isActive;
            await context.SaveChangesAsync();
            return true;
        }

        // Narrow, single-field-pair update — clears lockout state without touching any
        // other field (name, role, etc.). No encryption involved.
        public async Task<bool> UnlockUserAsync(string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var tracked = await context.Users.FindAsync(userId);
            if (tracked == null)
            {
                throw new UserNotFoundException($"No user found with Id '{userId}'.");
            }

            tracked.FailedLoginAttemptCount = 0;
            tracked.FirstFailedLoginTime = 0L;
            await context.SaveChangesAsync();
            return true;
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqliteException sqliteEx
                && sqliteEx.SqliteExtendedErrorCode == SqliteUniqueConstraintViolationErrorCode;
        }

        private static bool IsForeignKeyConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqliteException sqliteEx
                && sqliteEx.SqliteExtendedErrorCode == SqliteForeignKeyConstraintViolationErrorCode;
        }
    }
}
