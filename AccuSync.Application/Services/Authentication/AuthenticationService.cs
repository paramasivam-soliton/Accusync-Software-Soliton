// --------------------------------------------------------------------------------
// <copyright file="AuthenticationService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;

namespace AccuSync.Application.Services.Authentication
{
    /// <summary>
    /// Handles user authentication with lockout protection (10 failed attempts,
    /// 15-minute cooldown) and 90-day password expiration.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private const int MaxFailedAttempts = 10;
        private const int LockoutDurationMinutes = 15;

        /// <summary>
        /// Creates the service with its required data and password-hashing dependencies.
        /// </summary>
        /// <param name="userRepository">Used to look up and update user accounts.</param>
        /// <param name="passwordHasher">Used to verify the submitted password against its stored hash.</param>
        public AuthenticationService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Authenticates the given credentials, applying lockout and password
        /// expiration checks. See the class-level remarks for details.
        /// </summary>
        /// <param name="accountName">The account name to authenticate.</param>
        /// <param name="password">The plaintext password to verify.</param>
        /// <returns>An <see cref="AuthenticationResult"/> describing the outcome.</returns>
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

            var user = await _userRepository.GetUserByAccountNameAsync(accountName);
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
                    await _userRepository.UpdateUserAsync(user);
                }
            }

            // Password verification — hash-to-hash only, never decrypt/compare plaintext.
            if (!_passwordHasher.Verify(password, user.ProfilePassword))
            {
                // Track the timestamp of the first failure in a streak so the
                // lockout window is measured from when failures started.
                if (user.FailedLoginAttemptCount == 0)
                {
                    user.FirstFailedLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                }

                user.FailedLoginAttemptCount++;
                await _userRepository.UpdateUserAsync(user);

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
            await _userRepository.UpdateUserAsync(user);

            return new AuthenticationResult
            {
                Success = true,
                User = user
            };
        }
    }
}