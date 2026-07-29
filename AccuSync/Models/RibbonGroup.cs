using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace AccuSync.Models
{
    /// <summary>
    /// Defines the type of control rendered in the ribbon toolbar.
    /// </summary>
    public enum RibbonItemType
    {
        Button,
        Dropdown,
        SearchField,
        Separator
    }

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
        public string Name { get; set; }
        public string Label { get; set; }
        public string IconData { get; set; }
        public RibbonItemType ItemType { get; set; } = RibbonItemType.Button;
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsVisible { get; set; } = true;

        // Dropdown-specific
        public List<string> DropdownItems { get; set; }
        public string SelectedDropdownItem { get; set; }
        public Action<string> DropdownSelectionChanged { get; set; }

        // SearchField-specific
        public string SearchText { get; set; }
        public string PlaceholderText { get; set; } = "Search...";
        public Action<string> SearchExecuted { get; set; }
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

        public static RibbonItem CreateSeparator()
        {
            return new RibbonItem { ItemType = RibbonItemType.Separator };
        }
    }

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

    /// <summary>
    /// Complete ribbon configuration for a screen or tab.
    /// Built by factory methods in <see cref="Models.RibbonDefinitions"/>.
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