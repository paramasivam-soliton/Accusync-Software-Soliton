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
using AccuSync.Core.Abstractions.Repositories;
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
        private readonly IUserRepository _userRepository;
        private string _statusMessage;

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public SplashViewModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task InitializeAsync()
        {
            try
            {
                StatusMessage = Strings.SplashViewModel_InitializingDatabase;
                await Task.Delay(500); // Brief pause so the user sees each status message

                await _userRepository.InitializeDatabaseAsync();

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}