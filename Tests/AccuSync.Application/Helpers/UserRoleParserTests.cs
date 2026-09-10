// --------------------------------------------------------------------------------
// <copyright file="UserRoleParserTests.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Helpers;
using AccuSync.Core.Entities;

namespace AccuSync.Application.Tests.Helpers
{
    public class UserRoleParserTests
    {
        [Fact]
        public void Parse_AdminProfileId_ReturnsAdmin()
        {
            UserRole role = UserRoleParser.Parse((int)UserRole.Admin);

            Assert.Equal(UserRole.Admin, role);
        }

        [Fact]
        public void Parse_ScreenerProfileId_ReturnsScreener()
        {
            UserRole role = UserRoleParser.Parse((int)UserRole.Screener);

            Assert.Equal(UserRole.Screener, role);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        [InlineData(999)]
        public void Parse_ProfileIdWithNoMatchingUserRole_DefaultsToScreener(int profileId)
        {
            UserRole role = UserRoleParser.Parse(profileId);

            Assert.Equal(UserRole.Screener, role);
        }
    }
}
