// --------------------------------------------------------------------------------
// <copyright file="SplashViewModel.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using AccuSync.Application.Abstractions.Services;
using AccuSync.Application.Resources;

namespace AccuSync.Presentation.ViewModels
{
    /// <summary>
    /// Drives the splash screen. Initializes the database and shows
    /// progress status. Navigation after completion is handled by
    /// <c>SplashWindow.xaml.cs</c> via <c>App.NavigateAfterSplash()</c>.
    /// </summary>
    public class SplashViewModel : INotifyPropertyChanged
    {
        private readonly IDatabaseService _databaseService;
        private string _statusMessage;

        /// <summary>Current progress message shown on the splash screen.</summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Creates the view model with the database service used for initialization.</summary>
        /// <param name="databaseService">Service used to initialize the application database.</param>
        public SplashViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// Initializes the database and updates <see cref="StatusMessage"/> as progress advances.
        /// Shuts down the application if initialization fails.
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                StatusMessage = Strings.SplashViewModel_InitializingDatabase;
                await Task.Delay(500); // Brief pause so the user sees each status message

                await _databaseService.InitializeDatabaseAsync();

                StatusMessage = Strings.SplashViewModel_LoadingApplication;
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                // Show the error for 3 seconds then shut down — there's no recovery
                // from a failed database init.
                StatusMessage = string.Format(Strings.SplashViewModel_Error, ex.Message);
                await Task.Delay(3000);
                System.Windows.Application.Current.Shutdown();
            }
        }

        /// <summary>Raised whenever a bound property's value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}