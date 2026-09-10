// --------------------------------------------------------------------------------
// <copyright file="EncryptionServiceTests.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Services.Authentication;
using Microsoft.AspNetCore.DataProtection;

namespace AccuSync.Application.Tests.Services.Authentication
{
    /// <summary>
    /// Exercises EncryptionService against a real (not mocked) Data Protection key
    /// ring, persisted to a private, per-test temp folder that is cleaned up after
    /// each test. This is fast and fully isolated — no shared key ring, no disk
    /// state left behind — while still validating genuine encrypt/decrypt behavior
    /// rather than a stand-in.
    /// </summary>
    public class EncryptionServiceTests : IDisposable
    {
        private readonly string _keyRingPath;
        private readonly EncryptionService _encryptionService;

        public EncryptionServiceTests()
        {
            _keyRingPath = Path.Combine(Path.GetTempPath(), "AccuSyncEncryptionServiceTests_" + Guid.NewGuid());
            var provider = DataProtectionProvider.Create(new DirectoryInfo(_keyRingPath));
            _encryptionService = new EncryptionService(provider);
        }

        [Fact]
        public void Encrypt_PlainText_ReturnsCiphertextDifferentFromTheInput()
        {
            const string plainText = "Screener";

            string ciphertext = _encryptionService.Encrypt(plainText);

            Assert.NotEqual(plainText, ciphertext);
        }

        [Fact]
        public void Encrypt_SamePlainTextTwice_ProducesDifferentCiphertext()
        {
            const string plainText = "Screener";

            string firstCiphertext = _encryptionService.Encrypt(plainText);
            string secondCiphertext = _encryptionService.Encrypt(plainText);

            Assert.NotEqual(firstCiphertext, secondCiphertext);
        }

        [Fact]
        public void Decrypt_ValueThatWasEncryptedBySameService_ReturnsOriginalPlainText()
        {
            const string plainText = "Screener";
            string ciphertext = _encryptionService.Encrypt(plainText);

            string result = _encryptionService.Decrypt(ciphertext);

            Assert.Equal(plainText, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Encrypt_NullOrEmptyInput_ReturnsInputUnchanged(string? plainText)
        {
            string? result = _encryptionService.Encrypt(plainText!);

            Assert.Equal(plainText, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Decrypt_NullOrEmptyInput_ReturnsInputUnchanged(string? cipherText)
        {
            string? result = _encryptionService.Decrypt(cipherText!);

            Assert.Equal(cipherText, result);
        }

        public void Dispose()
        {
            if (Directory.Exists(_keyRingPath))
            {
                Directory.Delete(_keyRingPath, recursive: true);
            }
        }
    }
}
