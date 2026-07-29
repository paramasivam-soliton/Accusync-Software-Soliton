// --------------------------------------------------------------------------------
// <copyright file="PrintDialog.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace AccuSync.Views.PatientsTests.Dialogs
{
    public enum PrintReportType
    {
        BestTests,
        BestTEOAE,
        BestDPOAE,
        BestABR,
        SelectedTests
    }

    public enum PrintReportCategory
    {
        Basic,
        Detail
    }

    /// <summary>
    /// Code-behind for the print dialog. The user selects one of 10 report
    /// cards (5 report types × 2 categories: Basic and Detail). The selected
    /// card's <c>Tag</c> encodes both values as <c>"Category|ReportType"</c>.
    /// </summary>
    // NOTE: The card selection, hover, and reset logic is nearly identical to
    //       ExportDialog. If a third dialog needs the same pattern, extract a
    //       shared SelectableCardBehavior or base class.
    public partial class PrintDialog : Window
    {
        private Border _selectedCard;
        private string _selectedTag = "Basic|BestTests";

        // Card visual state brushes — shared with ExportDialog's pattern.
        // TODO: These are duplicated from ExportDialog. Extract to a shared
        //       static class or resource dictionary.
        private static readonly SolidColorBrush _lightBlueBrush = new(Color.FromRgb(212, 233, 249));
        private static readonly SolidColorBrush _brightBlueBrush = new(Color.FromRgb(0, 95, 190));
        private static readonly SolidColorBrush _skyBrush = new(Color.FromRgb(66, 143, 236));
        private static readonly SolidColorBrush _defaultBorderBrush = new(Color.FromRgb(229, 231, 235));
        private static readonly SolidColorBrush _hoverBgBrush = new(Color.FromRgb(240, 247, 255));

        // Maps each card Border to its icon Border so SelectCard/ResetCard
        // can update both without a switch statement.
        private Dictionary<Border, Border> _cardIconMap;

        // Read by the caller after dialog closes — parsed from the Tag string
        public PrintReportCategory SelectedCategory
        {
            get
            {
                var parts = _selectedTag.Split('|');
                return parts[0] == "Detail" ? PrintReportCategory.Detail : PrintReportCategory.Basic;
            }
        }

        public PrintReportType SelectedReport
        {
            get
            {
                var parts = _selectedTag.Split('|');
                return parts.Length > 1 ? parts[1] switch
                {
                    "BestTEOAE" => PrintReportType.BestTEOAE,
                    "BestDPOAE" => PrintReportType.BestDPOAE,
                    "BestABR" => PrintReportType.BestABR,
                    "SelectedTests" => PrintReportType.SelectedTests,
                    _ => PrintReportType.BestTests
                } : PrintReportType.BestTests;
            }
        }

        public PrintDialog()
        {
            InitializeComponent();
            BuildCardIconMap();
            SelectCard(BasicBestTests);
        }

        private void BuildCardIconMap()
        {
            _cardIconMap = new Dictionary<Border, Border>
            {
                { BasicBestTests, BasicBestTestsIcon },
                { BasicBestTEOAE, BasicBestTEOAEIcon },
                { BasicBestDPOAE, BasicBestDPOAEIcon },
                { BasicBestABR, BasicBestABRIcon },
                { BasicSelectedTests, BasicSelectedTestsIcon },
                { DetailBestTests, DetailBestTestsIcon },
                { DetailBestTEOAE, DetailBestTEOAEIcon },
                { DetailBestDPOAE, DetailBestDPOAEIcon },
                { DetailBestABR, DetailBestABRIcon },
                { DetailSelectedTests, DetailSelectedTestsIcon },
            };
        }

        // Window chrome

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) DialogResult = false;
        }

        // Footer buttons

        private void CloseButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
        private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
        private void PrintButton_Click(object sender, RoutedEventArgs e) => DialogResult = true;

        // Card selection — single selection across all 10 cards (both columns)

        private void ReportCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border card) SelectCard(card);
        }

        private void SelectCard(Border card)
        {
            foreach (var kvp in _cardIconMap)
                ResetCard(kvp.Key, kvp.Value);

            _selectedCard = card;
            _selectedTag = card.Tag?.ToString() ?? "Basic|BestTests";
            var iconBorder = _cardIconMap[card];

            card.BorderBrush = _brightBlueBrush;
            card.Background = _lightBlueBrush;
            card.Effect = new DropShadowEffect
            {
                BlurRadius = 8,
                Opacity = 0.12,
                ShadowDepth = 0,
                Color = Color.FromRgb(71, 48, 137)
            };

            iconBorder.Background = Brushes.White;
        }

        private void ResetCard(Border card, Border iconBorder)
        {
            card.BorderBrush = _defaultBorderBrush;
            card.Background = Brushes.White;
            card.Effect = null;
            iconBorder.Background = _lightBlueBrush;

            if (iconBorder.Child is System.Windows.Shapes.Path iconPath)
                iconPath.Fill = _brightBlueBrush;
        }

        // Hover effects — skipped for the currently selected card

        private void ReportCard_MouseEnter(object sender, MouseEventArgs e)
        {
            var card = sender as Border;
            if (card == null || card == _selectedCard) return;
            card.BorderBrush = _skyBrush;
            card.Background = _hoverBgBrush;
        }

        private void ReportCard_MouseLeave(object sender, MouseEventArgs e)
        {
            var card = sender as Border;
            if (card == null || card == _selectedCard) return;
            card.BorderBrush = _defaultBorderBrush;
            card.Background = Brushes.White;
        }
    }
}