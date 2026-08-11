// --------------------------------------------------------------------------------
// <copyright file="ChangePasswordViewModel.test.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Services.Authentication;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;
using AccuSync.Presentation.ViewModels;
using Moq;

namespace AccuSync.Presentation.Tests.ViewModels
{
    /// <summary>
    /// IUserRepository/ICurrentUserContext are mocked, but PasswordHasher is real —
    /// so the reuse-of-last-3-passwords check is exercised against genuine hashes,
    /// not a stand-in that always says yes/no.
    /// </summary>
    public class ChangePasswordViewModelTests
    {
        private const string CurrentPassword = "Current@123";

        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICurrentUserContext> _currentUserContextMock;
        private readonly PasswordHasher _passwordHasher;

        public ChangePasswordViewModelTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userRepositoryMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(true);

            _currentUserContextMock = new Mock<ICurrentUserContext>();
            _currentUserContextMock.SetupGet(c => c.Profile).Returns(new Profile { Id = "Screener", Name = "Screener" });

            _passwordHasher = new PasswordHasher();
        }

        private ChangePasswordViewModel CreateSut(User user) => new(
            _userRepositoryMock.Object,
            _passwordHasher,
            _currentUserContextMock.Object,
            user);

        private User CreateUser(string? lastThreePasswords = null) => new()
        {
            AccountName = "Screener",
            ProfilePassword = _passwordHasher.Hash(CurrentPassword),
            LastThreePasswords = lastThreePasswords ?? string.Empty
        };

        [Theory]
        [InlineData("Sh0rt!", false)]           // 6 chars — below the 8-character minimum
        [InlineData("LongEnough1!", true)]      // 12 chars
        public void NewPassword_GivenVariousLengths_WhenTyped_ThenHasMinimumLengthReflectsTheEightCharacterRule(
            string password, bool expected)
        {
            // Arrange
            var sut = CreateSut(CreateUser());

            // Act
            sut.NewPassword = password;

            // Assert
            Assert.Equal(expected, sut.HasMinimumLength);
        }

        [Theory]
        [InlineData("alllowercase1!", false)]
        [InlineData("HasUpper1!", true)]
        public void NewPassword_GivenPresenceOrAbsenceOfAnUppercaseLetter_WhenTyped_ThenHasUpperCaseReflectsIt(
            string password, bool expected)
        {
            // Arrange
            var sut = CreateSut(CreateUser());

            // Act
            sut.NewPassword = password;

            // Assert
            Assert.Equal(expected, sut.HasUpperCase);
        }

        [Theory]
        [InlineData("ALLUPPERCASE1!", false)]
        [InlineData("hasLower1!", true)]
        public void NewPassword_GivenPresenceOrAbsenceOfALowercaseLetter_WhenTyped_ThenHasLowerCaseReflectsIt(
            string password, bool expected)
        {
            // Arrange
            var sut = CreateSut(CreateUser());

            // Act
            sut.NewPassword = password;

            // Assert
            Assert.Equal(expected, sut.HasLowerCase);
        }

        [Theory]
        [InlineData("NoDigitsHere!", false)]
        [InlineData("HasDigit1!", true)]
        public void NewPassword_GivenPresenceOrAbsenceOfADigit_WhenTyped_ThenHasNumberReflectsIt(
            string password, bool expected)
        {
            // Arrange
            var sut = CreateSut(CreateUser());

            // Act
            sut.NewPassword = password;

            // Assert
            Assert.Equal(expected, sut.HasNumber);
        }

        [Theory]
        [InlineData("NoSpecialChar1", false)]
        [InlineData("HasSpecial1!", true)]
        public void NewPassword_GivenPresenceOrAbsenceOfASpecialCharacter_WhenTyped_ThenHasSpecialCharReflectsIt(
            string password, bool expected)
        {
            // Arrange
            var sut = CreateSut(CreateUser());

            // Act
            sut.NewPassword = password;

            // Assert
            Assert.Equal(expected, sut.HasSpecialChar);
        }

        [Fact]
        public void ConfirmPassword_GivenItDiffersFromTheNewPassword_WhenTyped_ThenPasswordsMatchIsFalse()
        {
            // Arrange
            var sut = CreateSut(CreateUser());

            // Act
            sut.NewPassword = "NewPassword1!";
            sut.ConfirmPassword = "SomethingElse1!";

            // Assert
            Assert.False(sut.PasswordsMatch);
        }

        [Fact]
        public void NewPassword_GivenItMatchesWhatWasTypedInOldPassword_WhenTyped_ThenNotSameAsOldIsFalse()
        {
            // Arrange — compares against the typed Old Password field, not the stored hash.
            var sut = CreateSut(CreateUser());

            // Act
            sut.OldPassword = "SamePassword1!";
            sut.NewPassword = "SamePassword1!";

            // Assert
            Assert.False(sut.NotSameAsOld);
        }

