// --------------------------------------------------------------------------------
// <copyright file="IEncryptionService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Core.Abstractions.Services
{
    /// <summary>
    /// Reversible encryption for data that must be recoverable in plaintext later
    /// (e.g., usernames, which the login dropdown needs to display). Passwords never
    /// go through this — see <see cref="IPasswordHasher"/> for one-way hashing.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts the given plaintext.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <returns>The encrypted representation of <paramref name="plainText"/>.</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypts the given ciphertext.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <returns>The decrypted plaintext.</returns>
        string Decrypt(string cipherText);
    }
}
