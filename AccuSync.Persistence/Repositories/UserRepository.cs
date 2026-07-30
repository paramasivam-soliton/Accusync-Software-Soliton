// --------------------------------------------------------------------------------
// <copyright file="UserRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AccuSync.Application.Abstractions.Repositories;
using AccuSync.Application.Abstractions.Services;
using AccuSync.Application.Models;
using Microsoft.Data.Sqlite;

namespace AccuSync.Persistence
{
    /// <summary>
    /// SQLite-backed data access for user accounts.
    /// Database is stored in <c>ProgramData\Natus\AccuSync\SettingsDatabase.db</c>.
    /// Sensitive fields (names, account names, passwords) are encrypted via
    /// <see cref="IEncryptionService"/> before storage.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly string _databasePath;
        private readonly IEncryptionService _encryptionService;
        private readonly string _connectionString;

        public UserRepository(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;

            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Natus",
                "AccuSync"
            );

            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "SettingsDatabase.db");
            _connectionString = $"Data Source={_databasePath}";
        }

        /// <summary>
        /// Creates the Users table if it doesn't exist and seeds default accounts
        /// on first run. Safe to call on every startup.
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            bool isNewDatabase = !File.Exists(_databasePath);

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Guid TEXT PRIMARY KEY,
                        AccountName TEXT NOT NULL UNIQUE,
                        FirstName TEXT,
                        LastName TEXT,
                        Status INTEGER DEFAULT 0,
                        ProfileId TEXT,
                        ProfilePassword TEXT NOT NULL,
                        FirstLogin INTEGER DEFAULT 1,
                        FailedLoginAttemptCount INTEGER DEFAULT 0,
                        FailedResetAttemptCount INTEGER DEFAULT 0,
                        FirstFailedLoginTime INTEGER DEFAULT 0,
                        FirstResetLoginTime INTEGER DEFAULT 0,
                        CreationDate INTEGER DEFAULT 0,
                        ModificationDate INTEGER DEFAULT 0,
                        PasswordModificationDate INTEGER DEFAULT 0,
                        LastThreePasswords TEXT DEFAULT ''
                    )";

                using (var command = new SqliteCommand(createTableQuery, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                if (isNewDatabase)
                {
                    await CreateDefaultUsersAsync(connection);
                }
            }
        }

        // TODO: Default password "12345" is hardcoded. These accounts should
        //       force a password change on first login (FirstLogin = 1 handles
        //       this, but verify the UI enforces it).
        private async Task CreateDefaultUsersAsync(SqliteConnection connection)
        {
            var adminUser = new User
            {
                AccountName = "Admin",
                FirstName = "Admin",
                LastName = "User",
                ProfileId = "Admin",
                ProfilePassword = _encryptionService.Encrypt("12345")
            };

            await InsertUserAsync(connection, adminUser);

            var screenerUser = new User
            {
                AccountName = "Screener",
                FirstName = "Screener",
                LastName = "User",
                ProfileId = "Screener",
                ProfilePassword = _encryptionService.Encrypt("12345")
            };

            await InsertUserAsync(connection, screenerUser);
        }

        // NOTE: Encryption is applied per-field here rather than in the User model.
        //       This means callers must always go through UserRepository — if anyone
        //       writes raw SQL against the same database, they'll get encrypted values.
        //       That's intentional, but worth knowing.
        private async Task InsertUserAsync(SqliteConnection connection, User user)
        {
            string insertQuery = @"
                INSERT INTO Users (
                    Guid, AccountName, FirstName, LastName, Status, ProfileId,
                    ProfilePassword, FirstLogin, FailedLoginAttemptCount,
                    FailedResetAttemptCount, FirstFailedLoginTime, FirstResetLoginTime,
                    CreationDate, ModificationDate, PasswordModificationDate, LastThreePasswords
                ) VALUES (
                    @Guid, @AccountName, @FirstName, @LastName, @Status, @ProfileId,
                    @ProfilePassword, @FirstLogin, @FailedLoginAttemptCount,
                    @FailedResetAttemptCount, @FirstFailedLoginTime, @FirstResetLoginTime,
                    @CreationDate, @ModificationDate, @PasswordModificationDate, @LastThreePasswords
                )";

            using (var command = new SqliteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@Guid", user.Guid);
                command.Parameters.AddWithValue("@AccountName", _encryptionService.Encrypt(user.AccountName));
                command.Parameters.AddWithValue("@FirstName", _encryptionService.Encrypt(user.FirstName));
                command.Parameters.AddWithValue("@LastName", _encryptionService.Encrypt(user.LastName));
                command.Parameters.AddWithValue("@Status", user.Status);
                command.Parameters.AddWithValue("@ProfileId", _encryptionService.Encrypt(user.ProfileId));
                command.Parameters.AddWithValue("@ProfilePassword", user.ProfilePassword);
                command.Parameters.AddWithValue("@FirstLogin", user.FirstLogin);
                command.Parameters.AddWithValue("@FailedLoginAttemptCount", user.FailedLoginAttemptCount);
                command.Parameters.AddWithValue("@FailedResetAttemptCount", user.FailedResetAttemptCount);
                command.Parameters.AddWithValue("@FirstFailedLoginTime", user.FirstFailedLoginTime);
                command.Parameters.AddWithValue("@FirstResetLoginTime", user.FirstResetLoginTime);
                command.Parameters.AddWithValue("@CreationDate", user.CreationDate);
                command.Parameters.AddWithValue("@ModificationDate", user.ModificationDate);
                command.Parameters.AddWithValue("@PasswordModificationDate", user.PasswordModificationDate);
                command.Parameters.AddWithValue("@LastThreePasswords", _encryptionService.Encrypt(user.LastThreePasswords));

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT * FROM Users";
                using (var command = new SqliteCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(MapUser(reader));
                    }
                }
            }

            return users;
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
                using (var connection = new SqliteConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string updateQuery = @"
                        UPDATE Users SET
                            AccountName = @AccountName,
                            FirstName = @FirstName,
                            LastName = @LastName,
                            Status = @Status,
                            ProfileId = @ProfileId,
                            ProfilePassword = @ProfilePassword,
                            FirstLogin = @FirstLogin,
                            FailedLoginAttemptCount = @FailedLoginAttemptCount,
                            FailedResetAttemptCount = @FailedResetAttemptCount,
                            FirstFailedLoginTime = @FirstFailedLoginTime,
                            FirstResetLoginTime = @FirstResetLoginTime,
                            ModificationDate = @ModificationDate,
                            PasswordModificationDate = @PasswordModificationDate,
                            LastThreePasswords = @LastThreePasswords
                        WHERE Guid = @Guid";

                    using (var command = new SqliteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Guid", user.Guid);
                        command.Parameters.AddWithValue("@AccountName", _encryptionService.Encrypt(user.AccountName));
                        command.Parameters.AddWithValue("@FirstName", _encryptionService.Encrypt(user.FirstName));
                        command.Parameters.AddWithValue("@LastName", _encryptionService.Encrypt(user.LastName));
                        command.Parameters.AddWithValue("@Status", user.Status);
                        command.Parameters.AddWithValue("@ProfileId", _encryptionService.Encrypt(user.ProfileId));
                        command.Parameters.AddWithValue("@ProfilePassword", user.ProfilePassword);
                        command.Parameters.AddWithValue("@FirstLogin", user.FirstLogin);
                        command.Parameters.AddWithValue("@FailedLoginAttemptCount", user.FailedLoginAttemptCount);
                        command.Parameters.AddWithValue("@FailedResetAttemptCount", user.FailedResetAttemptCount);
                        command.Parameters.AddWithValue("@FirstFailedLoginTime", user.FirstFailedLoginTime);
                        command.Parameters.AddWithValue("@FirstResetLoginTime", user.FirstResetLoginTime);
                        // ModificationDate is set here rather than using the caller's value
                        command.Parameters.AddWithValue("@ModificationDate", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                        command.Parameters.AddWithValue("@PasswordModificationDate", user.PasswordModificationDate);
                        command.Parameters.AddWithValue("@LastThreePasswords", _encryptionService.Encrypt(user.LastThreePasswords));

                        await command.ExecuteNonQueryAsync();
                    }
                }
                return true;
            }
            catch
            {
                // TODO: Silent failure — caller has no way to know what went wrong.
                //       At minimum log the exception.
                return false;
            }
        }

        // BUG: Mutates user.ProfilePassword in place before inserting.
        //      The caller's User object now holds the encrypted password,
        //      which can cause double-encryption if CreateUserAsync is retried
        //      or the object is reused.
        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    user.ProfilePassword = _encryptionService.Encrypt(user.ProfilePassword);
                    await InsertUserAsync(connection, user);
                }
                return true;
            }
            catch
            {
                // TODO: Silent failure — same concern as UpdateUserAsync.
                return false;
            }
        }

        // TODO: MapUser reads columns by ordinal position (0, 1, 2...) which breaks
        //       silently if the table schema changes. Use reader.GetOrdinal("ColumnName")
        //       or reader["ColumnName"] for safety.
        private User MapUser(SqliteDataReader reader)
        {
            return new User
            {
                Guid = reader.GetString(0),
                AccountName = _encryptionService.Decrypt(reader.GetString(1)),
                FirstName = _encryptionService.Decrypt(reader.GetString(2)),
                LastName = _encryptionService.Decrypt(reader.GetString(3)),
                Status = reader.GetInt32(4),
                ProfileId = _encryptionService.Decrypt(reader.GetString(5)),
                ProfilePassword = reader.GetString(6),
                FirstLogin = reader.GetInt32(7),
                FailedLoginAttemptCount = reader.GetInt32(8),
                FailedResetAttemptCount = reader.GetInt32(9),
                FirstFailedLoginTime = reader.GetInt64(10),
                FirstResetLoginTime = reader.GetInt64(11),
                CreationDate = reader.GetInt64(12),
                ModificationDate = reader.GetInt64(13),
                PasswordModificationDate = reader.GetInt64(14),
                LastThreePasswords = _encryptionService.Decrypt(reader.GetString(15))
            };
        }
    }
}
