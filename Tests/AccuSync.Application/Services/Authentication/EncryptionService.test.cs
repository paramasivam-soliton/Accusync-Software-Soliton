// --------------------------------------------------------------------------------
// <copyright file="EncryptionService.test.cs" company="Natus Sensory">
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
        private readonly EncryptionService _sut;

        public EncryptionServiceTests()
        {
            _keyRingPath = Path.Combine(Path.GetTempPath(), "AccuSyncEncryptionServiceTests_" + Guid.NewGuid());
            var provider = DataProtectionProvider.Create(new DirectoryInfo(_keyRingPath));
            _sut = new EncryptionService(provider);
        }

        [Fact]
        public void GivenPlainText_WhenEncrypted_ThenReturnsCiphertextDifferentFromTheInput()
        {
            // Arrange
            const string plainText = "Screener";

            // Act
            string ciphertext = _sut.Encrypt(plainText);

            // Assert
            Assert.NotEqual(plainText, ciphertext);
        }

        [Fact]
        public void GivenTheSamePlainTextTwice_WhenEncryptedEachTime_ThenProducesDifferentCiphertext()
        {
            // Arrange
            const string plainText = "Screener";

            // Act
            string firstCiphertext = _sut.Encrypt(plainText);
            string secondCiphertext = _sut.Encrypt(plainText);

            // Assert — non-deterministic by design; this is exactly why lookups/uniqueness
            // go through a separate deterministic hash column instead of this ciphertext.
            Assert.NotEqual(firstCiphertext, secondCiphertext);
        }

        [Fact]
        public void GivenValueThatWasEncryptedBySameService_WhenDecrypted_ThenReturnsOriginalPlainText()
        {
            // Arrange
            const string plainText = "Screener";
            string ciphertext = _sut.Encrypt(plainText);

            // Act
            string result = _sut.Decrypt(ciphertext);

            // Assert
            Assert.Equal(plainText, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GivenNullOrEmptyInput_WhenEncrypted_ThenReturnsInputUnchanged(string? plainText)
        {
            // Act
            string? result = _sut.Encrypt(plainText!);

            // Assert
            Assert.Equal(plainText, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GivenNullOrEmptyInput_WhenDecrypted_ThenReturnsInputUnchanged(string? cipherText)
        {
            // Act
            string? result = _sut.Decrypt(cipherText!);

            // Assert
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
