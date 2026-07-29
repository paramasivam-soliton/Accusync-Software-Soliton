// --------------------------------------------------------------------------------
// <copyright file="UsersContentModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace AccuSync.Models
{
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

        public string LoginName
        {
            get => _loginName;
            set { _loginName = value; OnPropertyChanged(nameof(LoginName)); OnPropertyChanged(nameof(DisplayName)); }
        }
        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(nameof(FirstName)); OnPropertyChanged(nameof(DisplayName)); }
        }
        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(nameof(LastName)); OnPropertyChanged(nameof(DisplayName)); }
        }
        public string Profile
        {
            get => _profile;
            set { _profile = value; OnPropertyChanged(nameof(Profile)); }
        }
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }
        public bool IsLocked
        {
            get => _isLocked;
            set { _isLocked = value; OnPropertyChanged(nameof(IsLocked)); OnPropertyChanged(nameof(LockedDisplay)); }
        }
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }
        public string Verify
        {
            get => _verify;
            set { _verify = value; OnPropertyChanged(nameof(Verify)); }
        }
        public int LanguageIndex
        {
            get => _languageIndex;
            set { _languageIndex = value; OnPropertyChanged(nameof(LanguageIndex)); }
        }

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
        public string LockedDisplay => IsLocked ? "Locked" : "\u2014";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
