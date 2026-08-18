// --------------------------------------------------------------------------------
// <copyright file="EncryptionService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AccuSync.Application.Abstractions.Services;

namespace AccuSync.Application.Services
{
    /// <summary>
    /// AES-256-CBC encryption service for protecting sensitive data at rest
    /// (e.g., passwords, SSNs). Encryption is disabled in debug builds so
    /// database values stay readable during development.
    /// </summary>
    // TODO: Several security issues need attention before production use:
    //
    //   1. The key is derived from the machine name + a hardcoded salt ("AccuSync2024").
    //      This means encrypted data can't be read on a different machine, and the
    //      salt is visible in the source. Move key material to Windows Credential
    //      Manager or Azure Key Vault.
    //
    //   2. The IV is derived from the same input every time, making it static.
    //      CBC with a fixed IV is vulnerable to pattern analysis. Generate a
    //      random IV per encryption and prepend it to the ciphertext.
    //
    //   3. Encrypt/Decrypt silently return plaintext on failure. This masks
    //      errors and can cause unencrypted data to be written to the database
    //      without anyone noticing.
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        /// <summary>
        /// Gets a value indicating whether this is a release build. Encryption is
        /// only active in release builds; debug builds store values as plaintext.
        /// </summary>
        public bool IsReleaseMode
        {
            get
            {
#if RELEASE
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Derives the AES key and IV from the machine name (see class-level TODO
        /// about moving this to a proper key store).
        /// </summary>
        public EncryptionService()
        {
            // Key is machine-specific so encrypted data is bound to this host.
            // See class-level TODO about moving to a proper key store.
            string machineKey = Environment.MachineName + "AccuSync2024";
            using (var sha256 = SHA256.Create())
            {
                var keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineKey));
                _key = new byte[32];
                Array.Copy(keyBytes, _key, 32);

                var ivBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineKey + "IV"));
                _iv = new byte[16];
                Array.Copy(ivBytes, _iv, 16);
            }
        }

        /// <summary>
        /// Encrypts <paramref name="plainText"/> using AES-256-CBC and returns
        /// a Base64-encoded string. Returns plaintext unchanged in debug builds.
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            if (!IsReleaseMode)
                return plainText;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = _iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch
            {
                // See class-level TODO #3 — silently returning plaintext here
                // can cause unencrypted data to be persisted.
                return plainText;
            }
        }

        /// <summary>
        /// Decrypts a Base64-encoded <paramref name="cipherText"/> string.
        /// Returns ciphertext unchanged in debug builds.
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            if (!IsReleaseMode)
                return cipherText;

            try
            {
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = _iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(buffer))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                // See class-level TODO #3
                return cipherText;
            }
        }
    }
}