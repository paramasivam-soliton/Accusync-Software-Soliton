// --------------------------------------------------------------------------------
// <copyright file="CurrentUserContext.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;

namespace AccuSync.Application.Services.Authentication
{
    /// <summary>
    /// In-memory <see cref="ICurrentUserContext"/> — registered as a singleton, so
    /// there is exactly one instance for the lifetime of the application process.
    /// </summary>
    public class CurrentUserContext : ICurrentUserContext
    {
        public string Guid { get; private set; }
        public string AccountName { get; private set; }
        public UserRole Role { get; private set; }
        public bool IsSignedIn { get; private set; }

        public void SignIn(User user, UserRole role)
        {
            Guid = user.Guid;
            AccountName = user.AccountName;
            Role = role;
            IsSignedIn = true;
        }

        public void SignOut()
        {
            Guid = null;
            AccountName = null;
            Role = UserRole.Screener;
            IsSignedIn = false;
        }
    }
}
