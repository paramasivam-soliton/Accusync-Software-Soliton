// --------------------------------------------------------------------------------
// <copyright file="SplashWindow.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Media.Animation;
using AccuSync.ViewModels;

namespace AccuSync.Views.Splash
{
    public partial class SplashWindow : Window
    {
        private readonly SplashViewModel _viewModel;

        public SplashWindow(SplashViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            Loaded += SplashWindow_Loaded;
        }

        private async void SplashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = ActualWidth - 40,
                Duration = System.TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever
            };
            LoadingBar.BeginAnimation(WidthProperty, animation);

            await _viewModel.InitializeAsync();

            App.NavigateAfterSplash(this);
        }
    }
}