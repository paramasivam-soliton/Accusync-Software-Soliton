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
    /// Handles user authentication: rejects deactivated accounts regardless of
    /// password correctness, enforces lockout protection (5 consecutive failed
    /// attempts locks the account for an admin-configurable duration, default 15
    /// minutes), and enforces 90-day password expiration. An Admin can also unlock
    /// a specific account immediately via <see cref="IUserRepository.UnlockUserAsync"/>,
    /// overriding the timer for that one account. There is no special case for Admin
    /// accounts — the same mandatory lock/duration applies to every role.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAppSettingsRepository _appSettingsRepository;
        private const int MaxFailedAttempts = 5;

        /// <summary>
        /// Creates the service with its required data, password-hashing, and settings dependencies.
        /// </summary>
        /// <param name="userRepository">Used to look up and update user accounts.</param>
        /// <param name="passwordHasher">Used to verify the submitted password against its stored hash.</param>
        /// <param name="appSettingsRepository">Used to read the admin-configurable lockout duration.</param>
        public AuthenticationService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IAppSettingsRepository appSettingsRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _appSettingsRepository = appSettingsRepository;
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

            // Deactivated accounts are blocked regardless of password correctness.
            // Same generic message as a bad password — active status isn't leaked either.
            if (!user.Status)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password."
                };
            }

            // Lockout check — auto-unlocks once the configured duration has elapsed since
            // the lockout started, OR earlier if an Admin explicitly called UnlockUserAsync.
            if (user.FailedLoginAttemptCount >= MaxFailedAttempts)
            {
                int lockoutDurationMinutes = await _appSettingsRepository.GetLockoutDurationMinutesAsync();
                long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long lockoutEndTime = user.FirstFailedLoginTime + (lockoutDurationMinutes * 60);

                if (currentTime < lockoutEndTime)
                {
                    long remainingSeconds = lockoutEndTime - currentTime;
                    long remainingMinutes = (remainingSeconds + 59) / 60; // round up to whole minutes
                    return new AuthenticationResult
                    {
                        Success = false,
                        IsLocked = true,
                        ErrorMessage = $"Account locked. Try again in {remainingMinutes} minute(s), or contact your administrator."
                    };
                }

                // Duration elapsed — auto-unlock and let this attempt proceed normally.
                user.FailedLoginAttemptCount = 0;
                user.FirstFailedLoginTime = 0L;
                await _userRepository.UpdateUserAsync(user);
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
                    int lockoutDurationMinutes = await _appSettingsRepository.GetLockoutDurationMinutesAsync();
                    return new AuthenticationResult
                    {
                        Success = false,
                        IsLocked = true,
                        ErrorMessage = $"Account is now locked due to {MaxFailedAttempts} failed attempts. Try again in {lockoutDurationMinutes} minute(s), or contact your administrator."
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
