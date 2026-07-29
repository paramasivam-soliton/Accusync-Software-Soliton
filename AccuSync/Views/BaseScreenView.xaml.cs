// --------------------------------------------------------------------------------
// <copyright file="BaseScreenView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using AccuSync.Controls;
using AccuSync.Models;

namespace AccuSync.Views
{
    public partial class BaseScreenView : UserControl
    {
        /// <summary>
        /// Raised when any ribbon button is clicked. Parent can subscribe to handle actions.
        /// </summary>
        public event EventHandler<RibbonItemClickEventArgs> RibbonItemClicked;

        public BaseScreenView()
        {
            InitializeComponent();
            Ribbon.ItemClicked += OnRibbonItemClicked;
        }

        // Public Configuration API

        /// <summary>
        /// Loads the ribbon toolbar from a RibbonDefinition.
        /// </summary>
        public void SetRibbonDefinition(RibbonDefinition definition)
        {
            Ribbon.LoadDefinition(definition);
        }

        /// <summary>
        /// Sets the content area to a specific UIElement (e.g., a DataGrid, form, etc.).
        /// </summary>
        public void SetContent(UIElement content)
        {
            ScreenContent.Content = content;
        }

        /// <summary>
        /// Provides direct access to the ribbon control for advanced scenarios.
        /// </summary>
        public RibbonToolbar RibbonToolbar => Ribbon;

        // Internal

        private void OnRibbonItemClicked(object sender, RibbonItemClickEventArgs e)
        {
            RibbonItemClicked?.Invoke(this, e);
        }
    }
}