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
        /// <param name="password">The plaintext password to hash.</param>
        /// <returns>A self-describing string encoding the iteration count, salt, and derived hash.</returns>
        string Hash(string password);

        /// <summary>
        /// Verifies <paramref name="password"/> against a hash previously produced by
        /// <see cref="Hash"/>, using the salt/iteration count embedded in that hash.
        /// </summary>
        /// <param name="password">The plaintext password to verify.</param>
        /// <param name="hash">The previously stored hash to verify against.</param>
        /// <returns><c>true</c> if <paramref name="password"/> matches <paramref name="hash"/>.</returns>
        /// <exception cref="System.FormatException">The stored hash's salt/hash segments are not valid base64 — indicates corrupted stored data, not a wrong password. Callers must not treat this the same as a failed verification.</exception>
        bool Verify(string password, string hash);
    }
}
