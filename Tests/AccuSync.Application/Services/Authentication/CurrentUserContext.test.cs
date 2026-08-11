// --------------------------------------------------------------------------------
// <copyright file="CurrentUserContext.test.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Services.Authentication;
using AccuSync.Core.Entities;

namespace AccuSync.Application.Tests.Services.Authentication
{
    public class CurrentUserContextTests
    {
        private static User NewUser(string guid = "user-guid-1", string accountName = "Screener") => new()
        {
            Guid = guid,
            AccountName = accountName
        };

        [Fact]
        public void GivenAFreshlyConstructedContext_WhenNeverSignedIn_ThenIsFalse()
        {
            // Arrange
            var sut = new CurrentUserContext();

            // Assert
            Assert.False(sut.IsSignedIn);
        }

        [Fact]
        public void GivenAUserAndRole_WhenSignedIn_ThenExposesThatIdentityAndRole()
        {
            // Arrange
            var sut = new CurrentUserContext();
            var user = NewUser(guid: "abc-123", accountName: "Admin");

            // Act
            sut.SignIn(user, UserRole.Admin);

            // Assert
            Assert.True(sut.IsSignedIn);
            Assert.Equal("abc-123", sut.Guid);
            Assert.Equal("Admin", sut.AccountName);
            Assert.Equal(UserRole.Admin, sut.Role);
        }

        [Fact]
        public void GivenASignedInContext_WhenSignedOut_ThenClearsIdentityAndResetsRoleToScreener()
        {
            // Arrange
            var sut = new CurrentUserContext();
            sut.SignIn(NewUser(), UserRole.Admin);

            // Act
            sut.SignOut();

            // Assert — Screener (the least-privileged role) rather than leaving the
            // prior Admin role dangling on an otherwise-signed-out context.
            Assert.False(sut.IsSignedIn);
            Assert.Null(sut.Guid);
            Assert.Null(sut.AccountName);
            Assert.Equal(UserRole.Screener, sut.Role);
        }

        [Fact]
        public void GivenAContextThatWasPreviouslySignedInAsAnotherUser_WhenSignedInAgain_ThenReplacesTheIdentityEntirely()
        {
            // Arrange — simulates one user logging out and a different user logging in,
            // without a fresh CurrentUserContext instance (it's a process-lifetime singleton).
            var sut = new CurrentUserContext();
            sut.SignIn(NewUser(guid: "admin-guid", accountName: "Admin"), UserRole.Admin);

            // Act
            sut.SignIn(NewUser(guid: "screener-guid", accountName: "Screener"), UserRole.Screener);

            // Assert — no leftover state from the previous session.
            Assert.Equal("screener-guid", sut.Guid);
            Assert.Equal("Screener", sut.AccountName);
            Assert.Equal(UserRole.Screener, sut.Role);
        }
    }
}
