// --------------------------------------------------------------------------------
// <copyright file="IPasswordHasher.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Core.Abstractions.Services
{
    /// <summary>
    /// One-way password hashing. Implemented by <c>AccuSync.Application</c>.
    /// Passwords are never encrypted/decrypted — only hashed and verified.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hashes <paramref name="password"/> with a freshly generated salt.
        /// Returns a single self-describing string (algorithm parameters + salt + hash)
        /// that <see cref="Verify"/> can check against later.
        /// </summary>
        string Hash(string password);

        /// <summary>
        /// Verifies <paramref name="password"/> against a hash previously produced by
        /// <see cref="Hash"/>, using the salt/iteration count embedded in that hash.
        /// </summary>
        bool Verify(string password, string hash);
    }
}
