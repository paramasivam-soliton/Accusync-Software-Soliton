// --------------------------------------------------------------------------------
// <copyright file="RibbonGroup.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AccuSync.WPF.Controls
{
    /// <summary>
    /// A labeled group of items within the ribbon toolbar.
    /// Groups are visually separated by vertical dividers.
    /// </summary>
    public class RibbonGroup
    {
        public string GroupLabel { get; set; }
        public List<RibbonItem> Items { get; set; } = new List<RibbonItem>();

        public RibbonGroup() { }

        public RibbonGroup(string label, params RibbonItem[] items)
        {
            GroupLabel = label;
            Items = new List<RibbonItem>(items);
        }
    }
}
