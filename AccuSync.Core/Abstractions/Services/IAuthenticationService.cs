// --------------------------------------------------------------------------------
// <copyright file="IAuthenticationService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace AccuSync.Core.Abstractions.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateAsync(string accountName, string password);
    }
}
