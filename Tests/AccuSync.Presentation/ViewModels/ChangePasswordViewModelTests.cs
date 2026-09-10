// --------------------------------------------------------------------------------
// <copyright file="ChangePasswordViewModelTests.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Resources;
using AccuSync.Application.Services.Authentication;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;
using AccuSync.Presentation.ViewModels;
using Moq;

namespace AccuSync.Presentation.Tests.ViewModels
{
    /// <summary>
    /// IUserService/ICurrentUserContext are mocked, but PasswordHasher is real — so
    /// these tests verify the ViewModel's actual hash-verification and password-history
    /// behavior, not just that a mock returned what it was told to.
    /// </summary>
    public class ChangePasswordViewModelTests
    {
        private const string CurrentPassword = "Correct-Horse-9";

        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ICurrentUserContext> _currentUserContextMock;
        private readonly PasswordHasher _passwordHasher;
        private readonly User _currentUser;

        public ChangePasswordViewModelTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _currentUserContextMock = new Mock<ICurrentUserContext>();
            _currentUserContextMock.SetupGet(c => c.Role).Returns(UserRole.Screener);
            _passwordHasher = new PasswordHasher();
            _currentUser = new User
            {
                AccountName = "Screener",
                ProfilePassword = _passwordHasher.Hash(CurrentPassword)
            };

            _userServiceMock
                .Setup(s => s.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(true);
        }

        private ChangePasswordViewModel CreateChangePasswordViewModel(bool isPasswordExpiredReset = false, bool requireCurrentPassword = true)
        {
            return new ChangePasswordViewModel(_userServiceMock.Object, _passwordHasher, _currentUserContextMock.Object, _currentUser, isPasswordExpiredReset, requireCurrentPassword);
        }

        [Fact]
        public void Construct_ForAnExpiredPasswordReset_SetsAnInitialExpiryNoticeMessage()
        {
            var changePasswordViewModel = CreateChangePasswordViewModel(isPasswordExpiredReset: true);

            Assert.Equal(Strings.ChangePasswordViewModel_PasswordExpiredNotice, changePasswordViewModel.ErrorMessage);
        }

        [Fact]
        public void Construct_ForAnOrdinaryPasswordChange_DoesNotSetAnyInitialMessage()
        {
            var changePasswordViewModel = CreateChangePasswordViewModel();

            Assert.Equal(string.Empty, changePasswordViewModel.ErrorMessage);
        }

