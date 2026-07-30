// --------------------------------------------------------------------------------
// <copyright file="FacesheetReview.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Models;
using AccuSync.WPF.Resources;
using AccuSync.Application.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Docnet.Core;
using Docnet.Core.Models;

namespace AccuSync.WPF.Views.PatientsTests
{
    /// <summary>
    /// Facesheet review step shown after a PDF, DOCX, or image import.
    /// Displays the original document alongside editable patient fields so
    /// the user can verify and correct extracted data before importing.
    /// </summary>
    public partial class FacesheetReview : UserControl
    {
        // Raised to parent (ImportWorkspace or PatientsView)
        public event EventHandler BackRequested;
        public event EventHandler CancelRequested;
        public event EventHandler<PatientData> ImportRequested;

        private PatientData _patient;
        private string _format;
        private string _filePath;

        // Tracks the selected tri-state value per risk key (the row's Tag). This is the
        // source of truth for CollectRiskFactors — selection is no longer inferred from
        // button colors, which broke silently if the active style changed.
        private readonly Dictionary<string, string> _riskSelections = new();

        // Risk button colors — static to avoid repeated allocations
        private static readonly SolidColorBrush _yesActiveBg = new(Color.FromRgb(0x10, 0xB9, 0x81));
        private static readonly SolidColorBrush _noActiveBg = new(Color.FromRgb(0xEF, 0x44, 0x44));
        private static readonly SolidColorBrush _unknownActiveBg = new(Color.FromRgb(0x6B, 0x72, 0x80));
        private static readonly SolidColorBrush _defaultBg = Brushes.White;
        private static readonly SolidColorBrush _defaultFg = new(Color.FromRgb(0x6B, 0x72, 0x80));
        private static readonly SolidColorBrush _defaultBorder = new(Color.FromRgb(0xD1, 0xD5, 0xDB));

        public FacesheetReview()
        {
            InitializeComponent();
        }

        // Public API

        /// <summary>
        /// Loads parsed patient data into the review form and sets up the
        /// document preview. Called by the parent after
        /// <see cref="ImportService.ParseFile"/> returns a facesheet result.
        /// </summary>
        public void LoadReview(PatientData patient, string filePath, string format)
        {
            _patient = patient ?? new PatientData { RiskFactors = new Dictionary<string, string>() };
            _filePath = filePath;
            _format = format;

            PopulatePatientFields();
            PopulateRiskFactors();
            PopulateAdditionalFields();
            SetupDocumentPreview();
            SetupConfidenceBanner();

            ReviewTabs.SelectedIndex = 0;
        }

        /// <summary>
        /// Collects all current form values back into a <see cref="PatientData"/>.
        /// Called when the user clicks Import Patient.
        /// </summary>
        public PatientData CollectPatientData()
        {
            var p = _patient ?? new PatientData();

            p.PatientId = TxtPatientId.Text.Trim();
            p.HospitalId = TxtHospitalId.Text.Trim();
            p.FirstName = TxtFirstName.Text.Trim();
            p.LastName = TxtLastName.Text.Trim();
            p.DateOfBirth = TxtDOB.Text.Trim();
            p.Gender = ComboItemText(CmbGender.SelectedItem);
            p.Weight = TxtWeight.Text.Trim();
            p.Height = TxtHeight.Text.Trim();
            p.GestationalAge = ComboItemText(CmbGestationalAge.SelectedItem);
            p.BirthLocation = TxtBirthLocation.Text.Trim();

            p.MotherFirstName = TxtMotherFirstName.Text.Trim();
            p.MotherLastName = TxtMotherLastName.Text.Trim();
            p.MotherDOB = TxtMotherDOB.Text.Trim();
            p.MotherPhone = TxtMotherPhone.Text.Trim();
            p.Physician = TxtPhysician.Text.Trim();
            p.Medication = TxtMedication.Text.Trim();

            p.RiskFactors = CollectRiskFactors();

            return p;
        }

        // Populate form from PatientData

        private void PopulatePatientFields()
        {
            TxtPatientId.Text = _patient.PatientId ?? "";
            TxtHospitalId.Text = _patient.HospitalId ?? "";
            TxtFirstName.Text = _patient.FirstName ?? "";
            TxtLastName.Text = _patient.LastName ?? "";
            TxtDOB.Text = _patient.DateOfBirth ?? "";
            TxtWeight.Text = _patient.Weight ?? "";
            TxtHeight.Text = _patient.Height ?? "";
            TxtBirthLocation.Text = _patient.BirthLocation ?? "";

            SelectComboBoxItem(CmbGender, _patient.Gender);
            SelectComboBoxItem(CmbGestationalAge, _patient.GestationalAge);
        }

