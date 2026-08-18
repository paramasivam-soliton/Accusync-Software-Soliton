// --------------------------------------------------------------------------------
// <copyright file="InactivityPolicy.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Timers;
using AccuSync.Core.Abstractions.Services;

namespace AccuSync.Application.Services.Authentication
{
    /// <summary>
    /// Framework-agnostic session-idle policy: raises <see cref="SessionExpired"/> when no
    /// activity has been reported for the idle timeout while a user is signed in. The default
    /// timeout is fixed for now; reading it from IAppSettingsRepository instead (matching how
    /// lockout duration is already configurable — see AuthenticationService) is future work.
    /// </summary>
    public sealed class InactivityPolicy : IDisposable
    {
        public static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(15);

        private readonly ICurrentUserContext _currentUserContext;
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

            // AutoReset means the countdown re-arms itself after every Elapsed — including the
            // "still idle, still signed in" case in OnElapsed — without needing to manually
            // restart it there, unlike System.Threading.Timer's one-shot-per-Change model.
            _timer = new Timer(idleTimeout.TotalMilliseconds) { AutoReset = true };
            _timer.Elapsed += OnElapsed;
            _timer.Start();
        }

        /// <summary>Call whenever activity is observed; restarts the countdown.</summary>
        public void NotifyActivity()
        {
            // Stop then Start (rather than just re-setting Enabled/Interval) is what actually
            // restarts the elapsed-time countdown from zero.
            _timer.Stop();
            _timer.Start();
        }

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_currentUserContext.IsSignedIn)
            {
                SessionExpired?.Invoke();
            }
        }

        /// <summary>
        /// Stops and releases the underlying timer. Without this, registering
        /// <see cref="InactivityPolicy"/> as a DI singleton leaves the timer running
        /// until process exit — disposing the container alone doesn't reach it.
        /// </summary>
        public void Dispose()
        {
            _timer.Elapsed -= OnElapsed;
            _timer.Dispose();
        }
    }
}
