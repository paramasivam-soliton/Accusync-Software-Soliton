// --------------------------------------------------------------------------------
// <copyright file="InactivityTimer.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Timers;
using AccuSync.Core.Abstractions.Services;

namespace AccuSync.Application.Services.Authentication
{
    /// <summary>
    /// Framework-agnostic session-idle timer: raises <see cref="SessionExpired"/> when no
    /// activity has been reported for the idle timeout while a user is signed in. The default
    /// timeout is fixed for now; reading it from IAppSettingsRepository instead (matching how
    /// lockout duration is already configurable — see AuthenticationService) is future work.
    /// </summary>
    public sealed class InactivityTimer : IDisposable
    {
        private readonly ICurrentUserContext _currentUserContext;
        private readonly Timer _timer;

        /// <summary>Creates the timer with the given idle timeout, or <see cref="DefaultIdleTimeout"/> if omitted.</summary>
        /// <param name="currentUserContext">Checked on each timeout to determine whether a session is actually active.</param>
        /// <param name="idleTimeout">How long the session may be idle before <see cref="SessionExpired"/> is raised. Tests pass a short value to exercise the countdown without a real 15-minute wait.</param>
        public InactivityTimer(ICurrentUserContext currentUserContext, TimeSpan? idleTimeout = null)
        {
            _currentUserContext = currentUserContext;

            // AutoReset means the countdown re-arms itself after every Elapsed — including the
            // "still idle, still signed in" case in OnElapsed — without needing to manually
            // restart it there, unlike System.Threading.Timer's one-shot-per-Change model.
            _timer = new Timer((idleTimeout ?? DefaultIdleTimeout).TotalMilliseconds) { AutoReset = true };
            _timer.Elapsed += OnElapsed;
            _timer.Start();
        }

        /// <summary>The idle duration after which a signed-in session is treated as expired.</summary>
        public static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(15);

        /// <summary>Raised when the signed-in session has been idle for the configured timeout.</summary>
        public event Action SessionExpired;

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
        /// <see cref="InactivityTimer"/> as a DI singleton leaves the timer running
        /// until process exit — disposing the container alone doesn't reach it.
        /// </summary>
        public void Dispose()
        {
            _timer.Elapsed -= OnElapsed;
            _timer.Dispose();
        }
    }
}
