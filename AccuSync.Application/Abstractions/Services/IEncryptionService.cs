// --------------------------------------------------------------------------------
// <copyright file="IEncryptionService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Abstractions.Services
{
    /// <summary>
    /// Encrypts and decrypts sensitive data at rest.
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

        /// <summary>
        /// Gets a value indicating whether the application is running a release build.
        /// </summary>
        bool IsReleaseMode { get; }
    }
}
