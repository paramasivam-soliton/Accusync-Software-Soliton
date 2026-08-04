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
        public List<RibbonGroup> Groups { get; set; } = new List<RibbonGroup>();

        public RibbonDefinition() { }

        public RibbonDefinition(params RibbonGroup[] groups)
        {
            Groups = new List<RibbonGroup>(groups);
        }
    }
}
