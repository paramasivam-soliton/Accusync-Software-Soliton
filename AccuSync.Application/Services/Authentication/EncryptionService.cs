// --------------------------------------------------------------------------------
// <copyright file="EncryptionService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Abstractions.Services;
using Microsoft.AspNetCore.DataProtection;

namespace AccuSync.Application.Services.Authentication
{
    /// <summary>
    /// Encryption via the ASP.NET Core Data Protection API. Each <see cref="Encrypt"/>
    /// call produces a different ciphertext for the same input.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        /// <summary>Data Protection purpose string identifying the encrypted-fields key ring.</summary>
        private const string EncryptionProtectorPurpose = "AccuSync.Users.EncryptedFields";

        private readonly IDataProtector _protector;

        /// <summary>
        /// Creates the service and derives its protector from the encrypted-fields purpose string.
        /// </summary>
        /// <param name="dataProtectionProvider">Supplies the key ring used to create the protector.</param>
        public EncryptionService(IDataProtectionProvider dataProtectionProvider)
        {
            _protector = dataProtectionProvider.CreateProtector(EncryptionProtectorPurpose);
        }

        /// <inheritdoc/>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            return _protector.Protect(plainText);
        }

        /// <inheritdoc/>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            return _protector.Unprotect(cipherText);
        }
    }
}
