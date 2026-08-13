// --------------------------------------------------------------------------------
// <copyright file="InactivityPolicy.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Threading;
using AccuSync.Core.Abstractions.Services;

namespace AccuSync.Application.Services.Authentication
{
    /// <summary>
    /// Framework-agnostic session-idle policy: raises <see cref="SessionExpired"/> when no
    /// activity has been reported for the idle timeout while a user is signed in. The default
    /// timeout is fixed for now; reading it from IAppSettingsRepository instead (matching how
    /// lockout duration is already configurable — see AuthenticationService) is future work.
    /// </summary>
    public class InactivityPolicy
    {
        public static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(15);

        private readonly ICurrentUserContext _currentUserContext;
        private readonly TimeSpan _idleTimeout;
        private readonly Timer _timer;

        /// <summary>Raised when the signed-in session has been idle for the configured timeout.</summary>
        public event Action SessionExpired;

        public InactivityPolicy(ICurrentUserContext currentUserContext)
            : this(currentUserContext, DefaultIdleTimeout)
        {
        }

        /// <summary>Overload used by tests to exercise the countdown without a real 15-minute wait.</summary>
        public InactivityPolicy(ICurrentUserContext currentUserContext, TimeSpan idleTimeout)
        {
            _currentUserContext = currentUserContext;
            _idleTimeout = idleTimeout;
            _timer = new Timer(OnElapsed, null, _idleTimeout, System.Threading.Timeout.InfiniteTimeSpan);
        }

        /// <summary>Call whenever activity is observed; restarts the countdown.</summary>
        public void NotifyActivity()
        {
            _timer.Change(_idleTimeout, System.Threading.Timeout.InfiniteTimeSpan);
        }

        private void OnElapsed(object state)
        {
            if (_currentUserContext.IsSignedIn)
            {
                SessionExpired?.Invoke();
            }

            _timer.Change(_idleTimeout, System.Threading.Timeout.InfiniteTimeSpan);
        }
    }
}
