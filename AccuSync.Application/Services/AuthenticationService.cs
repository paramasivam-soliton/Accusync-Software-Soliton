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

namespace AccuSync.Application.Services
{
    /// <summary>
    /// Handles user authentication with lockout protection (5 consecutive failed
    /// attempts locks the account until an Admin unlocks it — no automatic
    /// unlock/cooldown) and 90-day password expiration.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private const int MaxFailedAttempts = 5;

        public AuthenticationService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
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

            // Lockout is derived — no discrete "locked" flag/column and no auto-unlock.
            // Once FailedLoginAttemptCount reaches the threshold, the account stays
            // locked until an Admin calls IUserRepository.UnlockUserAsync.
            if (user.FailedLoginAttemptCount >= MaxFailedAttempts)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    IsLocked = true,
                    ErrorMessage = "Account locked. Contact administrator."
                };
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
                        ErrorMessage = "Account locked. Contact administrator."
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