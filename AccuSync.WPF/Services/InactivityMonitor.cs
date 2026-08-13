// --------------------------------------------------------------------------------
// <copyright file="InactivityMonitor.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Linq;
using System.Windows;
using System.Windows.Input;
using AccuSync.Application.Services.Authentication;

namespace AccuSync.WPF.Services
{
    /// <summary>
    /// WPF-side adapter for <see cref="InactivityPolicy"/> — the policy itself is
    /// framework-agnostic and knows nothing about windows or input events. This class only
    /// reports activity from every window in the app and signs the user out (via
    /// <see cref="App.Logout"/>) when the policy signals the session has expired.
    /// </summary>
    public class InactivityMonitor
    {
        private readonly InactivityPolicy _policy;

        public InactivityMonitor(InactivityPolicy policy)
        {
            _policy = policy;
            _policy.SessionExpired += OnSessionExpired;
        }

        /// <summary>
        /// Hooks input events on every window in the app (current and future — class
        /// handlers apply app-wide). Call once at startup.
        /// </summary>
        public void Start()
        {
            EventManager.RegisterClassHandler(typeof(Window), UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler((_, _) => _policy.NotifyActivity()));
            EventManager.RegisterClassHandler(typeof(Window), UIElement.PreviewMouseMoveEvent, new MouseEventHandler((_, _) => _policy.NotifyActivity()));
            EventManager.RegisterClassHandler(typeof(Window), UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler((_, _) => _policy.NotifyActivity()));
            EventManager.RegisterClassHandler(typeof(Window), UIElement.PreviewKeyDownEvent, new KeyEventHandler((_, _) => _policy.NotifyActivity()));
        }

        // The policy's timer callback runs on a thread-pool thread, not the UI thread —
        // unlike a DispatcherTimer, so window access here must be marshaled explicitly.
        private void OnSessionExpired()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Window activeWindow = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                    ?? System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault();

                if (activeWindow != null)
                {
                    App.Logout(activeWindow);
                }
            });
        }
    }
}
