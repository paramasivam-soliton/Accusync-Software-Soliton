// --------------------------------------------------------------------------------
// <copyright file="WpfApplicationLifecycle.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Presentation.Abstractions;

namespace AccuSync.WPF.Services
{
    /// <summary>WPF-backed <see cref="IApplicationLifecycle"/> — the one place a ViewModel's shutdown request reaches the real WPF Application.</summary>
    public class WpfApplicationLifecycle : IApplicationLifecycle
    {
        // Fully qualified: bare "Application" here would resolve to the sibling
        // AccuSync.Application namespace, not System.Windows.Application.
        public void Shutdown() => System.Windows.Application.Current.Shutdown();
    }
}