        private void PopulateAdditionalFields()
        {
            TxtMotherFirstName.Text = _patient.MotherFirstName ?? "";
            TxtMotherLastName.Text = _patient.MotherLastName ?? "";
            TxtMotherDOB.Text = _patient.MotherDOB ?? "";
            TxtMotherPhone.Text = _patient.MotherPhone ?? "";
            TxtPhysician.Text = _patient.Physician ?? "";
            TxtMedication.Text = _patient.Medication ?? "";
        }

        private void PopulateRiskFactors()
        {
            if (_patient.RiskFactors == null)
                _patient.RiskFactors = new Dictionary<string, string>();

            var riskRows = GetRiskRows();
            foreach (var row in riskRows)
            {
                string riskKey = row.Tag?.ToString() ?? "";
                string value = "Unknown";

                if (_patient.RiskFactors.TryGetValue(riskKey, out string v))
                    value = v;

                var buttons = GetRiskButtons(row);
                SetActiveRiskButton(buttons, value);
            }
        }

        // Document preview — shows the original facesheet alongside the form
        // so the user can compare extracted values against the source.

        private void SetupDocumentPreview()
        {
            ImageViewer.Visibility = Visibility.Collapsed;
            TextViewer.Visibility = Visibility.Collapsed;
            EmptyPreview.Visibility = Visibility.Visible;

            if (string.IsNullOrEmpty(_filePath))
                return;

            try
            {
                if (_format == "facesheet-image")
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_filePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    DocumentImage.Source = bitmap;
                    ImageViewer.Visibility = Visibility.Visible;
                    EmptyPreview.Visibility = Visibility.Collapsed;
                }
                else if (_format == "facesheet-pdf")
                {
                    var pdfImage = RenderPdfToImage(_filePath);
                    if (pdfImage != null)
                    {
                        DocumentImage.Source = pdfImage;
                        ImageViewer.Visibility = Visibility.Visible;
                        EmptyPreview.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    // DOCX — no image rendering, show extracted text
                    string previewText = ImportService.GetFacesheetPreviewText(_filePath, _format);
                    if (!string.IsNullOrWhiteSpace(previewText))
                    {
                        DocumentText.Text = previewText;
                        TextViewer.Visibility = Visibility.Visible;
                        EmptyPreview.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FacesheetReview: Preview failed - {ex.Message}");
            }
        }

        /// <summary>
        /// Renders the first page of a PDF as a <see cref="BitmapSource"/> using Docnet.
        /// Facesheets are typically single-page so only page 0 is rendered.
        /// </summary>
        private BitmapSource RenderPdfToImage(string filePath)
        {
            using var docReader = DocLib.Instance.GetDocReader(filePath, new PageDimensions(1080, 1920));
            int pageCount = docReader.GetPageCount();
            if (pageCount == 0) return null;

            using var pageReader = docReader.GetPageReader(0);
            var rawBytes = pageReader.GetImage();
            int width = pageReader.GetPageWidth();
            int height = pageReader.GetPageHeight();

            if (rawBytes == null || rawBytes.Length == 0) return null;

            var bitmap = BitmapSource.Create(
                width, height,
                96, 96,
                PixelFormats.Bgra32,
                null,
                rawBytes,
                width * 4);
            bitmap.Freeze();
            return bitmap;
        }

        // OCR confidence banner — only shown for image facesheets

        private void SetupConfidenceBanner()
        {
            if (_format != "facesheet-image")
            {
                ConfidenceBanner.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var confidences = OcrParser.GetFieldConfidences(_filePath);
                if (confidences.TryGetValue("Overall", out float confidence))
                {
                    int pct = (int)(confidence * 100);
                    ConfidenceText.Text = string.Format(Strings.FacesheetReview_OcrConfidencePercent, pct);
                    ConfidenceBar.Width = Math.Max(0, Math.Min(120, 120.0 * confidence));
                    ConfidenceBanner.Visibility = Visibility.Visible;
                }
                else
                {
                    ConfidenceBanner.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FacesheetReview: Confidence check failed - {ex.Message}");
                ConfidenceBanner.Visibility = Visibility.Collapsed;
            }
        }

        // Risk factor buttons — same tri-state pattern as RiskFactorsTab
        // but independent implementation since this control isn't backed by
        // PatientViewModel (it works directly with PatientData).

        private void RiskButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Parent is not StackPanel panel) return;

            var buttons = panel.Children.OfType<Button>().ToList();
            string value = btn.Tag?.ToString() ?? "Unknown";
            SetActiveRiskButton(buttons, value);
        }

        private void BulkSetYes_Click(object sender, RoutedEventArgs e) => BulkSetAllRisks("Yes");
        private void BulkSetNo_Click(object sender, RoutedEventArgs e) => BulkSetAllRisks("No");
        private void BulkSetUnknown_Click(object sender, RoutedEventArgs e) => BulkSetAllRisks("Unknown");

        private void BulkSetAllRisks(string value)
        {
            var riskRows = GetRiskRows();
            foreach (var row in riskRows)
            {
                var buttons = GetRiskButtons(row);
                SetActiveRiskButton(buttons, value);
            }
        }

        private void SetActiveRiskButton(List<Button> buttons, string value)
        {
            foreach (var b in buttons)
            {
                string tag = b.Tag?.ToString() ?? "";
                bool isActive = tag.Equals(value, StringComparison.OrdinalIgnoreCase);

                if (isActive)
                {
                    b.Background = tag switch
                    {
                        "Yes" => _yesActiveBg,
                        "No" => _noActiveBg,
                        _ => _unknownActiveBg
                    };
                    b.Foreground = Brushes.White;
                    b.BorderBrush = b.Background;
                }
                else
                {
                    b.Background = _defaultBg;
                    b.Foreground = _defaultFg;
                    b.BorderBrush = _defaultBorder;
                }
            }

            // Record the selection explicitly. Every selection path (populate, click,
            // bulk set) funnels through here, so this is the one place that needs it.
            string key = RiskKeyForButtons(buttons);
            if (!string.IsNullOrEmpty(key))
                _riskSelections[key] = value;
        }

        // Resolves the owning risk row's key (the Grid.Tag) from its buttons.
        // The three buttons share one StackPanel inside the row Grid.
        private static string RiskKeyForButtons(List<Button> buttons)
        {
            var row = (buttons.FirstOrDefault()?.Parent as StackPanel)?.Parent as Grid;
            return row?.Tag?.ToString() ?? "";
        }

        // Reads the tracked selections (populated by SetActiveRiskButton) rather than
        // inferring the active value from button colors. Enumerates the rows so the
        // result always covers every risk factor, defaulting any unset row to "Unknown".
        private Dictionary<string, string> CollectRiskFactors()
        {
            var risks = new Dictionary<string, string>();

            foreach (var row in GetRiskRows())
            {
                string key = row.Tag?.ToString() ?? "";
                if (string.IsNullOrEmpty(key)) continue;

                risks[key] = _riskSelections.TryGetValue(key, out var value) ? value : "Unknown";
            }

            return risks;
        }

        // Risk row helpers — navigate the visual tree to find risk factor
        // Grid rows and their Yes/No/Unknown buttons.

        private List<Grid> GetRiskRows()
        {
            var rows = new List<Grid>();

            foreach (var child in RiskFactorsPanel.Children)
            {
                if (child is Border border && border.Child is StackPanel sp)
                {
                    foreach (var item in sp.Children)
                    {
                        if (item is Grid g && g.Tag != null)
                            rows.Add(g);
                    }
                }
            }

            return rows;
        }

        private List<Button> GetRiskButtons(Grid riskRow)
        {
            foreach (var child in riskRow.Children)
            {
                if (child is StackPanel sp)
                    return sp.Children.OfType<Button>().ToList();
            }
            return new List<Button>();
        }

        // ComboBox helpers
        // Items may be either ComboBoxItem objects (declared inline in XAML) or plain
        // strings (bound via ItemsSource, e.g. the shared gestational age range), so
        // both helpers handle either form.

        private static string ComboItemText(object item) =>
            (item is ComboBoxItem cbi ? cbi.Content?.ToString() : item?.ToString())?.Trim() ?? "";

        private static void SelectComboBoxItem(ComboBox combo, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                // Clear selection. For lists with a leading blank entry this displays
                // empty; for ItemsSource-bound lists with no blank entry it avoids
                // wrongly selecting the first real value.
                combo.SelectedItem = null;
                return;
            }

            string normalized = value.Trim().ToLower();
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (ComboItemText(combo.Items[i]).ToLower() == normalized)
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }

            combo.SelectedItem = null;
        }

        // Footer buttons

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var editedPatient = CollectPatientData();
            ImportRequested?.Invoke(this, editedPatient);
        }
    }
}