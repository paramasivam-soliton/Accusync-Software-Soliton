// --------------------------------------------------------------------------------
// <copyright file="CurrentUserContextTests.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Services.Authentication;
using AccuSync.Core.Entities;

namespace AccuSync.Application.Tests.Services.Authentication
{
    public class CurrentUserContextTests
    {
        private static User NewUser(string id = "user-guid-1", string accountName = "Screener") => new()
        {
            Id = id,
            AccountName = accountName
        };

        [Fact]
        public void IsSignedIn_NewUserContext_IsFalse()
        {
            var userContext = new CurrentUserContext();

            Assert.False(userContext.IsSignedIn);
        }

        [Fact]
        public void Set_UserContext_ExposesIdentityAndRole()
        {
            var userContext = new CurrentUserContext();
            var user = NewUser(id: "abc-123", accountName: "Admin");

            userContext.Set(user, UserRole.Admin);

            Assert.True(userContext.IsSignedIn);
            Assert.Equal(user.Id, userContext.Id);
            Assert.Equal(user.AccountName, userContext.AccountName);
            Assert.Equal(UserRole.Admin, userContext.Role);
        }

        [Fact]
        public void Unset_SignedInUserContext_ClearsIdentityAndResetsRoleToScreener()
        {
            var userContext = new CurrentUserContext();
            userContext.Set(NewUser(), UserRole.Admin);

            userContext.Unset();

            Assert.False(userContext.IsSignedIn);
            Assert.Null(userContext.Id);
            Assert.Null(userContext.AccountName);
            Assert.Equal(UserRole.Screener, userContext.Role);
        }

        [Fact]
        public void Set_UserContextPreviouslySignedInAsAnotherUser_ReplacesIdentityEntirely()
        {
            var userContext = new CurrentUserContext();
            userContext.Set(NewUser(id: "admin-guid", accountName: "Admin"), UserRole.Admin);
            var screenerUser = NewUser(id: "screener-guid", accountName: "Screener");

            userContext.Set(screenerUser, UserRole.Screener);

            Assert.Equal(screenerUser.Id, userContext.Id);
            Assert.Equal(screenerUser.AccountName, userContext.AccountName);
            Assert.Equal(UserRole.Screener, userContext.Role);
        }
    }
}
