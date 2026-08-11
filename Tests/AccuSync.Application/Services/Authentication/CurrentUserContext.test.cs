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
        private static User NewUser(string id = "user-guid-1", string accountName = "Screener") => new()
        {
            Id = id,
            AccountName = accountName
        };

        private static Profile NewProfile(string id = "Screener") => new()
        {
            Id = id,
            Name = id
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
        public void GivenAUserAndProfile_WhenSignedIn_ThenExposesThatIdentityAndProfile()
        {
            // Arrange
            var sut = new CurrentUserContext();
            var user = NewUser(id: "abc-123", accountName: "Admin");
            var profile = NewProfile(id: "Admin");
            profile.UsersProfilesViewUsers = true;

            // Act
            sut.SignIn(user, profile);

            // Assert
            Assert.True(sut.IsSignedIn);
            Assert.Equal("abc-123", sut.Id);
            Assert.Equal("Admin", sut.AccountName);
            Assert.Same(profile, sut.Profile);
            Assert.True(sut.Profile.UsersProfilesViewUsers);
        }

        [Fact]
        public void GivenASignedInContext_WhenSignedOut_ThenClearsIdentityAndProfile()
        {
            // Arrange
            var sut = new CurrentUserContext();
            sut.SignIn(NewUser(), NewProfile("Admin"));

            // Act
            sut.SignOut();

            // Assert
            Assert.False(sut.IsSignedIn);
            Assert.Null(sut.Id);
            Assert.Null(sut.AccountName);
            Assert.Null(sut.Profile);
        }

        [Fact]
        public void GivenAContextThatWasPreviouslySignedInAsAnotherUser_WhenSignedInAgain_ThenReplacesTheIdentityEntirely()
        {
            // Arrange — simulates one user logging out and a different user logging in,
            // without a fresh CurrentUserContext instance (it's a process-lifetime singleton).
            var sut = new CurrentUserContext();
            sut.SignIn(NewUser(id: "admin-guid", accountName: "Admin"), NewProfile("Admin"));

            // Act
            sut.SignIn(NewUser(id: "screener-guid", accountName: "Screener"), NewProfile("Screener"));

            // Assert — no leftover state from the previous session.
            Assert.Equal("screener-guid", sut.Id);
            Assert.Equal("Screener", sut.AccountName);
            Assert.Equal("Screener", sut.Profile.Id);
        }
    }
}
