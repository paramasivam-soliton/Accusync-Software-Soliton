// --------------------------------------------------------------------------------
// <copyright file="AuthenticationService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using AccuSync.Application.Resources;
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
    /// <param name="userRepository">Used to look up and update user accounts.</param>
    /// <param name="passwordHasher">Used to verify the submitted password against its stored hash.</param>
    /// <param name="appSettingsRepository">Used to read the admin-configurable lockout duration.</param>
    public class AuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAppSettingsRepository appSettingsRepository) : IAuthenticationService
    {
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
                    ErrorMessage = Strings.AuthenticationService_ErrorCredentialsRequired
                };
            }

            var user = await userRepository.GetUserByAccountNameAsync(accountName);
            if (user == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = Strings.AuthenticationService_ErrorInvalidCredentials
                };
            }

            // Deactivated accounts are blocked regardless of password correctness.
            // Same generic message as a bad password — active status isn't leaked either.
            if (!user.IsActive)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = Strings.AuthenticationService_ErrorInvalidCredentials
                };
            }

            // Lockout check — auto-unlocks once the configured duration has elapsed since
            // the lockout started, OR earlier if an Admin explicitly called UnlockUserAsync.
            if (user.FailedLoginAttemptCount >= AuthenticationConstants.MaxFailedAttempts)
            {
                int lockoutDurationMinutes = await appSettingsRepository.GetLockoutDurationMinutesAsync();
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
                        ErrorMessage = string.Format(Strings.AuthenticationService_ErrorAccountLockedFormat, remainingMinutes)
                    };
                }

                // Duration elapsed — auto-unlock and let this attempt proceed normally.
                user.FailedLoginAttemptCount = 0;
                user.FirstFailedLoginTime = 0L;
                await userRepository.UpdateUserAsync(user);
            }

            // Password verification — hash-to-hash only, never decrypt/compare plaintext.
            if (!passwordHasher.Verify(password, user.ProfilePassword))
            {
                // Track the timestamp of the first failure in a streak so the
                // lockout window is measured from when failures started.
                if (user.FailedLoginAttemptCount == 0)
                {
                    user.FirstFailedLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                }

                user.FailedLoginAttemptCount++;
                await userRepository.UpdateUserAsync(user);

                int attemptsRemaining = AuthenticationConstants.MaxFailedAttempts - user.FailedLoginAttemptCount;

                if (user.FailedLoginAttemptCount >= AuthenticationConstants.MaxFailedAttempts)
                {
                    int lockoutDurationMinutes = await appSettingsRepository.GetLockoutDurationMinutesAsync();
                    return new AuthenticationResult
                    {
                        Success = false,
                        IsLocked = true,
                        ErrorMessage = string.Format(Strings.AuthenticationService_ErrorAccountNowLockedFormat, AuthenticationConstants.MaxFailedAttempts, lockoutDurationMinutes)
                    };
                }

                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = attemptsRemaining > 0
                        ? string.Format(Strings.AuthenticationService_ErrorInvalidCredentialsWithAttemptsFormat, attemptsRemaining)
                        : Strings.AuthenticationService_ErrorInvalidCredentials
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
                    ErrorMessage = Strings.AuthenticationService_ErrorPasswordExpired
                };
            }

            // Success — reset the failure streak
            user.FailedLoginAttemptCount = 0;
            user.FirstFailedLoginTime = 0L;
            await userRepository.UpdateUserAsync(user);

            return new AuthenticationResult
            {
                Success = true,
                User = user
            };
        }
    }
}
