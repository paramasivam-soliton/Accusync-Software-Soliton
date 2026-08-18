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
    /// Reversible encryption via the ASP.NET Core Data Protection API, used standalone
    /// (no web server/hosting involved). Active in every build configuration, including
    /// Debug. Each <see cref="Encrypt"/> call produces a different ciphertext for the
    /// same input (non-deterministic by design), which is why lookups/uniqueness on
    /// encrypted fields go through a separate deterministic hash column (see
    /// UserRepository.ComputeUsernameHash) instead of this service.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly IDataProtector _protector;

        public EncryptionService(IDataProtectionProvider dataProtectionProvider)
        {
            _protector = dataProtectionProvider.CreateProtector(AuthenticationConstants.EncryptionProtectorPurpose);
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            return _protector.Protect(plainText);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            return _protector.Unprotect(cipherText);
        }
    }
}
