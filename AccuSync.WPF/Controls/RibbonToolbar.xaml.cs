// --------------------------------------------------------------------------------
// <copyright file="RibbonToolbar.xaml.cs" company="AccuSync">
//     Copyright (c) 2026 AccuSync. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AccuSync.WPF.Controls
{
    /// <summary>
    /// Dynamically builds the ribbon toolbar UI from a <see cref="RibbonDefinition"/>.
    /// Each group renders as a pill-shaped container with icon+text buttons inside.
    /// Raises <see cref="ItemClicked"/> for buttons that don't have an ICommand bound.
    /// </summary>
    public partial class RibbonToolbar : UserControl
    {
        /// <summary>
        /// Raised when a toolbar button without an <see cref="ICommand"/> is clicked.
        /// Carries the item's <see cref="RibbonItem.Name"/> so the parent view can
        /// route the action.
        /// </summary>
        public event EventHandler<RibbonItemClickEventArgs> ItemClicked;

        public RibbonToolbar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Clears the current toolbar and rebuilds it from <paramref name="definition"/>.
        /// Call this when the active tab or screen changes.
        /// </summary>
        public void LoadDefinition(RibbonDefinition definition)
        {
            GroupsPanel.Children.Clear();
            if (definition?.Groups == null) return;

            for (int g = 0; g < definition.Groups.Count; g++)
            {
                var group = definition.Groups[g];

                if (group.Items.TrueForAll(i => !i.IsVisible)) continue;

                var groupWrapper = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                var pillBorder = new Border
                {
                    Background = (SolidColorBrush)FindResource("ToolbarGroupBackgroundBrush"),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(8, 8, 8, 4)
                };

                var pillContent = new StackPanel();

                var itemsRow = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                foreach (var item in group.Items)
                {
                    if (!item.IsVisible) continue;

                    switch (item.ItemType)
                    {
                        case RibbonItemType.Button:
                            itemsRow.Children.Add(CreateButton(item));
                            break;
                        case RibbonItemType.Dropdown:
                            itemsRow.Children.Add(CreateDropdown(item));
                            break;
                        case RibbonItemType.SearchField:
                            itemsRow.Children.Add(CreateSearchField(item));
                            break;
                        case RibbonItemType.Separator:
                            itemsRow.Children.Add(new Border
                            {
                                Width = 6,
                                Background = Brushes.Transparent
                            });
                            break;
                    }
                }

                pillContent.Children.Add(itemsRow);

                // Horizontal divider between buttons and group label
                pillContent.Children.Add(new Border
                {
                    Height = 1,
                    Background = (SolidColorBrush)FindResource("ToolbarBorderBrush"),
                    Margin = new Thickness(2, 4, 2, 0)
                });

                var label = new TextBlock
                {
                    Text = group.GroupLabel,
                    Style = (Style)FindResource("RibbonGroupLabelStyle")
                };
                pillContent.Children.Add(label);

                pillBorder.Child = pillContent;
                groupWrapper.Children.Add(pillBorder);

                GroupsPanel.Children.Add(groupWrapper);
            }
        }

        // Button uses Tag to carry icon path data — see RibbonButtonStyle template
        // which binds Path.Data to TemplatedParent.Tag.
        private Button CreateButton(RibbonItem item)
        {
            var btn = new Button
            {
                Content = item.Label,
                Tag = item.IconData,
                Style = (Style)FindResource("RibbonButtonStyle"),
                IsEnabled = item.IsEnabled
            };

            // Prefer ICommand when available; fall back to the ItemClicked event
            // so the parent view can handle it via a single handler.
            if (item.Command != null)
            {
                btn.Command = item.Command;
                btn.CommandParameter = item.CommandParameter ?? item.Name;
            }
            else
            {
                btn.Click += (s, e) =>
                {
                    ItemClicked?.Invoke(this, new RibbonItemClickEventArgs(item.Name, item));
                };
            }

            return btn;
        }

        private FrameworkElement CreateDropdown(RibbonItem item)
        {
            var combo = new ComboBox
            {
                Style = (Style)FindResource("RibbonDropdownStyle"),
                ItemsSource = item.DropdownItems,
                SelectedItem = item.SelectedDropdownItem,
                Margin = new Thickness(2, 0, 2, 0)
            };

            combo.SelectionChanged += (s, e) =>
            {
                if (combo.SelectedItem is string selected)
                {
                    item.SelectedDropdownItem = selected;
                    item.DropdownSelectionChanged?.Invoke(selected);
                    ItemClicked?.Invoke(this, new RibbonItemClickEventArgs(item.Name, item));
                }
            };

            return combo;
        }

        // TODO: Placeholder logic uses color comparison to distinguish placeholder
        //       text from real input. This is fragile — if the placeholder color
        //       changes in the style, the GotFocus/LostFocus/KeyDown checks break
        //       silently. Consider using a bool flag or a separate TextBlock overlay
        //       (like FirmwareUpdateDialog does).
        private FrameworkElement CreateSearchField(RibbonItem item)
        {
            var container = new Grid
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 2, 0)
            };

            var placeholderColor = (Color)ColorConverter.ConvertFromString("#9ca3af");
            var placeholderBrush = new SolidColorBrush(placeholderColor);

            var textBox = new TextBox
            {
                Style = (Style)FindResource("RibbonSearchTextBoxStyle"),
                Tag = item.PlaceholderText,
                Text = item.PlaceholderText,
                Foreground = placeholderBrush
            };

            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Foreground is SolidColorBrush brush && brush.Color == placeholderColor)
                {
                    textBox.Text = "";
                    textBox.Foreground = new SolidColorBrush(Colors.Black);
                }
            };
            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = item.PlaceholderText;
                    textBox.Foreground = placeholderBrush;
                }
            };

            textBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && textBox.Foreground is SolidColorBrush b && b.Color != placeholderColor)
                {
                    item.SearchText = textBox.Text;
                    item.SearchExecuted?.Invoke(textBox.Text);
                    ItemClicked?.Invoke(this, new RibbonItemClickEventArgs(item.Name, item));
                }
            };

            container.Children.Add(textBox);

            var clearBtn = new Button
            {
                Content = "✕",
                FontSize = 11,
                Foreground = placeholderBrush,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Width = 20,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0),
                Cursor = Cursors.Hand,
                Visibility = Visibility.Collapsed
            };

            clearBtn.Click += (s, e) =>
            {
                textBox.Text = item.PlaceholderText;
                textBox.Foreground = placeholderBrush;
                item.SearchText = "";
                item.SearchCleared?.Invoke();
                clearBtn.Visibility = Visibility.Collapsed;
            };

            textBox.TextChanged += (s, e) =>
            {
                bool hasText = !string.IsNullOrWhiteSpace(textBox.Text) &&
                               textBox.Text != item.PlaceholderText;
                clearBtn.Visibility = hasText ? Visibility.Visible : Visibility.Collapsed;
            };

            container.Children.Add(clearBtn);
            return container;
        }
    }
}