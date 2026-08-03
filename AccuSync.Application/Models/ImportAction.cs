// --------------------------------------------------------------------------------
// <copyright file="ImportAction.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    /// <summary>
    /// The action to take for a patient on import. The first value (<see cref="Import"/>)
    /// applies to new patients; the rest are the choices offered for a duplicate.
    /// </summary>
    public enum ImportAction
    {
        Import,
        Replace,
        Create,
        AddTests,
        Skip
    }
}
