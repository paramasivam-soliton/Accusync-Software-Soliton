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
        /// <summary>The signed-in user's unique identifier, or null if no one is signed in.</summary>
        public string Id { get; private set; }

        /// <summary>The signed-in user's account name, or null if no one is signed in.</summary>
        public string AccountName { get; private set; }

        /// <summary>The signed-in user's assigned profile, or null if no one is signed in.</summary>
        public Profile Profile { get; private set; }

        /// <summary>Whether a user is currently signed in.</summary>
        public bool IsSignedIn { get; private set; }

        /// <summary>Populates the session from a successful login.</summary>
        public void SignIn(User user, Profile profile)
        {
            Id = user.Id;
            AccountName = user.AccountName;
            Profile = profile;
            IsSignedIn = true;
        }

        /// <summary>Clears the session.</summary>
        public void SignOut()
        {
            Id = null;
            AccountName = null;
            Profile = null;
            IsSignedIn = false;
        }
    }
}
