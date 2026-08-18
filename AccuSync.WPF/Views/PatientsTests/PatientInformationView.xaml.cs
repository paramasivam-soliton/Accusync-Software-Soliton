// --------------------------------------------------------------------------------
// <copyright file="PatientInformationView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Abstractions.Parsing;
using AccuSync.Presentation.ViewModels;
using AccuSync.WPF.Resources;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.PatientsTests
{
    public partial class PatientInformationView : UserControl
    {
        private int currentTabIndex = 0;
        private Popup _requiredFieldsPopup;
        private bool _isEditMode = false;
        private bool _isAddMode = false;

        // The ViewModel we currently have a PropertyChanged subscription on, so we can
        // detach before subscribing to a new one (see SubscribeToViewModel).
        private PatientViewModel _subscribedViewModel;

        public PatientInformationView()
        {
            InitializeComponent();
            InitializeRequiredFieldsPopup();
        }

        // Required Fields Popup

        private void InitializeRequiredFieldsPopup()
        {
            _requiredFieldsPopup = new Popup
            {
                AllowsTransparency = true,
                StaysOpen = false,
                Placement = PlacementMode.Bottom,
                PlacementTarget = WarningBadge
            };

            var border = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 4, 0, 0),
                MaxWidth = 300
            };

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock
            {
                Text = Strings.PatientInformationView_MissingRequiredFields,
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 48, 73)),
                Margin = new Thickness(0, 0, 0, 8)
            });
            stackPanel.Children.Add(new ListBox
            {
                Name = "MissingFieldsListBox",
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush(Colors.Transparent)
            });

            border.Child = stackPanel;
            _requiredFieldsPopup.Child = border;
        }

        // Load Patient

        public void LoadPatient(PatientViewModel viewModel)
        {
            if (_isAddMode)
            {
                _isAddMode = false;
                NewPatientBanner.Visibility = Visibility.Collapsed;
                PatientPillGroup.Visibility = Visibility.Visible;
                PatientSummary.Visibility = Visibility.Visible;
            }

            if (viewModel == null)
            {
                SubscribeToViewModel(null);
                ShowEmptyState();
                return;
            }

            ShowContent();
            DataContext = viewModel;

            viewModel.BeginEdit();
            viewModel.GenerateQRCode();

            AdditionalInfoTab.InitializeDialCodes();
            RiskFactorsTab.InitializeRiskButtons();

            UpdateWarningBadge(viewModel.HasRequiredFieldsEmpty, viewModel.RequiredFieldsCount);

            SubscribeToViewModel(viewModel);

            SwitchToTab(0);
            _isEditMode = false;
            UpdateEditModeState();
            UpdateActionPillButtons(false);
        }

        // Attaches the PropertyChanged handler to the given ViewModel, first detaching
        // from any previously-loaded one. Without this, every LoadPatient/NewPatient
        // call leaked a fresh anonymous handler bound to a stale ViewModel.
        private void SubscribeToViewModel(PatientViewModel viewModel)
        {
            if (_subscribedViewModel != null)
                _subscribedViewModel.PropertyChanged -= ViewModel_PropertyChanged;

            _subscribedViewModel = viewModel;

            if (viewModel != null)
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is PatientViewModel viewModel)) return;

            if (e.PropertyName == nameof(PatientViewModel.HasRequiredFieldsEmpty) ||
                e.PropertyName == nameof(PatientViewModel.RequiredFieldsCount))
                UpdateWarningBadge(viewModel.HasRequiredFieldsEmpty, viewModel.RequiredFieldsCount);

            if (e.PropertyName == nameof(PatientViewModel.IsDirty) ||
                e.PropertyName == nameof(PatientViewModel.CanUndo))
                UpdateActionPillButtons(viewModel.IsDirty);

            if (e.PropertyName == nameof(PatientViewModel.PerinatalYesCount) ||
                e.PropertyName == nameof(PatientViewModel.PostnatalYesCount) ||
                e.PropertyName == nameof(PatientViewModel.OtherYesCount))
                RiskFactorsTab.UpdateRiskSummary();
        }

        // Add / Cancel New Patient

        public void NewPatient()
        {
            _isAddMode = true;
            _isEditMode = true;
            ShowContent();

            var viewModel = new PatientViewModel(App.GetService<IQrCodeGenerator>());
            viewModel.BeginEdit();
            DataContext = viewModel;

            AdditionalInfoTab.InitializeDialCodes();
            RiskFactorsTab.InitializeRiskButtons();

            NewPatientBanner.Visibility = Visibility.Visible;
            PatientPillGroup.Visibility = Visibility.Collapsed;
            PatientSummary.Visibility = Visibility.Collapsed;
            EditOverlay.Visibility = Visibility.Collapsed;

            SubscribeToViewModel(viewModel);

            SwitchToTab(0);
            UpdateWarningBadge(viewModel.HasRequiredFieldsEmpty, viewModel.RequiredFieldsCount);
            UpdateActionPillButtons(false);
        }

        public void CancelNewPatient()
        {
            _isAddMode = false;
            _isEditMode = false;
            NewPatientBanner.Visibility = Visibility.Collapsed;
            PatientPillGroup.Visibility = Visibility.Visible;
            PatientSummary.Visibility = Visibility.Visible;
            SubscribeToViewModel(null);
            DataContext = null;
            ShowEmptyState();
        }

        private void CancelNewPatient_Click(object sender, RoutedEventArgs e) => CancelNewPatient();

        private void ShowEmptyState()
        {
            EmptyState.Visibility = Visibility.Visible;
            ContentContainer.Visibility = Visibility.Collapsed;
        }

        private void ShowContent()
        {
            EmptyState.Visibility = Visibility.Collapsed;
            ContentContainer.Visibility = Visibility.Visible;
        }

        // Warning Badge

        private void UpdateWarningBadge(bool hasEmptyFields, int count)
        {
            if (hasEmptyFields && count > 0)
            {
                WarningBadge.Visibility = Visibility.Visible;
                WarningBadge.Content = count.ToString();
            }
            else
            {
                WarningBadge.Visibility = Visibility.Collapsed;
            }
        }

        private void WarningBadge_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as PatientViewModel;
            if (viewModel == null) return;

            var missingFields = viewModel.GetMissingRequiredFields();
            if (missingFields.Count == 0) return;

            if (_requiredFieldsPopup == null) InitializeRequiredFieldsPopup();

            // Toggle popup if already open
            if (_requiredFieldsPopup.IsOpen) { _requiredFieldsPopup.IsOpen = false; return; }

            var border = _requiredFieldsPopup.Child as Border;
            var stackPanel = border?.Child as StackPanel;
            var listBox = stackPanel?.Children[1] as ListBox;
            if (listBox == null) return;

            listBox.Items.Clear();
            foreach (var field in missingFields)
            {
                listBox.Items.Add(new ListBoxItem
                {
                    Content = string.Format(Strings.PatientInformationView_BulletField, field),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(139, 155, 168)),
                    Padding = new Thickness(4, 2, 4, 2)
                });
            }

            _requiredFieldsPopup.PlacementTarget = WarningBadge;
            _requiredFieldsPopup.Placement = PlacementMode.Bottom;
            _requiredFieldsPopup.IsOpen = true;
        }

        // Action Pill State

        private void UpdateActionPillButtons(bool isDirty)
        {
            SavePillButton.IsEnabled = isDirty;
            RevertPillButton.IsEnabled = isDirty;

            // Undo tracks the change stack rather than overall dirty state: it stays
            // enabled as long as there is a single edit left to roll back.
            UndoPillButton.IsEnabled = (DataContext as PatientViewModel)?.CanUndo ?? false;
        }

        // Tab Navigation

        private void PatientTab_Click(object sender, RoutedEventArgs e) => SwitchToTab(0);
        private void RiskTab_Click(object sender, RoutedEventArgs e) => SwitchToTab(1);
        private void AdditionalTab_Click(object sender, RoutedEventArgs e) => SwitchToTab(2);

        private void SwitchToTab(int tabIndex)
        {
            currentTabIndex = tabIndex;

            UpdateTabStyle(PatientTab, tabIndex == 0);
            UpdateTabStyle(RiskTab, tabIndex == 1);
            UpdateTabStyle(AdditionalTab, tabIndex == 2);

            PatientDetailsTab.Visibility = tabIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
            RiskFactorsTab.Visibility = tabIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
            AdditionalInfoTab.Visibility = tabIndex == 2 ? Visibility.Visible : Visibility.Collapsed;

            JumpToSection.Visibility = tabIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
            ScrollToTop();
        }

        private void UpdateTabStyle(Button tab, bool isActive)
        {
            tab.Style = (Style)FindResource(isActive ? "ActiveTabStyle" : "TabButtonStyle");
        }

        // Jump-To Navigation
        // Reaches into AdditionalInfoTab's named sections for scroll targeting

        private void JumpToMother_Click(object sender, RoutedEventArgs e)
            => ScrollToElement(AdditionalInfoTab.MotherSection);

        private void JumpToCaregiver_Click(object sender, RoutedEventArgs e)
            => ScrollToElement(AdditionalInfoTab.CaregiverSection);

        private void JumpToReferral_Click(object sender, RoutedEventArgs e)
            => ScrollToElement(AdditionalInfoTab.ReferralSection);

        private void JumpToMedical_Click(object sender, RoutedEventArgs e)
            => ScrollToElement(AdditionalInfoTab.MedicalSection);

        private void ScrollToElement(FrameworkElement element)
        {
            if (element == null) return;
            var scrollViewer = FindScrollViewer(TabContentContainer);
            if (scrollViewer == null) return;
            var transform = element.TransformToAncestor(scrollViewer);
            var position = transform.Transform(new Point(0, 0));
            AnimateScroll(scrollViewer, position.Y, 500);
        }

        // Cubic ease-in-out scroll animation
        private void AnimateScroll(ScrollViewer scrollViewer, double toValue, int duration)
        {
            var fromValue = scrollViewer.VerticalOffset;
            var startTime = DateTime.Now;
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };

            timer.Tick += (s, args) =>
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                var progress = Math.Min(elapsed / duration, 1.0);
                var easedProgress = progress < 0.5
                    ? 4 * progress * progress * progress
                    : 1 - Math.Pow(-2 * progress + 2, 3) / 2;
                scrollViewer.ScrollToVerticalOffset(fromValue + (toValue - fromValue) * easedProgress);
                if (progress >= 1.0) timer.Stop();
            };
            timer.Start();
        }

        private void ScrollToTop()
        {
            var scrollViewer = FindScrollViewer(TabContentContainer);
            scrollViewer?.ScrollToTop();
        }

        // Walks up the visual tree to find the nearest ScrollViewer ancestor
        private ScrollViewer FindScrollViewer(DependencyObject element)
        {
            if (element == null) return null;
            if (element is ScrollViewer sv) return sv;
            return FindScrollViewer(VisualTreeHelper.GetParent(element));
        }

        // Pill Button Handlers

        private void EditPillButton_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = !_isEditMode;
            UpdateEditModeState();
        }

        private void UpdateEditModeState()
        {
            if (_isEditMode)
            {
                EditOverlay.Visibility = Visibility.Collapsed;
                EditPillButton.Style = (Style)FindResource("PillButtonActiveStyle");
                EditPillButton.ToolTip = Strings.PatientInformationView_CurrentlyEditing;
                DeletePillButton.IsEnabled = false;
            }
            else
            {
                EditOverlay.Visibility = Visibility.Visible;
                EditPillButton.Style = (Style)FindResource("PillButtonStyle");
                EditPillButton.ToolTip = Strings.PatientInformationView_EditPatientTooltip;
                DeletePillButton.IsEnabled = true;
            }
        }

        private void DeletePillButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PatientViewModel;
            if (vm == null) return;

            var result = AppDialog.Show(
                string.Format(Strings.PatientInformationView_DeleteConfirm, vm.FirstName, vm.LastName),
                Strings.PatientInformationView_DeletePatientCaption, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // NOTE: No database delete — only clears the UI. Persistence TBD.
                _isEditMode = false;
                ShowEmptyState();
            }
        }

        private void SavePillButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PatientViewModel;
            if (vm == null) return;

            if (vm.HasRequiredFieldsEmpty)
            {
                var missingFields = vm.GetMissingRequiredFields();
                AppDialog.Show(
                    string.Format(
                        Strings.PatientInformationView_RequiredFieldsMissingMessage,
                        string.Join("\n", missingFields.Select(f => string.Format(Strings.PatientInformationView_BulletField, f)))),
                    Strings.PatientInformationView_RequiredFieldsMissingCaption, MessageBoxButton.OK, MessageBoxImage.Warning);

                PatientDetailsTab.ForceRequiredFieldValidation(vm);
                return;
            }

            // NOTE: AcceptChanges updates the ViewModel snapshot but doesn't persist to database.
            vm.AcceptChanges();

            if (_isAddMode)
            {
                _isAddMode = false;
                _isEditMode = false;
                NewPatientBanner.Visibility = Visibility.Collapsed;
                PatientPillGroup.Visibility = Visibility.Visible;
                PatientSummary.Visibility = Visibility.Visible;
                vm.GenerateQRCode();
                UpdateEditModeState();
                UpdateActionPillButtons(false);
                AppDialog.Show(Strings.PatientInformationView_NewPatientCreated, Strings.PatientInformationView_PatientAddedCaption,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _isEditMode = false;
            UpdateEditModeState();
            AppDialog.Show(Strings.PatientInformationView_PatientSaved, Strings.PatientInformationView_SavedCaption,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RevertPillButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isAddMode) { CancelNewPatient(); return; }

            var vm = DataContext as PatientViewModel;
            if (vm == null) return;

            if (AppDialog.Show(Strings.PatientInformationView_RevertConfirm,
                Strings.PatientInformationView_RevertChangesCaption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                vm.CancelEdit();

                // Force rebind — CancelEdit restores snapshot values but WPF may cache stale bindings
                DataContext = null;
                DataContext = vm;

                RiskFactorsTab.InitializeRiskButtons();
                UpdateActionPillButtons(false);
                _isEditMode = false;
                UpdateEditModeState();
            }
        }

        private void UndoPillButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PatientViewModel;
            if (vm == null || !vm.CanUndo) return;

            // Roll back only the most recent field change. Further edits (and further
            // undo steps) remain available.
            vm.Undo();

            // Force rebind — Undo restores a value on the ViewModel but WPF may cache
            // stale bindings (same pattern as Revert).
            DataContext = null;
            DataContext = vm;

            RiskFactorsTab.InitializeRiskButtons();
            UpdateActionPillButtons(vm.IsDirty);
        }
    }
}