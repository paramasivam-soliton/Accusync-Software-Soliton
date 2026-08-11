// --------------------------------------------------------------------------------
// <copyright file="ICurrentUserContext.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities;

namespace AccuSync.Core.Abstractions.Services
{
    /// <summary>
    /// The signed-in user for the current application session, held in memory only.
    /// Populated once at login; role changes for that user take effect on their next
    /// login, not live mid-session.
    /// </summary>
    public interface ICurrentUserContext
    {
        /// <summary>The signed-in user's unique identifier, or null if no one is signed in.</summary>
        string Id { get; }

        /// <summary>The signed-in user's account name, or null if no one is signed in.</summary>
        string AccountName { get; }

        /// <summary>The signed-in user's role. Undefined/least-privilege when no one is signed in.</summary>
        UserRole Role { get; }

        /// <summary>Whether a user is currently signed in.</summary>
        bool IsSignedIn { get; }

        /// <summary>Populates the session from a successful login.</summary>
        void SignIn(User user, UserRole role);

        /// <summary>Clears the session.</summary>
        void SignOut();
    }
}
