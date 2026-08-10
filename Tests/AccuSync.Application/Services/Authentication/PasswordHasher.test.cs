// --------------------------------------------------------------------------------
// <copyright file="PasswordHasher.test.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Services.Authentication;

namespace AccuSync.Application.Tests.Services.Authentication
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _sut;

        public PasswordHasherTests()
        {
            _sut = new PasswordHasher();
        }

        [Fact]
        public void GivenAPassword_WhenHashed_ThenReturnsSelfDescribingThreePartString()
        {
            // Arrange
            const string password = "Password@123";

            // Act
            string hash = _sut.Hash(password);

            // Assert
            string[] parts = hash.Split('.');
            Assert.Equal(3, parts.Length);
            Assert.True(int.TryParse(parts[0], out int iterations));
            Assert.Equal(210_000, iterations);
            Assert.True(Convert.FromBase64String(parts[1]).Length == 16); // 128-bit salt
            Assert.True(Convert.FromBase64String(parts[2]).Length == 32); // 256-bit derived key
        }

        [Fact]
        public void GivenTheSamePasswordTwice_WhenHashedEachTime_ThenProducesDifferentHashes()
        {
            // Arrange
            const string password = "Correct-Horse-9";

            // Act
            string firstHash = _sut.Hash(password);
            string secondHash = _sut.Hash(password);

            // Assert — a fresh random salt every call means the same password
            // never produces the same stored value twice.
            Assert.NotEqual(firstHash, secondHash);
        }

        [Fact]
        public void GivenTheExactPasswordThatWasHashed_WhenVerified_ThenReturnsTrue()
        {
            // Arrange
            const string password = "Correct-Horse-9";
            string hash = _sut.Hash(password);

            // Act
            bool result = _sut.Verify(password, hash);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("Correct-Horse-9", "wrong-password")]
        [InlineData("Correct-Horse-9", "correct-horse-9")] // case differs
        [InlineData("Correct-Horse-9", "Correct-Horse-9 ")] // trailing space
        public void GivenAPasswordThatDoesNotMatchTheHashedOne_WhenVerified_ThenReturnsFalse(
            string hashedPassword, string suppliedPassword)
        {
            // Arrange
            string hash = _sut.Hash(hashedPassword);

            // Act
            bool result = _sut.Verify(suppliedPassword, hash);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GivenANullOrEmptyStoredHash_WhenVerified_ThenReturnsFalseRatherThanThrowing(string? storedHash)
        {
            // Act
            bool result = _sut.Verify("any-password", storedHash!);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("not-a-valid-hash-format")]                 // no separators at all
        [InlineData("210000.onlyTwoParts")]                      // missing the third part
        [InlineData("not-a-number.c29tZXNhbHQ=.c29tZWhhc2g=")]   // non-numeric iteration count
        public void GivenAMalformedStoredHash_WhenVerified_ThenReturnsFalseRatherThanThrowing(string malformedHash)
        {
            // Act
            bool result = _sut.Verify("any-password", malformedHash);

            // Assert
            Assert.False(result);
        }
    }
}