        [Fact]
        public void CanSave_GivenEveryPolicyRuleIsSatisfied_WhenEvaluated_ThenReturnsTrue()
        {
            // Arrange
            var sut = CreateSut(CreateUser());

            // Act
            sut.OldPassword = CurrentPassword;
            sut.NewPassword = "BrandNew1!";
            sut.ConfirmPassword = "BrandNew1!";

            // Assert
            Assert.True(sut.CanSave);
        }

        [Fact]
        public void CanSave_GivenOnlyOneRuleIsUnsatisfied_WhenEvaluated_ThenReturnsFalse()
        {
            // Arrange — missing a special character only.
            var sut = CreateSut(CreateUser());

            // Act
            sut.OldPassword = CurrentPassword;
            sut.NewPassword = "BrandNew1";
            sut.ConfirmPassword = "BrandNew1";

            // Assert
            Assert.False(sut.CanSave);
        }

        [Fact]
        public async Task SaveCommand_GivenTheWrongCurrentPassword_WhenSaved_ThenRejectsWithoutUpdatingTheRepository()
        {
            // Arrange
            var sut = CreateSut(CreateUser());
            sut.OldPassword = "WrongCurrentPassword1!";
            sut.NewPassword = "BrandNew1!";
            sut.ConfirmPassword = "BrandNew1!";

            // Act
            sut.SaveCommand.Execute(null);
            await Task.Delay(50); // SaveCommand's handler is async void via RelayCommand

            // Assert
            Assert.NotEmpty(sut.ErrorMessage);
            _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task SaveCommand_GivenTheNewPasswordMatchesOneOfTheLastThreePasswords_WhenSaved_ThenRejectsWithoutUpdatingTheRepository()
        {
            // Arrange
            const string previouslyUsedPassword = "PreviouslyUsed1!";
            string previousHash = _passwordHasher.Hash(previouslyUsedPassword);
            string olderHash1 = _passwordHasher.Hash("SomeOlderPassword1!");
            string olderHash2 = _passwordHasher.Hash("SomeEvenOlderPassword1!");
            var user = CreateUser(lastThreePasswords: string.Join("|", previousHash, olderHash1, olderHash2));
            var sut = CreateSut(user);

            sut.OldPassword = CurrentPassword;
            sut.NewPassword = previouslyUsedPassword;
            sut.ConfirmPassword = previouslyUsedPassword;

            // Act
            sut.SaveCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            Assert.NotEmpty(sut.ErrorMessage);
            _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task SaveCommand_GivenAValidNewPasswordNotInTheHistory_WhenSaved_ThenHashesItAndPersistsTheUpdatedUser()
        {
            // Arrange
            var user = CreateUser();
            var sut = CreateSut(user);
            sut.OldPassword = CurrentPassword;
            sut.NewPassword = "BrandNew1!";
            sut.ConfirmPassword = "BrandNew1!";

            // Act
            sut.SaveCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            Assert.True(_passwordHasher.Verify("BrandNew1!", user.ProfilePassword));
            Assert.Equal(0, user.FirstLogin);
            _userRepositoryMock.Verify(r => r.UpdateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task SaveCommand_GivenAValidNewPassword_WhenSaved_ThenPrependsTheOutgoingPasswordToHistoryAndKeepsOnlyThree()
        {
            // Arrange — history already has 3 entries; saving should drop the oldest.
            string hash1 = _passwordHasher.Hash("Older1!");
            string hash2 = _passwordHasher.Hash("Older2!");
            string hash3 = _passwordHasher.Hash("Older3!");
            var user = CreateUser(lastThreePasswords: string.Join("|", hash1, hash2, hash3));
            string outgoingHash = user.ProfilePassword;
            var sut = CreateSut(user);
            sut.OldPassword = CurrentPassword;
            sut.NewPassword = "BrandNew1!";
            sut.ConfirmPassword = "BrandNew1!";

            // Act
            sut.SaveCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            var history = user.LastThreePasswords.Split('|');
            Assert.Equal(3, history.Length);
            Assert.Equal(outgoingHash, history[0]);
            Assert.DoesNotContain(hash3, history);
        }

        [Fact]
        public async Task SaveCommand_GivenASuccessfulChange_WhenSaved_ThenRaisesPasswordChangeSucceededWithTheAccountNameAndCurrentProfileName()
        {
            // Arrange
            _currentUserContextMock.SetupGet(c => c.Profile).Returns(new Profile { Id = "Admin", Name = "Admin" });
            var user = CreateUser();
            var sut = CreateSut(user);
            sut.OldPassword = CurrentPassword;
            sut.NewPassword = "BrandNew1!";
            sut.ConfirmPassword = "BrandNew1!";

            (string AccountName, string ProfileName)? raised = null;
            sut.PasswordChangeSucceeded += (accountName, profileName) => raised = (accountName, profileName);

            // Act
            sut.SaveCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            Assert.Equal(("Screener", "Admin"), raised);
        }
    }
}
