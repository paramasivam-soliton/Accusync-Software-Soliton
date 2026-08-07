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
        string Guid { get; }
        string AccountName { get; }
        UserRole Role { get; }
        bool IsSignedIn { get; }

        void SignIn(User user, UserRole role);
        void SignOut();
    }
}
