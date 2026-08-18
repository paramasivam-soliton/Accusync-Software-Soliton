// --------------------------------------------------------------------------------
// <copyright file="RibbonDefinition.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AccuSync.WPF.Controls
{
    /// <summary>
    /// Complete ribbon configuration for a screen or tab.
    /// Built by factory methods in <see cref="RibbonDefinitions"/>.
    /// </summary>
    public class RibbonDefinition
    {
        /// <summary>The ordered groups of items shown in the ribbon.</summary>
        public List<RibbonGroup> Groups { get; set; } = new List<RibbonGroup>();

        /// <summary>Creates an empty ribbon definition with no groups.</summary>
        public RibbonDefinition() { }

        /// <summary>Creates a ribbon definition from the given groups, in order.</summary>
        /// <param name="groups">The groups to include in the ribbon.</param>
        public RibbonDefinition(params RibbonGroup[] groups)
        {
            Groups = new List<RibbonGroup>(groups);
        }
    }
}
