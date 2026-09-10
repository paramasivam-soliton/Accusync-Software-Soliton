// --------------------------------------------------------------------------------
// <copyright file="PasswordHasherTests.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Services.Authentication;

namespace AccuSync.Application.Tests.Services.Authentication
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _passwordHasher;

        public PasswordHasherTests()
        {
            _passwordHasher = new PasswordHasher();
        }

        [Fact]
        public void Hash_APassword_ReturnsSelfDescribingThreePartString()
        {
            const string password = "Password@123";

            string hash = _passwordHasher.Hash(password);

            string[] parts = hash.Split('.');
            Assert.Equal(3, parts.Length);
            Assert.True(int.TryParse(parts[0], out int iterations));
            Assert.Equal(600_000, iterations);
            Assert.True(Convert.FromBase64String(parts[1]).Length == 16);
            Assert.True(Convert.FromBase64String(parts[2]).Length == 32);
        }

        [Fact]
        public void Hash_SamePasswordTwice_ProducesDifferentHashes()
        {
            const string password = "Correct-Horse-9";

            string firstHash = _passwordHasher.Hash(password);
            string secondHash = _passwordHasher.Hash(password);

            Assert.NotEqual(firstHash, secondHash);
        }

        [Fact]
        public void Verify_TheExactPasswordThatWasHashed_ReturnsTrue()
        {
            const string password = "Correct-Horse-9";
            string hash = _passwordHasher.Hash(password);

            bool result = _passwordHasher.Verify(password, hash);

            Assert.True(result);
        }

        [Theory]
        [InlineData("Correct-Horse-9", "wrong-password")]
        [InlineData("Correct-Horse-9", "correct-horse-9")]
        [InlineData("Correct-Horse-9", "Correct-Horse-9 ")]
        public void Verify_APasswordThatDoesNotMatchTheHashedOne_ReturnsFalse(
            string hashedPassword, string suppliedPassword)
        {
            string hash = _passwordHasher.Hash(hashedPassword);

            bool result = _passwordHasher.Verify(suppliedPassword, hash);

            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Verify_ANullOrEmptyStoredHash_ReturnsFalseRatherThanThrowing(string? storedHash)
        {
            bool result = _passwordHasher.Verify("any-password", storedHash!);

            Assert.False(result);
        }

        [Theory]
        [InlineData("not-a-valid-hash-format")]
        [InlineData("210000.onlyTwoParts")]
        [InlineData("not-a-number.c29tZXNhbHQ=.c29tZWhhc2g=")]
        public void Verify_AMalformedStoredHash_ReturnsFalseRatherThanThrowing(string malformedHash)
        {
            bool result = _passwordHasher.Verify("any-password", malformedHash);

            Assert.False(result);
        }

        [Fact]
        public void Verify_AStoredHashWithCorruptedBase64Segments_ThrowsRatherThanReturningFalse()
        {
            const string corruptedHash = "600000.not-valid-base64!!!.c29tZWhhc2g=";

            Assert.Throws<FormatException>(() => _passwordHasher.Verify("any-password", corruptedHash));
        }
    }
}
