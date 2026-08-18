// --------------------------------------------------------------------------------
// <copyright file="IApplicationLifecycle.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Presentation.Abstractions
{
    /// <summary>
    /// Lets a ViewModel end the application without referencing WPF directly.
    /// Implemented by AccuSync.WPF.
    /// </summary>
    public interface IApplicationLifecycle
    {
        /// <summary>Shuts the application down.</summary>
        void Shutdown();
    }
}
