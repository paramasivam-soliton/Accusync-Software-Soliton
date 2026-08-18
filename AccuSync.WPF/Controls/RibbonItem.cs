// --------------------------------------------------------------------------------
// <copyright file="RibbonItem.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace AccuSync.WPF.Controls
{
    /// <summary>
    /// Represents a single item (button, dropdown, or search field) within a ribbon group.
    /// Use the static <c>Create*</c> factory methods rather than constructing directly.
    /// </summary>
    // TODO: This class mixes properties for all item types (Button, Dropdown, SearchField).
    //       Consider splitting into subclasses or using a discriminated pattern so each
    //       type only exposes the properties it actually uses.
    public class RibbonItem
    {
        // Shared properties
        /// <summary>Identifier used to route clicks and as the default command parameter.</summary>
        public string Name { get; set; }
        /// <summary>Text displayed on the item.</summary>
        public string Label { get; set; }
        /// <summary>Geometry path data for the item's icon.</summary>
        public string IconData { get; set; }
        /// <summary>The kind of control this item renders as.</summary>
        public RibbonItemType ItemType { get; set; } = RibbonItemType.Button;
        /// <summary>Command invoked when the item is activated, if bound.</summary>
        public ICommand Command { get; set; }
        /// <summary>Parameter passed to <see cref="Command"/>; defaults to <see cref="Name"/> when not set.</summary>
        public object CommandParameter { get; set; }
        /// <summary>Whether the item can be interacted with.</summary>
        public bool IsEnabled { get; set; } = true;
        /// <summary>Whether the item is rendered in the toolbar.</summary>
        public bool IsVisible { get; set; } = true;

        // Dropdown-specific
        /// <summary>The selectable options for a dropdown item.</summary>
        public List<string> DropdownItems { get; set; }
        /// <summary>The currently selected option for a dropdown item.</summary>
        public string SelectedDropdownItem { get; set; }
        /// <summary>Callback invoked with the new value when a dropdown selection changes.</summary>
        public Action<string> DropdownSelectionChanged { get; set; }

        // SearchField-specific
        /// <summary>The current text entered into a search field item.</summary>
        public string SearchText { get; set; }
        /// <summary>Placeholder text shown in a search field item when empty.</summary>
        public string PlaceholderText { get; set; } = "Search...";
        /// <summary>Callback invoked with the entered text when a search is executed.</summary>
        public Action<string> SearchExecuted { get; set; }
        /// <summary>Callback invoked when a search field is cleared.</summary>
        public Action SearchCleared { get; set; }

        /// <summary>
        /// Creates a button item. If no <paramref name="commandParameter"/> is provided,
        /// the button's <see cref="Name"/> is used as the parameter so the ViewModel
        /// can identify which button was clicked.
        /// </summary>
        public static RibbonItem CreateButton(string name, string label, string iconData, ICommand command = null, object commandParameter = null)
        {
            return new RibbonItem
            {
                Name = name,
                Label = label,
                IconData = iconData,
                ItemType = RibbonItemType.Button,
                Command = command,
                CommandParameter = commandParameter ?? name
            };
        }

        /// <summary>
        /// Creates a dropdown item, preselecting the first entry in <paramref name="items"/> if any.
        /// </summary>
        /// <param name="name">Identifier for the item.</param>
        /// <param name="label">Text displayed on the item.</param>
        /// <param name="items">The selectable options.</param>
        /// <param name="selectionChanged">Callback invoked when the selection changes.</param>
        /// <returns>The created dropdown item.</returns>
        public static RibbonItem CreateDropdown(string name, string label, List<string> items, Action<string> selectionChanged = null)
        {
            return new RibbonItem
            {
                Name = name,
                Label = label,
                ItemType = RibbonItemType.Dropdown,
                DropdownItems = items,
                SelectedDropdownItem = items?.Count > 0 ? items[0] : null,
                DropdownSelectionChanged = selectionChanged
            };
        }

        /// <summary>Creates a search field item.</summary>
        /// <param name="name">Identifier for the item.</param>
        /// <param name="placeholder">Placeholder text shown when the field is empty.</param>
        /// <param name="searchExecuted">Callback invoked with the entered text when a search is executed.</param>
        /// <param name="searchCleared">Callback invoked when the field is cleared.</param>
        /// <returns>The created search field item.</returns>
        public static RibbonItem CreateSearchField(string name, string placeholder = "Search...", Action<string> searchExecuted = null, Action searchCleared = null)
        {
            return new RibbonItem
            {
                Name = name,
                Label = placeholder,
                ItemType = RibbonItemType.SearchField,
                PlaceholderText = placeholder,
                SearchExecuted = searchExecuted,
                SearchCleared = searchCleared
            };
        }

        /// <summary>Creates a visual separator item with no interactive content.</summary>
        /// <returns>The created separator item.</returns>
        public static RibbonItem CreateSeparator()
        {
            return new RibbonItem { ItemType = RibbonItemType.Separator };
        }
    }
}
