// --------------------------------------------------------------------------------
// <copyright file="AuthenticationService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using AccuSync.Application.Abstractions.Services;
using AccuSync.Application.Models;

namespace AccuSync.Application.Services
{
    /// <summary>
    /// Handles user authentication with lockout protection (10 failed attempts,
    /// 15-minute cooldown) and 90-day password expiration.
    /// </summary>
    // TODO: Passwords are compared by decrypting the stored value and checking
    //       equality in plaintext. This means passwords are reversibly encrypted,
    //       not hashed. Industry standard is to store a salted hash (e.g., bcrypt)
    //       and compare hashes — reversible encryption means anyone with the key
    //       can read all passwords.
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IDatabaseService _databaseService;
        private readonly IEncryptionService _encryptionService;
        private const int MaxFailedAttempts = 10;
        private const int LockoutDurationMinutes = 15;

        public AuthenticationService(IDatabaseService databaseService, IEncryptionService encryptionService)
        {
            _databaseService = databaseService;
            _encryptionService = encryptionService;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string accountName, string password)
        {
            if (string.IsNullOrWhiteSpace(accountName) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Username and password are required."
                };
            }

            var user = await _databaseService.GetUserByAccountNameAsync(accountName);
            if (user == null)
            {
                // Same error message whether the user exists or not,
                // so attackers can't enumerate valid account names.
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                };
            }

            // Lockout check — resets automatically after the cooldown period
            if (user.FailedLoginAttemptCount >= MaxFailedAttempts)
            {
                long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long lockoutEndTime = user.FirstFailedLoginTime + (LockoutDurationMinutes * 60);

                if (currentTime < lockoutEndTime)
                {
                    long remainingSeconds = lockoutEndTime - currentTime;
                    return new AuthenticationResult
                    {
                        Success = false,
                        IsLocked = true,
                        RemainingLockTime = TimeSpan.FromSeconds(remainingSeconds),
                        ErrorMessage = $"Account is locked. Please try again in {TimeSpan.FromSeconds(remainingSeconds).Minutes} minutes."
                    };
                }
                else
                {
                    user.FailedLoginAttemptCount = 0;
                    user.FirstFailedLoginTime = 0L;
                    await _databaseService.UpdateUserAsync(user);
                }
            }

            // Password verification — see class-level TODO about hashing vs encryption
            string decryptedPassword = _encryptionService.Decrypt(user.ProfilePassword);
            if (password != decryptedPassword)
            {
                // Track the timestamp of the first failure in a streak so the
                // lockout window is measured from when failures started.
                if (user.FailedLoginAttemptCount == 0)
                {
                    user.FirstFailedLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                }

                user.FailedLoginAttemptCount++;
                await _databaseService.UpdateUserAsync(user);

                int attemptsRemaining = MaxFailedAttempts - user.FailedLoginAttemptCount;

                if (user.FailedLoginAttemptCount >= MaxFailedAttempts)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        IsLocked = true,
                        ErrorMessage = $"Account is now locked due to {MaxFailedAttempts} failed attempts. Please try again in {LockoutDurationMinutes} minutes."
                    };
                }

                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = attemptsRemaining > 0
                        ? $"Invalid username or password. {attemptsRemaining} attempt(s) remaining."
                        : "Invalid username or password."
                };
            }

            // 90-day password expiration
            long currentTimeSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long passwordAge = currentTimeSeconds - user.PasswordModificationDate;
            long ninetyDaysInSeconds = 90 * 24 * 60 * 60;

            // TODO: Expired passwords return Success = false but the user has no way
            //       to reset their own password — they have to contact an admin.
            //       Consider returning a distinct result (e.g., PasswordExpired flag)
            //       so the UI can redirect to a password-change screen instead.
            if (passwordAge > ninetyDaysInSeconds)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Your password has expired. Please contact your administrator."
                };
            }

            // Success — reset the failure streak
            user.FailedLoginAttemptCount = 0;
            user.FirstFailedLoginTime = 0L;
            await _databaseService.UpdateUserAsync(user);

            return new AuthenticationResult
            {
                Success = true,
                User = user
            };
        }
    }
}