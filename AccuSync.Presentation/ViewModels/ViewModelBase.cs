// --------------------------------------------------------------------------------
// <copyright file="ViewModelBase.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AccuSync.Presentation.ViewModels
{
    /// <summary>
    /// Shared <see cref="INotifyPropertyChanged"/> plumbing for every ViewModel in this project,
    /// so each screen's ViewModel does not re-implement the same event and change-notification
    /// helper. A derived class may override <see cref="SetProperty{T}"/> to layer extra behavior
    /// (e.g. dirty-state tracking) on top of the base equality-check-and-notify logic.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>Raised whenever a bound property's value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Raises <see cref="PropertyChanged"/> for the given property.</summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Assigns <paramref name="value"/> to <paramref name="storage"/> and raises
        /// <see cref="PropertyChanged"/> only when the value actually differs.
        /// </summary>
        /// <returns><see langword="true"/> if the value changed; otherwise <see langword="false"/>.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
