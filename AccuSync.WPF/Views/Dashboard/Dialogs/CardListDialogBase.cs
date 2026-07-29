// --------------------------------------------------------------------------------
// <copyright file="CardListDialogBase.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AccuSync.WPF.Views.Dashboard.Dialogs
{
    /// <summary>
    /// Shared base for the card-list dialogs (AssignedPatientsDialog,
    /// CompletedScreeningsDialog), which were ~95% identical. Holds the behavior
    /// that doesn't depend on the item type: three-state (loading/list/empty)
    /// visibility, search placeholder handling, card hover animation, and close.
    /// Derived dialogs expose their named XAML elements via the abstract accessors.
    /// </summary>
    public abstract class CardListDialogBase : Window
    {
        // Named XAML elements differ between dialogs (e.g. PatientListView vs
        // ScreeningListView), so derived classes surface them here.
        protected abstract UIElement LoadingElement { get; }
        protected abstract UIElement ListElement { get; }
        protected abstract UIElement EmptyElement { get; }
        protected abstract TextBox SearchInput { get; }
        protected abstract string SearchPlaceholder { get; }

        // Three-state visibility

        protected void ShowLoadingState()
        {
            LoadingElement.Visibility = Visibility.Visible;
            ListElement.Visibility = Visibility.Collapsed;
            EmptyElement.Visibility = Visibility.Collapsed;
        }

        protected void ShowEmptyState()
        {
            LoadingElement.Visibility = Visibility.Collapsed;
            ListElement.Visibility = Visibility.Collapsed;
            EmptyElement.Visibility = Visibility.Visible;
        }

        protected void ShowList()
        {
            LoadingElement.Visibility = Visibility.Collapsed;
            ListElement.Visibility = Visibility.Visible;
            EmptyElement.Visibility = Visibility.Collapsed;
        }

        // Search placeholder (no watermark adorner — bare text swap)

        protected void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchInput.Text == SearchPlaceholder)
                SearchInput.Text = "";
        }

        protected void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchInput.Text))
                SearchInput.Text = SearchPlaceholder;
        }

        protected void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        // Card hover animation

        // Animates a named child's opacity. Logs a warning if the child can't be found,
        // so a renamed DataTemplate element surfaces during development instead of
        // silently disabling the hover animation.
        protected void AnimateChildOpacity(DependencyObject card, string childName, double toOpacity)
        {
            var child = FindVisualChild<FrameworkElement>(card, childName);
            if (child == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[{GetType().Name}] Hover animation target '{childName}' not found — " +
                    "check the DataTemplate element names.");
                return;
            }

            child.BeginAnimation(OpacityProperty, new DoubleAnimation(toOpacity, TimeSpan.FromMilliseconds(200)));
        }

        protected static T FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T element && (string.IsNullOrEmpty(name) || element.Name == name))
                    return element;

                var result = FindVisualChild<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
