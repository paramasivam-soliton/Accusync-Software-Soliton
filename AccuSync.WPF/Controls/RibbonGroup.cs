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
        /// <summary>Text shown under the group's items in the ribbon.</summary>
        public string GroupLabel { get; set; }

        /// <summary>The items (buttons, dropdowns, search fields) contained in this group.</summary>
        public List<RibbonItem> Items { get; set; } = new List<RibbonItem>();

        /// <summary>Creates an empty ribbon group with no label or items.</summary>
        public RibbonGroup() { }

        /// <summary>Creates a ribbon group with the given label and items, in order.</summary>
        /// <param name="label">Text shown under the group's items.</param>
        /// <param name="items">The items to include in the group.</param>
        public RibbonGroup(string label, params RibbonItem[] items)
        {
            GroupLabel = label;
            Items = new List<RibbonItem>(items);
        }
    }
}
