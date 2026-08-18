// --------------------------------------------------------------------------------
// <copyright file="AppDialog.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AccuSync.WPF.Resources;

namespace AccuSync.WPF.Controls
{
    /// <summary>
    /// A styled, application-themed replacement for <see cref="MessageBox"/>. The static
    /// <c>Show</c> overloads mirror <see cref="MessageBox.Show(string)"/> so call sites can
    /// swap <c>MessageBox.Show</c> for <c>AppDialog.Show</c> with no other changes; the return
    /// value is a <see cref="MessageBoxResult"/> just like the framework dialog.
    /// </summary>
    public partial class AppDialog : Window
    {
        // Brand palette (matches the rest of the app).
        private static readonly Brush BlueAccent = Brush("#428FEC");
        private static readonly Brush BlueTint = Brush("#E6F1FB");
        private static readonly Brush BlueTitle = Brush("#0C447C");
        private static readonly Brush RedAccent = Brush("#E24B4A");
        private static readonly Brush RedTint = Brush("#FCEBEB");
        private static readonly Brush RedTitle = Brush("#7F1D1D");

        // Material-style 24x24 glyphs.
        private const string IconInfo = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z";
        private const string IconWarning = "M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z";
        private const string IconQuestion = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 17h-2v-2h2v2zm2.07-7.75l-.9.92C13.45 12.9 13 13.5 13 15h-2v-.5c0-1.1.45-2.1 1.17-2.83l1.24-1.26c.37-.36.59-.86.59-1.41 0-1.1-.9-2-2-2s-2 .9-2 2H8c0-2.21 1.79-4 4-4s4 1.79 4 4c0 .88-.36 1.68-.93 2.25z";
        private const string IconError = "M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2 12 2zm5 13.59L15.59 17 12 13.41 8.41 17 7 15.59 10.59 12 7 8.41 8.41 7 12 10.59 15.59 7 17 8.41 13.41 12 17 15.59z";

        private Brush _accent = BlueAccent;

        /// <summary>The button the user chose. Defaults to <see cref="MessageBoxResult.None"/>.</summary>
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        private AppDialog()
        {
            InitializeComponent();
        }

        // ── Public API (mirrors MessageBox.Show overloads used in the app) ──────────────

        /// <summary>Shows an OK-only dialog with no caption or icon.</summary>
        /// <param name="messageBoxText">The message body to display.</param>
        /// <returns>The button the user chose.</returns>
        public static MessageBoxResult Show(string messageBoxText)
            => Show(messageBoxText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None);

        /// <summary>Shows an OK-only dialog with the given caption.</summary>
        /// <param name="messageBoxText">The message body to display.</param>
        /// <param name="caption">The dialog title.</param>
        /// <returns>The button the user chose.</returns>
        public static MessageBoxResult Show(string messageBoxText, string caption)
            => Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);

        /// <summary>Shows a dialog with the given caption and button set.</summary>
        /// <param name="messageBoxText">The message body to display.</param>
        /// <param name="caption">The dialog title.</param>
        /// <param name="button">Which buttons to show.</param>
        /// <returns>The button the user chose.</returns>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
            => Show(messageBoxText, caption, button, MessageBoxImage.None);

        /// <summary>Shows a dialog with the given caption, button set, and icon kind.</summary>
        /// <param name="messageBoxText">The message body to display.</param>
        /// <param name="caption">The dialog title.</param>
        /// <param name="button">Which buttons to show.</param>
        /// <param name="icon">Which icon and accent color to use.</param>
        /// <returns>The button the user chose.</returns>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            var dlg = new AppDialog();
            dlg.Build(messageBoxText, caption, button, icon);

            var owner = System.Windows.Application.Current?.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.IsActive && w != dlg)
                ?? System.Windows.Application.Current?.MainWindow;

            if (owner != null && owner != dlg && owner.IsLoaded)
            {
                dlg.Owner = owner;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            dlg.ShowDialog();
            return dlg.Result;
        }

        // ── Build ───────────────────────────────────────────────────────────────────────

        private void Build(string message, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            MessageText.Text = message ?? string.Empty;
            TitleText.Text = string.IsNullOrEmpty(caption) ? "AccuSync" : caption;

            ApplyKind(icon);
            BuildButtons(button);
        }

        private void ApplyKind(MessageBoxImage icon)
        {
            string glyph;
            Brush tint, title;

            switch (icon)
            {
                case MessageBoxImage.Warning: // == Exclamation
                    _accent = RedAccent; tint = RedTint; title = RedTitle; glyph = IconWarning;
                    break;
                case MessageBoxImage.Error:   // == Hand, Stop
                    _accent = RedAccent; tint = RedTint; title = RedTitle; glyph = IconError;
                    break;
                case MessageBoxImage.Question:
                    _accent = BlueAccent; tint = BlueTint; title = BlueTitle; glyph = IconQuestion;
                    break;
                default:                      // Information / Asterisk / None
                    _accent = BlueAccent; tint = BlueTint; title = BlueTitle; glyph = IconInfo;
                    break;
            }

            HeaderBorder.Background = tint;
            IconCircle.Background = _accent;
            IconPath.Data = Geometry.Parse(glyph);
            TitleText.Foreground = title;
        }

        private void BuildButtons(MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OK:
                    AddButton(Strings.AppDialog_OK, primary: true, isDefault: true, isCancel: true, MessageBoxResult.OK);
                    break;
                case MessageBoxButton.OKCancel:
                    AddButton(Strings.AppDialog_Cancel, primary: false, isDefault: false, isCancel: true, MessageBoxResult.Cancel);
                    AddButton(Strings.AppDialog_OK, primary: true, isDefault: true, isCancel: false, MessageBoxResult.OK);
                    break;
                case MessageBoxButton.YesNoCancel:
                    AddButton(Strings.AppDialog_Cancel, primary: false, isDefault: false, isCancel: true, MessageBoxResult.Cancel);
                    AddButton(Strings.AppDialog_No, primary: false, isDefault: false, isCancel: false, MessageBoxResult.No);
                    AddButton(Strings.AppDialog_Yes, primary: true, isDefault: true, isCancel: false, MessageBoxResult.Yes);
                    break;
                case MessageBoxButton.YesNo:
                default:
                    AddButton(Strings.AppDialog_No, primary: false, isDefault: false, isCancel: true, MessageBoxResult.No);
                    AddButton(Strings.AppDialog_Yes, primary: true, isDefault: true, isCancel: false, MessageBoxResult.Yes);
                    break;
            }
        }

        private void AddButton(string text, bool primary, bool isDefault, bool isCancel, MessageBoxResult result)
        {
            var btn = new Button
            {
                Content = text,
                Style = (Style)Resources[primary ? "PrimaryPill" : "OutlinePill"],
                IsDefault = isDefault,
                IsCancel = isCancel,
            };
            if (primary) btn.Background = _accent;
            if (ButtonPanel.Children.Count > 0) btn.Margin = new Thickness(10, 0, 0, 0);
            btn.Click += (s, e) => { Result = result; DialogResult = true; };
            ButtonPanel.Children.Add(btn);
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private static Brush Brush(string hex)
        {
            var b = (Brush)new BrushConverter().ConvertFromString(hex);
            b.Freeze();
            return b;
        }
    }
}
