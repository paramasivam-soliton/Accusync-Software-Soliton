// --------------------------------------------------------------------------------
// <copyright file="UserRoleParser.test.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Helpers;
using AccuSync.Core.Entities;

namespace AccuSync.Application.Tests.Helpers
{
    public class UserRoleParserTests
    {
        [Theory]
        [InlineData("Admin")]
        [InlineData("admin")]
        [InlineData("ADMIN")]
        [InlineData("AdMiN")]
        public void Parse_GivenAdminInAnyCasing_WhenParsed_ThenReturnsAdmin(string profileId)
        {
            // Act
            UserRole role = UserRoleParser.Parse(profileId);

            // Assert
            Assert.Equal(UserRole.Admin, role);
        }

        [Fact]
        public void Parse_GivenScreener_WhenParsed_ThenReturnsScreener()
        {
            // Act
            UserRole role = UserRoleParser.Parse("Screener");

            // Assert
            Assert.Equal(UserRole.Screener, role);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("SomeUnrecognizedProfileId")]
        [InlineData(" Admin")] // leading whitespace — not an exact match
        [InlineData("Admin ")] // trailing whitespace — not an exact match
        public void Parse_GivenAnythingOtherThanAnExactAdminMatch_WhenParsed_ThenDefaultsToScreener(string? profileId)
        {
            // Act
            UserRole role = UserRoleParser.Parse(profileId!);

            // Assert — Screener is the least-privileged role; an unrecognized or
            // missing ProfileId must never silently resolve to Admin.
            Assert.Equal(UserRole.Screener, role);
        }
    }
}
