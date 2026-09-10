// --------------------------------------------------------------------------------
// <copyright file="UserService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AccuSync.Application.Resources;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;
using AccuSync.Core.Exceptions;

namespace AccuSync.Application.Services.Users
{
    /// <summary>
    /// User account use cases. Names/account names are encrypted via <see cref="IEncryptionService"/>
    /// before storage and decrypted on read. Passwords are one-way hashed via <see cref="IPasswordHasher"/>
    /// — ProfilePassword and LastThreePasswords travel as hashes end-to-end and are never reversed.
    /// AccountName's encryption is non-deterministic, so uniqueness/lookup is carried by UsernameHash — a
    /// deterministic SHA-256 of the normalized username — instead of the encrypted column itself
    /// (blind index pattern). <see cref="IUserRepository"/> stores whatever it is given as-is.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IPasswordHasher _passwordHasher;

        /// <summary>
        /// Creates the service with its persistence, encryption, and password-hashing dependencies.
        /// </summary>
        /// <param name="userRepository">Used to store and retrieve user records in their storage form.</param>
        /// <param name="encryptionService">Used to encrypt/decrypt reversible fields (account name, first/last name).</param>
        /// <param name="passwordHasher">Used to hash a new user's password.</param>
        public UserService(IUserRepository userRepository, IEncryptionService encryptionService, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _encryptionService = encryptionService;
            _passwordHasher = passwordHasher;
        }

        /// <inheritdoc/>
        public async Task<List<User>> GetAllUsersAsync()
        {
            var stored = await _userRepository.GetAllUsersAsync();
            return stored.Select(Decrypt).ToList();
        }

        /// <inheritdoc/>
        public async Task<User> GetUserByAccountNameAsync(string accountName)
        {
            string usernameHash = ComputeUsernameHash(accountName);
            var stored = await _userRepository.GetUserByUsernameHashAsync(usernameHash);
            return stored == null ? null : Decrypt(stored);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                return await _userRepository.UpdateUserAsync(Encrypt(user, hashPassword: false));
            }
            catch (UserNotFoundException)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public Task<bool> CreateUserAsync(User user)
        {
            return _userRepository.CreateUserAsync(Encrypt(user, hashPassword: true));
        }

        /// <inheritdoc/>
        public async Task<RoleUpdateResult> UpdateUserRoleAsync(string userId, UserRole role)
        {
            try
            {
                bool success = await _userRepository.UpdateUserProfileIdAsync(userId, (int)role);
                return success
                    ? new RoleUpdateResult { Success = true }
                    : new RoleUpdateResult { Success = false, ErrorMessage = Strings.UserService_RoleAssignmentFailed };
            }
            catch (UserNotFoundException)
            {
                return new RoleUpdateResult { Success = false, ErrorMessage = Strings.UserService_UserNotFound };
            }
        }

        /// <inheritdoc/>
        public async Task<ActiveStatusUpdateResult> SetUserActiveStatusAsync(string userId, bool isActive)
        {
            try
            {
                await _userRepository.SetUserActiveStatusAsync(userId, isActive);
                return new ActiveStatusUpdateResult { Success = true };
            }
            catch (UserNotFoundException)
            {
                return new ActiveStatusUpdateResult { Success = false, ErrorMessage = Strings.UserService_UserNotFound };
            }
        }

        /// <inheritdoc/>
        public async Task<UnlockUserResult> UnlockUserAsync(string userId)
        {
            try
            {
                await _userRepository.UnlockUserAsync(userId);
                return new UnlockUserResult { Success = true };
            }
            catch (UserNotFoundException)
            {
                return new UnlockUserResult { Success = false, ErrorMessage = Strings.UserService_UserNotFound };
            }
        }

        /// <summary>
        /// Deterministic SHA-256 hash of the normalized (trimmed, lowercased) username.
        /// Used only for uniqueness enforcement and login lookup — AccountName itself is
        /// encrypted non-deterministically (see <see cref="IEncryptionService"/>) and can
        /// no longer be compared directly. Blind index pattern.
        /// </summary>
        private static string ComputeUsernameHash(string accountName)
        {
            string normalized = (accountName ?? string.Empty).Trim().ToLowerInvariant();
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            return Convert.ToBase64String(hash);
        }

        private User Encrypt(User user, bool hashPassword)
        {
            return new User
            {
                Id = user.Id,
                AccountName = _encryptionService.Encrypt(user.AccountName),
                UsernameHash = ComputeUsernameHash(user.AccountName),
                FirstName = _encryptionService.Encrypt(user.FirstName),
                LastName = _encryptionService.Encrypt(user.LastName),
                IsActive = user.IsActive,
                ProfileId = user.ProfileId,
                // On create, the caller supplies a plaintext password to hash. On update,
                // ProfilePassword already carries a hash produced by the caller (e.g. a
                // password-change flow) and must be passed through unchanged.
                ProfilePassword = hashPassword ? _passwordHasher.Hash(user.ProfilePassword) : user.ProfilePassword,
                FirstLogin = user.FirstLogin,
                FailedLoginAttemptCount = user.FailedLoginAttemptCount,
                FailedResetAttemptCount = user.FailedResetAttemptCount,
                FirstFailedLoginTime = user.FirstFailedLoginTime,
                FirstResetLoginTime = user.FirstResetLoginTime,
                PasswordModificationDate = user.PasswordModificationDate,
                LastThreePasswords = user.LastThreePasswords
            };
        }

        private User Decrypt(User stored)
        {
            return new User
            {
                Id = stored.Id,
                AccountName = _encryptionService.Decrypt(stored.AccountName),
                UsernameHash = stored.UsernameHash,
                FirstName = _encryptionService.Decrypt(stored.FirstName),
                LastName = _encryptionService.Decrypt(stored.LastName),
                IsActive = stored.IsActive,
                ProfileId = stored.ProfileId,
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
