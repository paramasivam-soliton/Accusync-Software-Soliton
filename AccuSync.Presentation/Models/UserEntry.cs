// --------------------------------------------------------------------------------
// <copyright file="UserEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace AccuSync.Presentation.Models
{
    /// <summary>
    /// A configurable user account entry shown in the user management list.
    /// </summary>
    public class UserEntry : INotifyPropertyChanged
    {
        private string _loginName = "";
        private string _firstName = "";
        private string _lastName = "";
        private string _profile = "Screener";
        private string _status = "Active";
        private bool _isLocked;
        private string _password = "";
        private string _verify = "";
        private int _languageIndex;

        /// <summary>The user's login name.</summary>
        public string LoginName
        {
            get => _loginName;
            set { _loginName = value; OnPropertyChanged(nameof(LoginName)); OnPropertyChanged(nameof(DisplayName)); }
        }

        /// <summary>The user's first name.</summary>
        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(nameof(FirstName)); OnPropertyChanged(nameof(DisplayName)); }
        }

        /// <summary>The user's last name.</summary>
        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(nameof(LastName)); OnPropertyChanged(nameof(DisplayName)); }
        }

        /// <summary>The profile assigned to the user.</summary>
        public string Profile
        {
            get => _profile;
            set { _profile = value; OnPropertyChanged(nameof(Profile)); }
        }

        /// <summary>The user's account status.</summary>
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        /// <summary>Whether the user's account is locked.</summary>
        public bool IsLocked
        {
            get => _isLocked;
            set { _isLocked = value; OnPropertyChanged(nameof(IsLocked)); OnPropertyChanged(nameof(LockedDisplay)); }
        }

        /// <summary>The user's password.</summary>
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        /// <summary>Confirmation entry for <see cref="Password"/>.</summary>
        public string Verify
        {
            get => _verify;
            set { _verify = value; OnPropertyChanged(nameof(Verify)); }
        }

        /// <summary>Selected index into the language ComboBox.</summary>
        public int LanguageIndex
        {
            get => _languageIndex;
            set { _languageIndex = value; OnPropertyChanged(nameof(LanguageIndex)); }
        }

        /// <summary>The user's display name, formatted as "Last, First" when both are known.</summary>
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(LastName) && !string.IsNullOrEmpty(FirstName))
                    return $"{LastName}, {FirstName}";
                if (!string.IsNullOrEmpty(LastName))
                    return LastName;
                if (!string.IsNullOrEmpty(FirstName))
                    return FirstName;
                return "";
            }
        }

        /// <summary>Display text for whether the account is locked.</summary>
        public string LockedDisplay => IsLocked ? "Locked" : "\u2014";

        /// <summary>Raised when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Raises <see cref="PropertyChanged"/> for the given property name.</summary>
        /// <param name="name">The name of the property that changed.</param>
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>Creates a copy of this entry with the same property values.</summary>
        /// <returns>A new <see cref="UserEntry"/> with the same values.</returns>
        public UserEntry Clone() => new UserEntry
        {
            LoginName = LoginName,
            FirstName = FirstName,
            LastName = LastName,
            Profile = Profile,
            Status = Status,
            IsLocked = IsLocked,
            Password = Password,
            Verify = Verify,
            LanguageIndex = LanguageIndex
        };
    }
}
