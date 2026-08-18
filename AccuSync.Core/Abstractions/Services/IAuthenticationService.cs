// --------------------------------------------------------------------------------
// <copyright file="IAuthenticationService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace AccuSync.Core.Abstractions.Services
{
    /// <summary>
    /// Authenticates user credentials against stored accounts.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Verifies the given credentials and reports success, failure, or lockout state.
        /// </summary>
        /// <param name="accountName">The account name to authenticate.</param>
        /// <param name="password">The plaintext password to verify.</param>
        /// <returns>An <see cref="AuthenticationResult"/> describing the outcome.</returns>
        Task<AuthenticationResult> AuthenticateAsync(string accountName, string password);
    }
}
