// --------------------------------------------------------------------------------
// <copyright file="InactivityPolicy.test.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Services.Authentication;
using AccuSync.Core.Abstractions.Services;
using Moq;

namespace AccuSync.Application.Tests.Services.Authentication
{
    /// <summary>
    /// Uses a short idle timeout (milliseconds, via the test-only constructor overload)
    /// instead of the real 15-minute default, so the countdown can be exercised directly.
    /// </summary>
    public class InactivityPolicyTests
    {
        private static readonly TimeSpan ShortIdleTimeout = TimeSpan.FromMilliseconds(50);

        private readonly Mock<ICurrentUserContext> _currentUserContextMock;

        public InactivityPolicyTests()
        {
            _currentUserContextMock = new Mock<ICurrentUserContext>();
        }

        private InactivityPolicy CreateSut() => new(_currentUserContextMock.Object, ShortIdleTimeout);

        [Fact]
        public async Task GivenASignedInSession_WhenNoActivityIsReportedForTheIdleTimeout_ThenSessionExpiredIsRaised()
        {
            // Arrange
            _currentUserContextMock.SetupGet(c => c.IsSignedIn).Returns(true);
            var sut = CreateSut();
            bool raised = false;
            sut.SessionExpired += () => raised = true;

            // Act
            await Task.Delay(150);

            // Assert
            Assert.True(raised);
        }

        [Fact]
        public async Task GivenNoOneSignedIn_WhenTheIdleTimeoutElapses_ThenSessionExpiredIsNotRaised()
        {
            // Arrange
            _currentUserContextMock.SetupGet(c => c.IsSignedIn).Returns(false);
            var sut = CreateSut();
            bool raised = false;
            sut.SessionExpired += () => raised = true;

            // Act
            await Task.Delay(150);

            // Assert
            Assert.False(raised);
        }

        [Fact]
        public async Task GivenActivityIsReportedBeforeTheIdleTimeout_WhenTheOriginalDeadlinePasses_ThenSessionExpiredIsNotYetRaised()
        {
            // Arrange — activity partway through should push the deadline out, not just delay it slightly.
            _currentUserContextMock.SetupGet(c => c.IsSignedIn).Returns(true);
            var sut = CreateSut();
            bool raised = false;
            sut.SessionExpired += () => raised = true;

            // Act
            await Task.Delay(25);
            sut.NotifyActivity();
            await Task.Delay(40); // past the original 50ms deadline, before the renewed one

            // Assert
            Assert.False(raised);
        }

        [Fact]
        public async Task GivenActivityWasReported_WhenTheRenewedIdleTimeoutElapses_ThenSessionExpiredIsRaised()
        {
            // Arrange
            _currentUserContextMock.SetupGet(c => c.IsSignedIn).Returns(true);
            var sut = CreateSut();
            bool raised = false;
            sut.SessionExpired += () => raised = true;

            // Act
            await Task.Delay(25);
            sut.NotifyActivity();
            await Task.Delay(100);

            // Assert
            Assert.True(raised);
        }
    }
}
