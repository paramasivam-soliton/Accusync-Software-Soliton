// --------------------------------------------------------------------------------
// <copyright file="PasswordHasher.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Security.Cryptography;
using AccuSync.Core.Abstractions.Services;

namespace AccuSync.Application.Services.Authentication
{
    /// <summary>
    /// PBKDF2-HMACSHA256 password hashing (<see cref="Rfc2898DeriveBytes"/>, built into
    /// .NET — no third-party dependency). Chosen over BCrypt/Argon2 because PBKDF2 has
    /// FIPS 140-validated implementations available on Windows.
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        // OWASP 2023 minimum recommendation for PBKDF2-HMAC-SHA256.
        private const int Iterations = 210_000;
        private const int SaltSizeBytes = 16;   // 128-bit salt
        private const int HashSizeBytes = 32;   // 256-bit derived key

        /// <summary>
        /// Stored as "{iterations}.{base64(salt)}.{base64(hash)}" — self-describing so the
        /// iteration count can be raised later without invalidating existing hashes.
        /// </summary>
        public string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSizeBytes);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrEmpty(hash)) return false;

            string[] parts = hash.Split('.');
            if (parts.Length != 3) return false;

            if (!int.TryParse(parts[0], out int iterations)) return false;

            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expectedHash = Convert.FromBase64String(parts[2]);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
