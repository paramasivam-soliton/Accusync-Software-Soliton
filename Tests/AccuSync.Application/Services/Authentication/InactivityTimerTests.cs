// --------------------------------------------------------------------------------
// <copyright file="InactivityTimerTests.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Services.Authentication;
using AccuSync.Core.Abstractions.Services;
using Moq;

namespace AccuSync.Application.Tests.Services.Authentication
{
    /// <summary>
    /// Uses a short idle timeout (milliseconds, passed to the optional constructor
    /// parameter) instead of the real 15-minute default, so the countdown can be
    /// exercised directly.
    /// </summary>
    public class InactivityTimerTests
    {
        private static readonly TimeSpan ShortIdleTimeout = TimeSpan.FromMilliseconds(50);

        private readonly Mock<ICurrentUserContext> _currentUserContextMock;

        public InactivityTimerTests()
        {
            _currentUserContextMock = new Mock<ICurrentUserContext>();
        }

        private InactivityTimer CreateInactivityTimer() => new(_currentUserContextMock.Object, ShortIdleTimeout);

        [Fact]
        public async Task SessionExpired_SignedInSessionWithNoActivityForTheIdleTimeout_IsRaised()
        {
            _currentUserContextMock.SetupGet(c => c.IsSignedIn).Returns(true);
            var inactivityTimer = CreateInactivityTimer();
            bool raised = false;
            inactivityTimer.SessionExpired += () => raised = true;

            await Task.Delay(150);

            Assert.True(raised);
        }

        // [Fact]
        // public async Task SessionExpired_NoOneSignedInWhenTheIdleTimeoutElapses_IsNotRaised()
        // {
        //     _currentUserContextMock.SetupGet(c => c.IsSignedIn).Returns(false);
        //     var inactivityTimer = CreateInactivityTimer();
        //     bool raised = false;
        //     inactivityTimer.SessionExpired += () => raised = true;

        //     await Task.Delay(150);

        //     Assert.False(raised);
        // }

        // [Fact]
        // public async Task SessionExpired_ActivityReportedBeforeTheIdleTimeoutThenOriginalDeadlinePasses_IsNotYetRaised()
        // {
        //     _currentUserContextMock.SetupGet(c => c.IsSignedIn).Returns(true);
        //     var inactivityTimer = CreateInactivityTimer();
        //     bool raised = false;
        //     inactivityTimer.SessionExpired += () => raised = true;

        //     await Task.Delay(25);
        //     inactivityTimer.NotifyActivity();
        //     await Task.Delay(40);

        //     Assert.False(raised);
        // }

        // [Fact]
        // public async Task SessionExpired_ActivityWasReportedAndTheRenewedIdleTimeoutElapses_IsRaised()
        // {
        //     _currentUserContextMock.SetupGet(c => c.IsSignedIn).Returns(true);
        //     var inactivityTimer = CreateInactivityTimer();
        //     bool raised = false;
        //     inactivityTimer.SessionExpired += () => raised = true;

        //     await Task.Delay(25);
        //     inactivityTimer.NotifyActivity();
        //     await Task.Delay(100);

        //     Assert.True(raised);
        // }
    }
}
