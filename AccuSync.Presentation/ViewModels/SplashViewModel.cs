// --------------------------------------------------------------------------------
// <copyright file="SplashViewModel.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Presentation.Abstractions;
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
        private readonly IDatabaseInitializer _databaseInitializer;
        private readonly IApplicationLifecycle _applicationLifecycle;
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

        /// <summary>Creates the view model with the initializer used to bring the database up to date.</summary>
        /// <param name="databaseInitializer">Service used to initialize the application database.</param>
        /// <param name="applicationLifecycle">Used to shut the application down if initialization fails.</param>
        public SplashViewModel(IDatabaseInitializer databaseInitializer, IApplicationLifecycle applicationLifecycle)
        {
            _databaseInitializer = databaseInitializer;
            _applicationLifecycle = applicationLifecycle;
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

                await _databaseInitializer.InitializeAsync();

                StatusMessage = Strings.SplashViewModel_LoadingApplication;
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                // Show the error for 3 seconds then shut down — there's no recovery
                // from a failed database init.
                StatusMessage = string.Format(Strings.SplashViewModel_Error, ex.Message);
                await Task.Delay(3000);
                _applicationLifecycle.Shutdown();
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
