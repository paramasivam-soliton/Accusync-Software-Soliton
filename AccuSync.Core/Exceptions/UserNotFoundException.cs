// --------------------------------------------------------------------------------
// <copyright file="UserNotFoundException.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Core.Exceptions
{
    /// <summary>
    /// Thrown when an operation targets a user account that no longer exists in
    /// storage (e.g. an update racing a deletion) — a data-integrity condition,
    /// distinct from a normal "account not found" login outcome.
    /// </summary>
    public class UserNotFoundException : Exception
    {
        /// <summary>Creates the exception with a message describing which lookup failed.</summary>
        /// <param name="message">Describes which user could not be found.</param>
        public UserNotFoundException(string message) : base(message)
        {
        }
    }
}
