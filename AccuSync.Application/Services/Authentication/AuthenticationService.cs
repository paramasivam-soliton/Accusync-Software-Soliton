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
using AccuSync.Core.Constants;
using AccuSync.Core.Entities;

namespace AccuSync.Application.Services.Authentication
{
    /// <summary>
    /// Handles user authentication: rejects deactivated accounts regardless of
    /// password correctness, enforces lockout protection (5 consecutive failed
    /// attempts locks the account for an admin-configurable duration, default 15
    /// minutes), and enforces 90-day password expiration — credentials that are
    /// otherwise correct succeed but are flagged via
    /// <see cref="AuthenticationResult.IsPasswordExpired"/> so the caller can force
    /// a password reset before granting access, the same way first-login does. An
    /// Admin can also unlock a specific account immediately via
    /// <see cref="IUserService.UnlockUserAsync"/>, overriding the timer for that one
    /// account. There is no special case for Admin accounts — the same mandatory
    /// lock/duration applies to every role.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAppSettingsRepository _appSettingsRepository;

        /// <summary>
        /// Creates the service with its user-lookup, password-verification, and
        /// lockout-settings dependencies.
        /// </summary>
        /// <param name="userService">Used to look up and update user accounts.</param>
        /// <param name="passwordHasher">Used to verify the submitted password against its stored hash.</param>
        /// <param name="appSettingsRepository">Used to read the admin-configurable lockout duration.</param>
        public AuthenticationService(
            IUserService userService,
            IPasswordHasher passwordHasher,
            IAppSettingsRepository appSettingsRepository)
        {
            _userService = userService;
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
            var inputError = ValidateInput(accountName, password);
            if (inputError != null)
            {
                return inputError;
            }

            var user = await _userService.GetUserByAccountNameAsync(accountName);
            if (user == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = Strings.AuthenticationService_AuthenticationFailed
                };
            }

            // Everything below touches either the password hash or persists lockout/attempt
            // state. A corrupted stored hash or a failed save are data-integrity problems,
            // not a wrong password — they must not count against the lockout threshold or
            // be reported to the user as "invalid credentials". They're surfaced here as a
            // single, dedicated failure instead of leaking an exception/raw message to the UI.
            // TODO: Log the exception once a logger is introduced into the solution.
            try
            {
                var inactiveResult = ValidateActiveAccount(user);
                if (inactiveResult != null)
                {
                    return inactiveResult;
                }

                var lockoutResult = await EnforceLockoutAsync(user);
                if (lockoutResult != null)
                {
                    return lockoutResult;
                }

                var passwordResult = await VerifyPasswordAsync(user, password);
                if (passwordResult != null)
                {
                    return passwordResult;
                }

                return await CompleteSuccessAsync(user);
            }
            catch (Exception)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = string.Format(Strings.AuthenticationService_UnexpectedFailure, ErrorCode.Unexpected.ToDisplayCode())
                };
            }
        }

        /// <summary>Rejects empty/whitespace credentials before any lookup is attempted.</summary>
        private static AuthenticationResult ValidateInput(string accountName, string password)
        {
            if (string.IsNullOrWhiteSpace(accountName) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = Strings.AuthenticationService_CredentialsRequired
                };
            }

            return null;
        }

        /// <summary>
        /// Blocks deactivated accounts regardless of password correctness. Returns the same
        /// generic message as a bad password so active status isn't leaked to the caller.
        /// </summary>
        private static AuthenticationResult ValidateActiveAccount(User user)
        {
            if (!user.IsActive)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = Strings.AuthenticationService_AuthenticationFailed
                };
            }

            return null;
        }

        /// <summary>
        /// Returns a locked-out result while the cooldown is still active. Once the cooldown
        /// has elapsed, resets the failure streak and returns <see langword="null"/> to let
        /// authentication continue. Auto-unlocks once the configured duration has elapsed
        /// since the lockout started, OR earlier if an Admin explicitly called
        /// <see cref="IUserService.UnlockUserAsync"/>.
        /// </summary>
        private async Task<AuthenticationResult> EnforceLockoutAsync(User user)
        {
            if (user.FailedLoginAttemptCount < AuthenticationConstants.MaxFailedAttempts)
            {
                return null;
            }

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
                    ErrorMessage = string.Format(Strings.AuthenticationService_AccountLocked, remainingMinutes)
                };
            }

            // Duration elapsed — auto-unlock and let this attempt proceed normally.
            user.FailedLoginAttemptCount = 0;
            user.FirstFailedLoginTime = 0L;
            await _userService.UpdateUserAsync(user);
            return null;
        }

        /// <summary>
        /// Verifies the password hash-to-hash. On a mismatch, records the failed attempt
        /// (and the streak's start time) and returns the appropriate failure/now-locked
        /// result; returns <see langword="null"/> to let authentication continue on success.
        /// </summary>
        private async Task<AuthenticationResult> VerifyPasswordAsync(User user, string password)
        {
            if (_passwordHasher.Verify(password, user.ProfilePassword))
            {
                return null;
            }

            // Track the timestamp of the first failure in a streak so the
            // lockout window is measured from when failures started.
            if (user.FailedLoginAttemptCount == 0)
            {
                user.FirstFailedLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            user.FailedLoginAttemptCount++;
            await _userService.UpdateUserAsync(user);

            int attemptsRemaining = AuthenticationConstants.MaxFailedAttempts - user.FailedLoginAttemptCount;

            if (user.FailedLoginAttemptCount >= AuthenticationConstants.MaxFailedAttempts)
            {
                int lockoutDurationMinutes = await _appSettingsRepository.GetLockoutDurationMinutesAsync();
                return new AuthenticationResult
                {
                    Success = false,
                    IsLocked = true,
                    ErrorMessage = string.Format(Strings.AuthenticationService_AccountNowLocked, AuthenticationConstants.MaxFailedAttempts, lockoutDurationMinutes)
                };
            }

            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = attemptsRemaining > 0
                    ? string.Format(Strings.AuthenticationService_InvalidCredentialsWithAttempts, attemptsRemaining)
                    : Strings.AuthenticationService_AuthenticationFailed
            };
        }

        /// <summary>
        /// Resets the failure streak and returns a successful result for <paramref name="user"/>,
        /// flagging <see cref="AuthenticationResult.IsPasswordExpired"/> when the password is
        /// <see cref="AuthenticationConstants.PasswordExpiryDays"/>+ days old — a forced reset,
        /// not a login failure, the same way first-login is.
        /// </summary>
        private async Task<AuthenticationResult> CompleteSuccessAsync(User user)
        {
            user.FailedLoginAttemptCount = 0;
            user.FirstFailedLoginTime = 0L;
            await _userService.UpdateUserAsync(user);

            // Computed from PasswordModificationDate — same "derive it, don't store a
            // separate flag" pattern lockout uses above — and entirely in Unix seconds, so
            // the comparison is timezone-agnostic regardless of the server/client's local
            // offset. A record with no recorded change (PasswordModificationDate <= 0, e.g.
            // hand-edited or imported data) produces a very large age and is therefore
            // treated as expired: fail secure rather than silently exempting it from the policy.
            long currentTimeSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long passwordAge = currentTimeSeconds - user.PasswordModificationDate;
            long passwordExpiryAgeSeconds = AuthenticationConstants.PasswordExpiryDays * 24 * 60 * 60;
            bool isPasswordExpired = passwordAge >= passwordExpiryAgeSeconds;

            return new AuthenticationResult
            {
                Success = true,
                User = user,
                IsPasswordExpired = isPasswordExpired
            };
        }
    }
}