        [Fact]
        public async Task SavePasswordAsync_TheWrongCurrentPassword_SetsCurrentPasswordIncorrectAndDoesNotUpdateTheUser()
        {
            var changePasswordViewModel = CreateChangePasswordViewModel();
            changePasswordViewModel.OldPassword = "wrong-current-password";
            changePasswordViewModel.NewPassword = "NewPassword@123";
            changePasswordViewModel.ConfirmPassword = "NewPassword@123";

            await changePasswordViewModel.SavePasswordAsync();

            Assert.Equal("Current password is incorrect.", changePasswordViewModel.ErrorMessage);
            _userServiceMock.Verify(s => s.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task SavePasswordAsync_NewPasswordSameAsCurrentPassword_SetsPasswordSameAsCurrentAndDoesNotUpdateTheUser()
        {
            var changePasswordViewModel = CreateChangePasswordViewModel();
            changePasswordViewModel.OldPassword = CurrentPassword;
            changePasswordViewModel.NewPassword = CurrentPassword;
            changePasswordViewModel.ConfirmPassword = CurrentPassword;

            await changePasswordViewModel.SavePasswordAsync();

            Assert.Equal("New password must be different from your current password.", changePasswordViewModel.ErrorMessage);
            _userServiceMock.Verify(s => s.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void CanSave_SettingsFlowWithoutRequiringCurrentPassword_IgnoresTheEmptyOldPasswordField()
        {
            var changePasswordViewModel = CreateChangePasswordViewModel(requireCurrentPassword: false);

            changePasswordViewModel.NewPassword = "NewPassword@123";
            changePasswordViewModel.ConfirmPassword = "NewPassword@123";

            Assert.Equal(string.Empty, changePasswordViewModel.OldPassword);
            Assert.False(changePasswordViewModel.NotSameAsOld);
            Assert.True(changePasswordViewModel.CanSave);
        }

        [Fact]
        public async Task SavePasswordAsync_SettingsFlowWithoutRequiringCurrentPassword_UpdatesTheUserWithoutVerifyingOldPassword()
        {
            var changePasswordViewModel = CreateChangePasswordViewModel(requireCurrentPassword: false);
            changePasswordViewModel.NewPassword = "NewPassword@123";
            changePasswordViewModel.ConfirmPassword = "NewPassword@123";

            await changePasswordViewModel.SavePasswordAsync();

            Assert.Equal(string.Empty, changePasswordViewModel.ErrorMessage);
            Assert.True(_passwordHasher.Verify("NewPassword@123", _currentUser.ProfilePassword));
            _userServiceMock.Verify(s => s.UpdateUserAsync(_currentUser), Times.Once);
        }

        [Fact]
        public async Task SavePasswordAsync_SettingsFlowWithNewPasswordSameAsCurrent_SetsPasswordSameAsCurrentAndDoesNotUpdateTheUser()
        {
            var changePasswordViewModel = CreateChangePasswordViewModel(requireCurrentPassword: false);
            changePasswordViewModel.NewPassword = CurrentPassword;
            changePasswordViewModel.ConfirmPassword = CurrentPassword;

            await changePasswordViewModel.SavePasswordAsync();

            Assert.Equal("New password must be different from your current password.", changePasswordViewModel.ErrorMessage);
            _userServiceMock.Verify(s => s.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task SavePasswordAsync_ANewPasswordMatchingOneOfTheLastThree_SetsPasswordReusedAndDoesNotUpdateTheUser()
        {
            string reusedHash = _passwordHasher.Hash("Old-Password-1");
            _currentUser.LastThreePasswords = reusedHash;

            var changePasswordViewModel = CreateChangePasswordViewModel();
            changePasswordViewModel.OldPassword = CurrentPassword;
            changePasswordViewModel.NewPassword = "Old-Password-1";
            changePasswordViewModel.ConfirmPassword = "Old-Password-1";

            await changePasswordViewModel.SavePasswordAsync();

            Assert.Equal("Password cannot be the same as your last three passwords.", changePasswordViewModel.ErrorMessage);
            _userServiceMock.Verify(s => s.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task SavePasswordAsync_ValidPasswordChangeWithNonDefaultRole_UpdatesTheUserAndRaisesPasswordChangeSucceededWithAccountNameAndRole()
        {
            _currentUserContextMock.SetupGet(c => c.Role).Returns(UserRole.Admin);
            var changePasswordViewModel = CreateChangePasswordViewModel();
            changePasswordViewModel.OldPassword = CurrentPassword;
            changePasswordViewModel.NewPassword = "NewPassword@123";
            changePasswordViewModel.ConfirmPassword = "NewPassword@123";

            (string AccountName, string Role)? raised = null;
            changePasswordViewModel.PasswordChangeSucceeded += (accountName, role) => raised = (accountName, role);

            await changePasswordViewModel.SavePasswordAsync();

            Assert.Equal(("Screener", nameof(UserRole.Admin)), raised);
            Assert.True(_passwordHasher.Verify("NewPassword@123", _currentUser.ProfilePassword));
            Assert.Equal(0, _currentUser.FirstLogin);
            _userServiceMock.Verify(s => s.UpdateUserAsync(_currentUser), Times.Once);
        }

        [Fact]
        public async Task SavePasswordAsync_ValidNewPassword_PrependsTheOutgoingPasswordToHistoryAndKeepsOnlyThree()
        {
            string hash1 = _passwordHasher.Hash("Older1!");
            string hash2 = _passwordHasher.Hash("Older2!");
            string hash3 = _passwordHasher.Hash("Older3!");
            _currentUser.LastThreePasswords = string.Join("|", hash1, hash2, hash3);
            string outgoingHash = _currentUser.ProfilePassword;

            var changePasswordViewModel = CreateChangePasswordViewModel();
            changePasswordViewModel.OldPassword = CurrentPassword;
            changePasswordViewModel.NewPassword = "BrandNew1!";
            changePasswordViewModel.ConfirmPassword = "BrandNew1!";

            await changePasswordViewModel.SavePasswordAsync();

            var history = _currentUser.LastThreePasswords.Split('|');
            Assert.Equal(3, history.Length);
            Assert.Equal(outgoingHash, history[0]);
            Assert.DoesNotContain(hash3, history);
        }

        [Fact]
        public async Task SavePasswordAsync_UserServiceReportsFailure_SetsUpdateFailedError()
        {
            _userServiceMock
                .Setup(s => s.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(false);

            var changePasswordViewModel = CreateChangePasswordViewModel();
            changePasswordViewModel.OldPassword = CurrentPassword;
            changePasswordViewModel.NewPassword = "NewPassword@123";
            changePasswordViewModel.ConfirmPassword = "NewPassword@123";

            await changePasswordViewModel.SavePasswordAsync();

            Assert.Equal("Failed to update password. Please try again.", changePasswordViewModel.ErrorMessage);
        }

        [Fact]
        public async Task SavePasswordAsync_TheUserServiceThrows_SetsGenericUnexpectedErrorMessage()
        {
            _userServiceMock
                .Setup(s => s.UpdateUserAsync(It.IsAny<User>()))
                .ThrowsAsync(new InvalidOperationException("Simulated failure."));

            var changePasswordViewModel = CreateChangePasswordViewModel();
            changePasswordViewModel.OldPassword = CurrentPassword;
            changePasswordViewModel.NewPassword = "NewPassword@123";
            changePasswordViewModel.ConfirmPassword = "NewPassword@123";

            await changePasswordViewModel.SavePasswordAsync();

            Assert.Equal(
                string.Format(Strings.ChangePasswordViewModel_UnexpectedError, ErrorCode.Unexpected.ToDisplayCode()),
                changePasswordViewModel.ErrorMessage);
            Assert.DoesNotContain("Simulated failure.", changePasswordViewModel.ErrorMessage);
        }

        [Theory]
        [InlineData("Short1!", false)]
        [InlineData("nouppercase1!", false)]
        [InlineData("NOLOWERCASE1!", false)]
        [InlineData("NoNumber!!", false)]
        [InlineData("NoSpecialChar1", false)]
        [InlineData("ValidPass1!", true)]
        public void CanSave_NewPasswordTyped_ReflectsWhetherEveryPolicyRulePasses(
            string newPassword, bool expectedPolicyPassesBeforeMatchCheck)
        {
            var changePasswordViewModel = CreateChangePasswordViewModel();
            changePasswordViewModel.OldPassword = CurrentPassword;

            changePasswordViewModel.NewPassword = newPassword;
            changePasswordViewModel.ConfirmPassword = newPassword;

            Assert.Equal(expectedPolicyPassesBeforeMatchCheck, changePasswordViewModel.CanSave);
        }

        [Theory]
        [InlineData("Sh0rt!", false)]
        [InlineData("LongEnough1!", true)]
        public void HasMinimumLength_VariousLengths_ReflectsTheEightCharacterRule(
            string password, bool expected)
        {
            var changePasswordViewModel = CreateChangePasswordViewModel();

            changePasswordViewModel.NewPassword = password;

            Assert.Equal(expected, changePasswordViewModel.HasMinimumLength);
        }

        [Theory]
        [InlineData("alllowercase1!", false)]
        [InlineData("HasUpper1!", true)]
        public void HasUpperCase_PresenceOrAbsenceOfAnUppercaseLetter_ReflectsIt(
            string password, bool expected)
        {
            var changePasswordViewModel = CreateChangePasswordViewModel();

            changePasswordViewModel.NewPassword = password;

            Assert.Equal(expected, changePasswordViewModel.HasUpperCase);
        }

        [Theory]
        [InlineData("ALLUPPERCASE1!", false)]
        [InlineData("hasLower1!", true)]
        public void HasLowerCase_PresenceOrAbsenceOfALowercaseLetter_ReflectsIt(
            string password, bool expected)
        {
            var changePasswordViewModel = CreateChangePasswordViewModel();

            changePasswordViewModel.NewPassword = password;

            Assert.Equal(expected, changePasswordViewModel.HasLowerCase);
        }

        [Theory]
        [InlineData("NoDigitsHere!", false)]
        [InlineData("HasDigit1!", true)]
        public void HasNumber_PresenceOrAbsenceOfADigit_ReflectsIt(
            string password, bool expected)
        {
            var changePasswordViewModel = CreateChangePasswordViewModel();

            changePasswordViewModel.NewPassword = password;

            Assert.Equal(expected, changePasswordViewModel.HasNumber);
        }

        [Theory]
        [InlineData("NoSpecialChar1", false)]
        [InlineData("HasSpecial1!", true)]
        public void HasSpecialChar_PresenceOrAbsenceOfASpecialCharacter_ReflectsIt(
            string password, bool expected)
        {
            var changePasswordViewModel = CreateChangePasswordViewModel();

            changePasswordViewModel.NewPassword = password;

            Assert.Equal(expected, changePasswordViewModel.HasSpecialChar);
        }

        [Fact]
        public void CanSave_NewPasswordIdenticalToTheOldPasswordAsTyped_IsFalse()
        {
            var changePasswordViewModel = CreateChangePasswordViewModel();
            changePasswordViewModel.OldPassword = CurrentPassword;

            changePasswordViewModel.NewPassword = CurrentPassword;
            changePasswordViewModel.ConfirmPassword = CurrentPassword;

            Assert.False(changePasswordViewModel.CanSave);
            Assert.False(changePasswordViewModel.NotSameAsOld);
        }

        [Fact]
        public void PasswordsMatch_ConfirmPasswordDoesNotMatchNewPassword_IsFalseAndCanSaveIsFalse()
        {
            var changePasswordViewModel = CreateChangePasswordViewModel();
            changePasswordViewModel.OldPassword = CurrentPassword;

            changePasswordViewModel.NewPassword = "ValidPass1!";
            changePasswordViewModel.ConfirmPassword = "ValidPass2!";

            Assert.False(changePasswordViewModel.PasswordsMatch);
            Assert.False(changePasswordViewModel.CanSave);
        }
    }
}
