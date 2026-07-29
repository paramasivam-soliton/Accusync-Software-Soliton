// --------------------------------------------------------------------------------
// <copyright file="PatientViewModel.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace AccuSync.Presentation.ViewModels
{
    /// <summary>
    /// Main patient form ViewModel. Provides real-time validation, dirty-state
    /// tracking (BeginEdit/CancelEdit/AcceptChanges), risk factor summary counts,
    /// and phone number formatting with dial codes.
    /// </summary>
    // TODO: BeginEdit and CancelEdit manually list every property (~80 lines each).
    //       Use reflection over a known property list, or implement IEditableObject
    //       with a memento/snapshot pattern to eliminate the maintenance burden.
    //       Every time a property is added, both methods must be updated or the
    //       dirty tracking silently breaks for that field.
    // TODO: The five phone property blocks (MotherPhone, MotherMobile, CaregiverPhone,
    //       CaregiverMobile, ReferralPhone) are identical except for the name prefix.
    //       Extract a PhoneFieldViewModel class with DialCode, Number, Formatted,
    //       and Full properties, then compose five instances here.
    public class PatientViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        // Dirty state management

        private Dictionary<string, object> _originalValues = new Dictionary<string, object>();
        private readonly HashSet<string> _dirtyProperties = new HashSet<string>();
        private readonly Stack<KeyValuePair<string, object>> _undoStack = new Stack<KeyValuePair<string, object>>();
        private bool _isUndoing;
        private bool _isDirty = false;
        private Dictionary<string, string> _validationErrors = new Dictionary<string, string>();
        private BitmapImage _qrCodeImage;

        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasValidationErrors => _validationErrors.Any();

        /// <summary>
        /// True when there is at least one recorded field change that <see cref="Undo"/>
        /// can roll back. Independent of <see cref="IsDirty"/> — undoing every change
        /// returns to the snapshot, but a field manually edited back to its original
        /// value is no longer dirty yet still has an undo entry.
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;

        public event PropertyChangedEventHandler PropertyChanged;

        // Backing fields — patient

        private string _patientId;
        private string _hospitalId;
        private string _firstName;
        private string _lastName;
        private DateTime? _dateOfBirth;
        private string _gender;
        private string _gestationalAge;
        private string _weight;
        private string _height;
        private string _birthLocation;
        private string _nationality;
        private string _screeningConsent;
        private string _consentState;
        private string _nicu;
        private DateTime? _discharged;
        private DateTime? _deceased;
        private string _trackingConsent;
        private string _comments;

        // Backing fields — mother
        private string _motherTitle;
        private string _motherSSN;
        private string _motherId;
        private string _motherFirstName;
        private string _motherLastName;
        private DateTime? _motherDateOfBirth;
        private string _motherLanguage;
        private string _motherAddress1;
        private string _motherAddress2;
        private string _motherCity;
        private string _motherState;
        private string _motherZipCode;
        private string _motherCountry;
        private string _motherPhone;
        private string _motherMobilePhone;
        private string _motherFax;
        private string _motherEmail;

        // Backing fields — caregiver
        private string _caregiverTitle;
        private string _caregiverSSN;
        private string _caregiverFirstName;
        private string _caregiverLastName;
        private string _caregiverLanguage;
        private string _caregiverAddress1;
        private string _caregiverAddress2;
        private string _caregiverCity;
        private string _caregiverState;
        private string _caregiverZipCode;
        private string _caregiverCountry;
        private string _caregiverPhone;
        private string _caregiverMobilePhone;
        private string _caregiverFax;
        private string _caregiverEmail;

        // Backing fields — referral
        private string _audiologyReferral;
        private DateTime? _referralDate;
        private string _referralTo;
        private string _referralFrom;
        private string _referralPhone;

        // Backing fields — medical
        private string _medication;
        private string _physician;
        private string _audiologist;

        // Backing fields — risk factors (perinatal)
        private string _familyHistory = "Unknown";
        private string _lowBirthWeight = "Unknown";
        private string _hyperbilirubinemia = "Unknown";
        private string _asphyxia = "Unknown";
        private string _craniofacialAnomalies = "Unknown";
        private string _syndromes = "Unknown";
        private string _inUteroInfections = "Unknown";

        // Backing fields — risk factors (postnatal)
        private string _bacterialMeningitis = "Unknown";
        private string _perinatalInfection = "Unknown";
        private string _ototoxicMedications = "Unknown";
        private string _aminoglycosides = "Unknown";
        private string _prolongedVentilation = "Unknown";
        private string _ecmo = "Unknown";
        private string _nicuStay = "Unknown";
        private string _headTrauma = "Unknown";

        // Backing fields — risk factors (other)
        private string _caregiverConcern = "Unknown";

        // Backing fields — screening
        private string _leftEarResult;
        private string _rightEarResult;

        public BitmapImage QRCodeImage
        {
            get => _qrCodeImage;
            private set => SetProperty(ref _qrCodeImage, value);
        }

        public void GenerateQRCode()
        {
            string qrContent = QRCodeHelper.FormatPatientData(
                FirstName,
                LastName,
                PatientId,
                DateOfBirth,
                Gender,
                HospitalId
            );

            QRCodeImage = QRCodeHelper.GenerateQRCode(qrContent, pixelsPerModule: 3);
        }

        #region Phone Properties with Dial Codes
        // Each phone field has four properties: DialCode, Number, Formatted (display),
        // and Full (international format for reports). See class-level TODO about
        // extracting a shared PhoneFieldViewModel.

        // Mother phone
        private string _motherPhoneDialCode = "+1";
        public string MotherPhoneDialCode
        {
            get => _motherPhoneDialCode;
            set
            {
                if (SetProperty(ref _motherPhoneDialCode, value))
                {
                    OnPropertyChanged(nameof(MotherPhoneFormatted));
                    OnPropertyChanged(nameof(MotherPhoneFull));
                }
            }
        }

        private string _motherPhoneNumber = string.Empty;
        public string MotherPhoneNumber
        {
            get => _motherPhoneNumber;
            set
            {
                if (SetProperty(ref _motherPhoneNumber, value))
                {
                    OnPropertyChanged(nameof(MotherPhoneFormatted));
                    OnPropertyChanged(nameof(MotherPhoneFull));
                }
            }
        }

        public string MotherPhoneFormatted
        {
            get => PhoneNumberFormatter.Format(MotherPhoneNumber, MotherPhoneDialCode);
        }

        public string MotherPhoneFull
        {
            get => string.IsNullOrEmpty(MotherPhoneNumber)
                ? string.Empty
                : $"{MotherPhoneDialCode} {MotherPhoneFormatted}";
        }

        // Mother mobile
        private string _motherMobilePhoneDialCode = "+1";
        public string MotherMobilePhoneDialCode
        {
            get => _motherMobilePhoneDialCode;
            set
            {
                if (SetProperty(ref _motherMobilePhoneDialCode, value))
                {
                    OnPropertyChanged(nameof(MotherMobilePhoneFormatted));
                    OnPropertyChanged(nameof(MotherMobilePhoneFull));
                }
            }
        }

        private string _motherMobilePhoneNumber = string.Empty;
        public string MotherMobilePhoneNumber
        {
            get => _motherMobilePhoneNumber;
            set
            {
                if (SetProperty(ref _motherMobilePhoneNumber, value))
                {
                    OnPropertyChanged(nameof(MotherMobilePhoneFormatted));
                    OnPropertyChanged(nameof(MotherMobilePhoneFull));
                }
            }
        }

        public string MotherMobilePhoneFormatted
        {
            get => PhoneNumberFormatter.Format(MotherMobilePhoneNumber, MotherMobilePhoneDialCode);
        }

        public string MotherMobilePhoneFull
        {
            get => string.IsNullOrEmpty(MotherMobilePhoneNumber)
                ? string.Empty
                : $"{MotherMobilePhoneDialCode} {MotherMobilePhoneFormatted}";
        }

        // Caregiver phone
        private string _caregiverPhoneDialCode = "+1";
        public string CaregiverPhoneDialCode
        {
            get => _caregiverPhoneDialCode;
            set
            {
                if (SetProperty(ref _caregiverPhoneDialCode, value))
                {
                    OnPropertyChanged(nameof(CaregiverPhoneFormatted));
                    OnPropertyChanged(nameof(CaregiverPhoneFull));
                }
            }
        }

        private string _caregiverPhoneNumber = string.Empty;
        public string CaregiverPhoneNumber
        {
            get => _caregiverPhoneNumber;
            set
            {
                if (SetProperty(ref _caregiverPhoneNumber, value))
                {
                    OnPropertyChanged(nameof(CaregiverPhoneFormatted));
                    OnPropertyChanged(nameof(CaregiverPhoneFull));
                }
            }
        }

        public string CaregiverPhoneFormatted
        {
            get => PhoneNumberFormatter.Format(CaregiverPhoneNumber, CaregiverPhoneDialCode);
        }

        public string CaregiverPhoneFull
        {
            get => string.IsNullOrEmpty(CaregiverPhoneNumber)
                ? string.Empty
                : $"{CaregiverPhoneDialCode} {CaregiverPhoneFormatted}";
        }

        // Caregiver mobile
        private string _caregiverMobilePhoneDialCode = "+1";
        public string CaregiverMobilePhoneDialCode
        {
            get => _caregiverMobilePhoneDialCode;
            set
            {
                if (SetProperty(ref _caregiverMobilePhoneDialCode, value))
                {
                    OnPropertyChanged(nameof(CaregiverMobilePhoneFormatted));
                    OnPropertyChanged(nameof(CaregiverMobilePhoneFull));
                }
            }
        }

        private string _caregiverMobilePhoneNumber = string.Empty;
        public string CaregiverMobilePhoneNumber
        {
            get => _caregiverMobilePhoneNumber;
            set
            {
                if (SetProperty(ref _caregiverMobilePhoneNumber, value))
                {
                    OnPropertyChanged(nameof(CaregiverMobilePhoneFormatted));
                    OnPropertyChanged(nameof(CaregiverMobilePhoneFull));
                }
            }
        }

        public string CaregiverMobilePhoneFormatted
        {
            get => PhoneNumberFormatter.Format(CaregiverMobilePhoneNumber, CaregiverMobilePhoneDialCode);
        }

        public string CaregiverMobilePhoneFull
        {
            get => string.IsNullOrEmpty(CaregiverMobilePhoneNumber)
                ? string.Empty
                : $"{CaregiverMobilePhoneDialCode} {CaregiverMobilePhoneFormatted}";
        }

        // Referral phone
        private string _referralPhoneDialCode = "+1";
        public string ReferralPhoneDialCode
        {
            get => _referralPhoneDialCode;
            set
            {
                if (SetProperty(ref _referralPhoneDialCode, value))
                {
                    OnPropertyChanged(nameof(ReferralPhoneFormatted));
                    OnPropertyChanged(nameof(ReferralPhoneFull));
                }
            }
        }

        private string _referralPhoneNumber = string.Empty;
        public string ReferralPhoneNumber
        {
            get => _referralPhoneNumber;
            set
            {
                if (SetProperty(ref _referralPhoneNumber, value))
                {
                    OnPropertyChanged(nameof(ReferralPhoneFormatted));
                    OnPropertyChanged(nameof(ReferralPhoneFull));
                }
            }
        }

        public string ReferralPhoneFormatted
        {
            get => PhoneNumberFormatter.Format(ReferralPhoneNumber, ReferralPhoneDialCode);
        }

        public string ReferralPhoneFull
        {
            get => string.IsNullOrEmpty(ReferralPhoneNumber)
                ? string.Empty
                : $"{ReferralPhoneDialCode} {ReferralPhoneFormatted}";
        }

        #endregion

        // Core properties with validation

        public string PatientId
        {
            get => _patientId;
            set
            {
                if (SetProperty(ref _patientId, value))
                {
                    ValidateProperty(value, "Patient ID is required");
                    UpdateRequiredFieldsCount();
                }
            }
        }

        public string HospitalId
        {
            get => _hospitalId;
            set
            {
                if (SetProperty(ref _hospitalId, value))
                {
                    ValidateProperty(value, "Hospital ID is required");
                    UpdateRequiredFieldsCount();
                }
            }
        }

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (SetProperty(ref _lastName, value))
                {
                    ValidateProperty(value, "Last Name is required");
                    UpdateRequiredFieldsCount();
                }
            }
        }

        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value);
        }

        public string Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        public string GestationalAge
        {
            get => _gestationalAge;
            set => SetProperty(ref _gestationalAge, value);
        }

        public string Weight
        {
            get => _weight;
            set => SetProperty(ref _weight, value);
        }

        public string Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public string BirthLocation
        {
            get => _birthLocation;
            set => SetProperty(ref _birthLocation, value);
        }

        public string Nationality
        {
            get => _nationality;
            set => SetProperty(ref _nationality, value);
        }

        public string ScreeningConsent
        {
            get => _screeningConsent;
            set => SetProperty(ref _screeningConsent, value);
        }

        public string ConsentState
        {
            get => _consentState;
            set => SetProperty(ref _consentState, value);
        }

        public string NICU
        {
            get => _nicu;
            set => SetProperty(ref _nicu, value);
        }

        public DateTime? Discharged
        {
            get => _discharged;
            set => SetProperty(ref _discharged, value);
        }

        public DateTime? Deceased
        {
            get => _deceased;
            set => SetProperty(ref _deceased, value);
        }

        public string TrackingConsent
        {
            get => _trackingConsent;
            set => SetProperty(ref _trackingConsent, value);
        }

        public string Comments
        {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }

        // Mother properties

        public string MotherTitle
        {
            get => _motherTitle;
            set => SetProperty(ref _motherTitle, value);
        }

        public string MotherSSN
        {
            get => _motherSSN;
            set => SetProperty(ref _motherSSN, value);
        }

        public string MotherId
        {
            get => _motherId;
            set => SetProperty(ref _motherId, value);
        }

        public string MotherFirstName
        {
            get => _motherFirstName;
            set => SetProperty(ref _motherFirstName, value);
        }

        public string MotherLastName
        {
            get => _motherLastName;
            set => SetProperty(ref _motherLastName, value);
        }

        public DateTime? MotherDateOfBirth
        {
            get => _motherDateOfBirth;
            set => SetProperty(ref _motherDateOfBirth, value);
        }

        public string MotherLanguage
        {
            get => _motherLanguage;
            set => SetProperty(ref _motherLanguage, value);
        }

        public string MotherAddress1
        {
            get => _motherAddress1;
            set => SetProperty(ref _motherAddress1, value);
        }

        public string MotherAddress2
        {
            get => _motherAddress2;
            set => SetProperty(ref _motherAddress2, value);
        }

        public string MotherCity
        {
            get => _motherCity;
            set => SetProperty(ref _motherCity, value);
        }

        public string MotherState
        {
            get => _motherState;
            set => SetProperty(ref _motherState, value);
        }

        public string MotherZipCode
        {
            get => _motherZipCode;
            set => SetProperty(ref _motherZipCode, value);
        }

        public string MotherCountry
        {
            get => _motherCountry;
            set => SetProperty(ref _motherCountry, value);
        }

        public string MotherPhone
        {
            get => _motherPhone;
            set => SetProperty(ref _motherPhone, value);
        }

        public string MotherMobilePhone
        {
            get => _motherMobilePhone;
            set => SetProperty(ref _motherMobilePhone, value);
        }

        public string MotherFax
        {
            get => _motherFax;
            set => SetProperty(ref _motherFax, value);
        }

        public string MotherEmail
        {
            get => _motherEmail;
            set => SetProperty(ref _motherEmail, value);
        }

        // Caregiver properties

        public string CaregiverTitle
        {
            get => _caregiverTitle;
            set => SetProperty(ref _caregiverTitle, value);
        }

        public string CaregiverSSN
        {
            get => _caregiverSSN;
            set => SetProperty(ref _caregiverSSN, value);
        }

        public string CaregiverFirstName
        {
            get => _caregiverFirstName;
            set => SetProperty(ref _caregiverFirstName, value);
        }

        public string CaregiverLastName
        {
            get => _caregiverLastName;
            set => SetProperty(ref _caregiverLastName, value);
        }

        public string CaregiverLanguage
        {
            get => _caregiverLanguage;
            set => SetProperty(ref _caregiverLanguage, value);
        }

        public string CaregiverAddress1
        {
            get => _caregiverAddress1;
            set => SetProperty(ref _caregiverAddress1, value);
        }

        public string CaregiverAddress2
        {
            get => _caregiverAddress2;
            set => SetProperty(ref _caregiverAddress2, value);
        }

        public string CaregiverCity
        {
            get => _caregiverCity;
            set => SetProperty(ref _caregiverCity, value);
        }

        public string CaregiverState
        {
            get => _caregiverState;
            set => SetProperty(ref _caregiverState, value);
        }

        public string CaregiverZipCode
        {
            get => _caregiverZipCode;
            set => SetProperty(ref _caregiverZipCode, value);
        }

        public string CaregiverCountry
        {
            get => _caregiverCountry;
            set => SetProperty(ref _caregiverCountry, value);
        }

        public string CaregiverPhone
        {
            get => _caregiverPhone;
            set => SetProperty(ref _caregiverPhone, value);
        }

        public string CaregiverMobilePhone
        {
            get => _caregiverMobilePhone;
            set => SetProperty(ref _caregiverMobilePhone, value);
        }

        public string CaregiverFax
        {
            get => _caregiverFax;
            set => SetProperty(ref _caregiverFax, value);
        }

        public string CaregiverEmail
        {
            get => _caregiverEmail;
            set => SetProperty(ref _caregiverEmail, value);
        }

        // Referral properties

        public string AudiologyReferral
        {
            get => _audiologyReferral;
            set => SetProperty(ref _audiologyReferral, value);
        }

        public DateTime? ReferralDate
        {
            get => _referralDate;
            set => SetProperty(ref _referralDate, value);
        }

        public string ReferralTo
        {
            get => _referralTo;
            set => SetProperty(ref _referralTo, value);
        }

        public string ReferralFrom
        {
            get => _referralFrom;
            set => SetProperty(ref _referralFrom, value);
        }

        public string ReferralPhone
        {
            get => _referralPhone;
            set => SetProperty(ref _referralPhone, value);
        }

        // Medical properties

        public string Medication
        {
            get => _medication;
            set => SetProperty(ref _medication, value);
        }

        public string Physician
        {
            get => _physician;
            set => SetProperty(ref _physician, value);
        }

        public string Audiologist
        {
            get => _audiologist;
            set => SetProperty(ref _audiologist, value);
        }

        // Risk factor properties — each setter updates summary counts

        public string FamilyHistory
        {
            get => _familyHistory;
            set { if (SetProperty(ref _familyHistory, value)) UpdateRiskFactorCounts(); }
        }

        public string LowBirthWeight
        {
            get => _lowBirthWeight;
            set { if (SetProperty(ref _lowBirthWeight, value)) UpdateRiskFactorCounts(); }
        }

        public string Hyperbilirubinemia
        {
            get => _hyperbilirubinemia;
            set { if (SetProperty(ref _hyperbilirubinemia, value)) UpdateRiskFactorCounts(); }
        }

        public string Asphyxia
        {
            get => _asphyxia;
            set { if (SetProperty(ref _asphyxia, value)) UpdateRiskFactorCounts(); }
        }

        public string CraniofacialAnomalies
        {
            get => _craniofacialAnomalies;
            set { if (SetProperty(ref _craniofacialAnomalies, value)) UpdateRiskFactorCounts(); }
        }

        public string Syndromes
        {
            get => _syndromes;
            set { if (SetProperty(ref _syndromes, value)) UpdateRiskFactorCounts(); }
        }

        public string InUteroInfections
        {
            get => _inUteroInfections;
            set { if (SetProperty(ref _inUteroInfections, value)) UpdateRiskFactorCounts(); }
        }

        public string BacterialMeningitis
        {
            get => _bacterialMeningitis;
            set { if (SetProperty(ref _bacterialMeningitis, value)) UpdateRiskFactorCounts(); }
        }

        public string PerinatalInfection
        {
            get => _perinatalInfection;
            set { if (SetProperty(ref _perinatalInfection, value)) UpdateRiskFactorCounts(); }
        }

        public string OtotoxicMedications
        {
            get => _ototoxicMedications;
            set { if (SetProperty(ref _ototoxicMedications, value)) UpdateRiskFactorCounts(); }
        }

        public string Aminoglycosides
        {
            get => _aminoglycosides;
            set { if (SetProperty(ref _aminoglycosides, value)) UpdateRiskFactorCounts(); }
        }

        public string ProlongedVentilation
        {
            get => _prolongedVentilation;
            set { if (SetProperty(ref _prolongedVentilation, value)) UpdateRiskFactorCounts(); }
        }

        public string ECMO
        {
            get => _ecmo;
            set { if (SetProperty(ref _ecmo, value)) UpdateRiskFactorCounts(); }
        }

        public string NICUStay
        {
            get => _nicuStay;
            set { if (SetProperty(ref _nicuStay, value)) UpdateRiskFactorCounts(); }
        }

        public string HeadTrauma
        {
            get => _headTrauma;
            set { if (SetProperty(ref _headTrauma, value)) UpdateRiskFactorCounts(); }
        }

        public string CaregiverConcernRisk
        {
            get => _caregiverConcern;
            set { if (SetProperty(ref _caregiverConcern, value)) UpdateRiskFactorCounts(); }
        }

        // Risk factor summary — bound to dashboard-style counts in the view

        private int _totalYesCount;
        private int _totalNoCount;
        private int _totalUnknownCount;
        private int _totalAnsweredCount;
        private int _perinatalYesCount;
        private int _postnatalYesCount;
        private int _otherYesCount;

        public int TotalYesCount
        {
            get => _totalYesCount;
            private set => SetProperty(ref _totalYesCount, value);
        }

        public int TotalNoCount
        {
            get => _totalNoCount;
            private set => SetProperty(ref _totalNoCount, value);
        }

        public int TotalUnknownCount
        {
            get => _totalUnknownCount;
            private set => SetProperty(ref _totalUnknownCount, value);
        }

        public int TotalAnsweredCount
        {
            get => _totalAnsweredCount;
            private set => SetProperty(ref _totalAnsweredCount, value);
        }

        public int PerinatalYesCount
        {
            get => _perinatalYesCount;
            private set => SetProperty(ref _perinatalYesCount, value);
        }

        public int PostnatalYesCount
        {
            get => _postnatalYesCount;
            private set => SetProperty(ref _postnatalYesCount, value);
        }

        public int OtherYesCount
        {
            get => _otherYesCount;
            private set => SetProperty(ref _otherYesCount, value);
        }

        private void UpdateRiskFactorCounts()
        {
            var allRisks = new[]
            {
                FamilyHistory, LowBirthWeight, Hyperbilirubinemia, Asphyxia,
                CraniofacialAnomalies, Syndromes, InUteroInfections,
                BacterialMeningitis, PerinatalInfection, OtotoxicMedications,
                Aminoglycosides, ProlongedVentilation, ECMO, NICUStay, HeadTrauma,
                CaregiverConcernRisk
            };

            TotalYesCount = allRisks.Count(r => r == "Yes");
            TotalNoCount = allRisks.Count(r => r == "No");
            TotalUnknownCount = allRisks.Count(r => r == "Unknown");
            TotalAnsweredCount = allRisks.Count(r => r != "Unknown");

            var perinatalRisks = new[] { FamilyHistory, LowBirthWeight, Hyperbilirubinemia,
                                  Asphyxia, CraniofacialAnomalies, Syndromes, InUteroInfections };
            PerinatalYesCount = perinatalRisks.Count(r => r == "Yes");

            var postnatalRisks = new[] { BacterialMeningitis, PerinatalInfection, OtotoxicMedications,
                                  Aminoglycosides, ProlongedVentilation, ECMO, NICUStay, HeadTrauma };
            PostnatalYesCount = postnatalRisks.Count(r => r == "Yes");

            OtherYesCount = CaregiverConcernRisk == "Yes" ? 1 : 0;
        }

        public int GetPerinatalAnsweredCount()
        {
            var risks = new[] { FamilyHistory, LowBirthWeight, Hyperbilirubinemia,
                       Asphyxia, CraniofacialAnomalies, Syndromes, InUteroInfections };
            return risks.Count(r => r != "Unknown");
        }

        public int GetPostnatalAnsweredCount()
        {
            var risks = new[] { BacterialMeningitis, PerinatalInfection, OtotoxicMedications,
                       Aminoglycosides, ProlongedVentilation, ECMO, NICUStay, HeadTrauma };
            return risks.Count(r => r != "Unknown");
        }

        public int GetOtherAnsweredCount()
        {
            return CaregiverConcernRisk != "Unknown" ? 1 : 0;
        }

        // Screening results

        public string LeftEarResult
        {
            get => _leftEarResult;
            set => SetProperty(ref _leftEarResult, value);
        }

        public string RightEarResult
        {
            get => _rightEarResult;
            set => SetProperty(ref _rightEarResult, value);
        }

        // Required fields tracking

        private int _requiredFieldsCount;
        public int RequiredFieldsCount
        {
            get => _requiredFieldsCount;
            private set
            {
                if (_requiredFieldsCount != value)
                {
                    _requiredFieldsCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasRequiredFieldsEmpty));
                }
            }
        }

        public bool HasRequiredFieldsEmpty => RequiredFieldsCount > 0;

        public List<string> GetMissingRequiredFields()
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(PatientId)) missing.Add("Patient ID");
            if (string.IsNullOrWhiteSpace(HospitalId)) missing.Add("Hospital ID");
            if (string.IsNullOrWhiteSpace(LastName)) missing.Add("Last Name");
            return missing;
        }

        private void UpdateRequiredFieldsCount()
        {
            RequiredFieldsCount = GetMissingRequiredFields().Count;
        }

        // Dirty state — BeginEdit / CancelEdit / AcceptChanges

        /// <summary>
        /// Snapshots all property values as the clean baseline.
        /// Call after loading a patient or saving changes.
        /// </summary>
        public void BeginEdit()
        {
            _originalValues.Clear();
            _dirtyProperties.Clear();
            _originalValues[nameof(PatientId)] = PatientId;
            _originalValues[nameof(HospitalId)] = HospitalId;
            _originalValues[nameof(FirstName)] = FirstName;
            _originalValues[nameof(LastName)] = LastName;
            _originalValues[nameof(DateOfBirth)] = DateOfBirth;
            _originalValues[nameof(Gender)] = Gender;
            _originalValues[nameof(GestationalAge)] = GestationalAge;
            _originalValues[nameof(Weight)] = Weight;
            _originalValues[nameof(Height)] = Height;
            _originalValues[nameof(BirthLocation)] = BirthLocation;
            _originalValues[nameof(Nationality)] = Nationality;
            _originalValues[nameof(ScreeningConsent)] = ScreeningConsent;
            _originalValues[nameof(ConsentState)] = ConsentState;
            _originalValues[nameof(NICU)] = NICU;
            _originalValues[nameof(Discharged)] = Discharged;
            _originalValues[nameof(Deceased)] = Deceased;
            _originalValues[nameof(TrackingConsent)] = TrackingConsent;
            _originalValues[nameof(Comments)] = Comments;

            _originalValues[nameof(MotherTitle)] = MotherTitle;
            _originalValues[nameof(MotherSSN)] = MotherSSN;
            _originalValues[nameof(MotherId)] = MotherId;
            _originalValues[nameof(MotherFirstName)] = MotherFirstName;
            _originalValues[nameof(MotherLastName)] = MotherLastName;
            _originalValues[nameof(MotherDateOfBirth)] = MotherDateOfBirth;
            _originalValues[nameof(MotherLanguage)] = MotherLanguage;
            _originalValues[nameof(MotherAddress1)] = MotherAddress1;
            _originalValues[nameof(MotherAddress2)] = MotherAddress2;
            _originalValues[nameof(MotherCity)] = MotherCity;
            _originalValues[nameof(MotherState)] = MotherState;
            _originalValues[nameof(MotherZipCode)] = MotherZipCode;
            _originalValues[nameof(MotherCountry)] = MotherCountry;
            _originalValues[nameof(MotherPhone)] = MotherPhone;
            _originalValues[nameof(MotherMobilePhone)] = MotherMobilePhone;
            _originalValues[nameof(MotherFax)] = MotherFax;
            _originalValues[nameof(MotherEmail)] = MotherEmail;

            _originalValues[nameof(CaregiverTitle)] = CaregiverTitle;
            _originalValues[nameof(CaregiverSSN)] = CaregiverSSN;
            _originalValues[nameof(CaregiverFirstName)] = CaregiverFirstName;
            _originalValues[nameof(CaregiverLastName)] = CaregiverLastName;
            _originalValues[nameof(CaregiverLanguage)] = CaregiverLanguage;
            _originalValues[nameof(CaregiverAddress1)] = CaregiverAddress1;
            _originalValues[nameof(CaregiverAddress2)] = CaregiverAddress2;
            _originalValues[nameof(CaregiverCity)] = CaregiverCity;
            _originalValues[nameof(CaregiverState)] = CaregiverState;
            _originalValues[nameof(CaregiverZipCode)] = CaregiverZipCode;
            _originalValues[nameof(CaregiverCountry)] = CaregiverCountry;
            _originalValues[nameof(CaregiverPhone)] = CaregiverPhone;
            _originalValues[nameof(CaregiverMobilePhone)] = CaregiverMobilePhone;
            _originalValues[nameof(CaregiverFax)] = CaregiverFax;
            _originalValues[nameof(CaregiverEmail)] = CaregiverEmail;

            _originalValues[nameof(AudiologyReferral)] = AudiologyReferral;
            _originalValues[nameof(ReferralDate)] = ReferralDate;
            _originalValues[nameof(ReferralTo)] = ReferralTo;
            _originalValues[nameof(ReferralFrom)] = ReferralFrom;
            _originalValues[nameof(ReferralPhone)] = ReferralPhone;

            _originalValues[nameof(Medication)] = Medication;
            _originalValues[nameof(Physician)] = Physician;
            _originalValues[nameof(Audiologist)] = Audiologist;

            _originalValues[nameof(MotherPhoneDialCode)] = MotherPhoneDialCode;
            _originalValues[nameof(MotherPhoneNumber)] = MotherPhoneNumber;
            _originalValues[nameof(MotherMobilePhoneDialCode)] = MotherMobilePhoneDialCode;
            _originalValues[nameof(MotherMobilePhoneNumber)] = MotherMobilePhoneNumber;
            _originalValues[nameof(CaregiverPhoneDialCode)] = CaregiverPhoneDialCode;
            _originalValues[nameof(CaregiverPhoneNumber)] = CaregiverPhoneNumber;
            _originalValues[nameof(CaregiverMobilePhoneDialCode)] = CaregiverMobilePhoneDialCode;
            _originalValues[nameof(CaregiverMobilePhoneNumber)] = CaregiverMobilePhoneNumber;
            _originalValues[nameof(ReferralPhoneDialCode)] = ReferralPhoneDialCode;
            _originalValues[nameof(ReferralPhoneNumber)] = ReferralPhoneNumber;

            _originalValues[nameof(FamilyHistory)] = FamilyHistory;
            _originalValues[nameof(LowBirthWeight)] = LowBirthWeight;
            _originalValues[nameof(Hyperbilirubinemia)] = Hyperbilirubinemia;
            _originalValues[nameof(Asphyxia)] = Asphyxia;
            _originalValues[nameof(CraniofacialAnomalies)] = CraniofacialAnomalies;
            _originalValues[nameof(Syndromes)] = Syndromes;
            _originalValues[nameof(InUteroInfections)] = InUteroInfections;
            _originalValues[nameof(BacterialMeningitis)] = BacterialMeningitis;
            _originalValues[nameof(PerinatalInfection)] = PerinatalInfection;
            _originalValues[nameof(OtotoxicMedications)] = OtotoxicMedications;
            _originalValues[nameof(Aminoglycosides)] = Aminoglycosides;
            _originalValues[nameof(ProlongedVentilation)] = ProlongedVentilation;
            _originalValues[nameof(ECMO)] = ECMO;
            _originalValues[nameof(NICUStay)] = NICUStay;
            _originalValues[nameof(HeadTrauma)] = HeadTrauma;
            _originalValues[nameof(CaregiverConcernRisk)] = CaregiverConcernRisk;

            _originalValues[nameof(LeftEarResult)] = LeftEarResult;
            _originalValues[nameof(RightEarResult)] = RightEarResult;

            ClearUndoHistory();
            IsDirty = false;
        }

        /// <summary>
        /// Reverts all properties to the values captured by <see cref="BeginEdit"/>.
        /// </summary>
        public void CancelEdit()
        {
            if (_originalValues.ContainsKey(nameof(PatientId))) PatientId = (string)_originalValues[nameof(PatientId)];
            if (_originalValues.ContainsKey(nameof(HospitalId))) HospitalId = (string)_originalValues[nameof(HospitalId)];
            if (_originalValues.ContainsKey(nameof(FirstName))) FirstName = (string)_originalValues[nameof(FirstName)];
            if (_originalValues.ContainsKey(nameof(LastName))) LastName = (string)_originalValues[nameof(LastName)];
            if (_originalValues.ContainsKey(nameof(DateOfBirth))) DateOfBirth = (DateTime?)_originalValues[nameof(DateOfBirth)];
            if (_originalValues.ContainsKey(nameof(Gender))) Gender = (string)_originalValues[nameof(Gender)];
            if (_originalValues.ContainsKey(nameof(GestationalAge))) GestationalAge = (string)_originalValues[nameof(GestationalAge)];
            if (_originalValues.ContainsKey(nameof(Weight))) Weight = (string)_originalValues[nameof(Weight)];
            if (_originalValues.ContainsKey(nameof(Height))) Height = (string)_originalValues[nameof(Height)];
            if (_originalValues.ContainsKey(nameof(BirthLocation))) BirthLocation = (string)_originalValues[nameof(BirthLocation)];
            if (_originalValues.ContainsKey(nameof(Nationality))) Nationality = (string)_originalValues[nameof(Nationality)];
            if (_originalValues.ContainsKey(nameof(ScreeningConsent))) ScreeningConsent = (string)_originalValues[nameof(ScreeningConsent)];
            if (_originalValues.ContainsKey(nameof(ConsentState))) ConsentState = (string)_originalValues[nameof(ConsentState)];
            if (_originalValues.ContainsKey(nameof(NICU))) NICU = (string)_originalValues[nameof(NICU)];
            if (_originalValues.ContainsKey(nameof(Discharged))) Discharged = (DateTime?)_originalValues[nameof(Discharged)];
            if (_originalValues.ContainsKey(nameof(Deceased))) Deceased = (DateTime?)_originalValues[nameof(Deceased)];
            if (_originalValues.ContainsKey(nameof(TrackingConsent))) TrackingConsent = (string)_originalValues[nameof(TrackingConsent)];
            if (_originalValues.ContainsKey(nameof(Comments))) Comments = (string)_originalValues[nameof(Comments)];

            if (_originalValues.ContainsKey(nameof(MotherTitle))) MotherTitle = (string)_originalValues[nameof(MotherTitle)];
            if (_originalValues.ContainsKey(nameof(MotherSSN))) MotherSSN = (string)_originalValues[nameof(MotherSSN)];
            if (_originalValues.ContainsKey(nameof(MotherId))) MotherId = (string)_originalValues[nameof(MotherId)];
            if (_originalValues.ContainsKey(nameof(MotherFirstName))) MotherFirstName = (string)_originalValues[nameof(MotherFirstName)];
            if (_originalValues.ContainsKey(nameof(MotherLastName))) MotherLastName = (string)_originalValues[nameof(MotherLastName)];
            if (_originalValues.ContainsKey(nameof(MotherDateOfBirth))) MotherDateOfBirth = (DateTime?)_originalValues[nameof(MotherDateOfBirth)];
            if (_originalValues.ContainsKey(nameof(MotherLanguage))) MotherLanguage = (string)_originalValues[nameof(MotherLanguage)];
            if (_originalValues.ContainsKey(nameof(MotherAddress1))) MotherAddress1 = (string)_originalValues[nameof(MotherAddress1)];
            if (_originalValues.ContainsKey(nameof(MotherAddress2))) MotherAddress2 = (string)_originalValues[nameof(MotherAddress2)];
            if (_originalValues.ContainsKey(nameof(MotherCity))) MotherCity = (string)_originalValues[nameof(MotherCity)];
            if (_originalValues.ContainsKey(nameof(MotherState))) MotherState = (string)_originalValues[nameof(MotherState)];
            if (_originalValues.ContainsKey(nameof(MotherZipCode))) MotherZipCode = (string)_originalValues[nameof(MotherZipCode)];
            if (_originalValues.ContainsKey(nameof(MotherCountry))) MotherCountry = (string)_originalValues[nameof(MotherCountry)];
            if (_originalValues.ContainsKey(nameof(MotherPhone))) MotherPhone = (string)_originalValues[nameof(MotherPhone)];
            if (_originalValues.ContainsKey(nameof(MotherMobilePhone))) MotherMobilePhone = (string)_originalValues[nameof(MotherMobilePhone)];
            if (_originalValues.ContainsKey(nameof(MotherFax))) MotherFax = (string)_originalValues[nameof(MotherFax)];
            if (_originalValues.ContainsKey(nameof(MotherEmail))) MotherEmail = (string)_originalValues[nameof(MotherEmail)];

            if (_originalValues.ContainsKey(nameof(CaregiverTitle))) CaregiverTitle = (string)_originalValues[nameof(CaregiverTitle)];
            if (_originalValues.ContainsKey(nameof(CaregiverSSN))) CaregiverSSN = (string)_originalValues[nameof(CaregiverSSN)];
            if (_originalValues.ContainsKey(nameof(CaregiverFirstName))) CaregiverFirstName = (string)_originalValues[nameof(CaregiverFirstName)];
            if (_originalValues.ContainsKey(nameof(CaregiverLastName))) CaregiverLastName = (string)_originalValues[nameof(CaregiverLastName)];
            if (_originalValues.ContainsKey(nameof(CaregiverLanguage))) CaregiverLanguage = (string)_originalValues[nameof(CaregiverLanguage)];
            if (_originalValues.ContainsKey(nameof(CaregiverAddress1))) CaregiverAddress1 = (string)_originalValues[nameof(CaregiverAddress1)];
            if (_originalValues.ContainsKey(nameof(CaregiverAddress2))) CaregiverAddress2 = (string)_originalValues[nameof(CaregiverAddress2)];
            if (_originalValues.ContainsKey(nameof(CaregiverCity))) CaregiverCity = (string)_originalValues[nameof(CaregiverCity)];
            if (_originalValues.ContainsKey(nameof(CaregiverState))) CaregiverState = (string)_originalValues[nameof(CaregiverState)];
            if (_originalValues.ContainsKey(nameof(CaregiverZipCode))) CaregiverZipCode = (string)_originalValues[nameof(CaregiverZipCode)];
            if (_originalValues.ContainsKey(nameof(CaregiverCountry))) CaregiverCountry = (string)_originalValues[nameof(CaregiverCountry)];
            if (_originalValues.ContainsKey(nameof(CaregiverPhone))) CaregiverPhone = (string)_originalValues[nameof(CaregiverPhone)];
            if (_originalValues.ContainsKey(nameof(CaregiverMobilePhone))) CaregiverMobilePhone = (string)_originalValues[nameof(CaregiverMobilePhone)];
            if (_originalValues.ContainsKey(nameof(CaregiverFax))) CaregiverFax = (string)_originalValues[nameof(CaregiverFax)];
            if (_originalValues.ContainsKey(nameof(CaregiverEmail))) CaregiverEmail = (string)_originalValues[nameof(CaregiverEmail)];

            if (_originalValues.ContainsKey(nameof(AudiologyReferral))) AudiologyReferral = (string)_originalValues[nameof(AudiologyReferral)];
            if (_originalValues.ContainsKey(nameof(ReferralDate))) ReferralDate = (DateTime?)_originalValues[nameof(ReferralDate)];
            if (_originalValues.ContainsKey(nameof(ReferralTo))) ReferralTo = (string)_originalValues[nameof(ReferralTo)];
            if (_originalValues.ContainsKey(nameof(ReferralFrom))) ReferralFrom = (string)_originalValues[nameof(ReferralFrom)];
            if (_originalValues.ContainsKey(nameof(ReferralPhone))) ReferralPhone = (string)_originalValues[nameof(ReferralPhone)];

            if (_originalValues.ContainsKey(nameof(Medication))) Medication = (string)_originalValues[nameof(Medication)];
            if (_originalValues.ContainsKey(nameof(Physician))) Physician = (string)_originalValues[nameof(Physician)];
            if (_originalValues.ContainsKey(nameof(Audiologist))) Audiologist = (string)_originalValues[nameof(Audiologist)];

            if (_originalValues.ContainsKey(nameof(MotherPhoneDialCode))) MotherPhoneDialCode = (string)_originalValues[nameof(MotherPhoneDialCode)];
            if (_originalValues.ContainsKey(nameof(MotherPhoneNumber))) MotherPhoneNumber = (string)_originalValues[nameof(MotherPhoneNumber)];
            if (_originalValues.ContainsKey(nameof(MotherMobilePhoneDialCode))) MotherMobilePhoneDialCode = (string)_originalValues[nameof(MotherMobilePhoneDialCode)];
            if (_originalValues.ContainsKey(nameof(MotherMobilePhoneNumber))) MotherMobilePhoneNumber = (string)_originalValues[nameof(MotherMobilePhoneNumber)];
            if (_originalValues.ContainsKey(nameof(CaregiverPhoneDialCode))) CaregiverPhoneDialCode = (string)_originalValues[nameof(CaregiverPhoneDialCode)];
            if (_originalValues.ContainsKey(nameof(CaregiverPhoneNumber))) CaregiverPhoneNumber = (string)_originalValues[nameof(CaregiverPhoneNumber)];
            if (_originalValues.ContainsKey(nameof(CaregiverMobilePhoneDialCode))) CaregiverMobilePhoneDialCode = (string)_originalValues[nameof(CaregiverMobilePhoneDialCode)];
            if (_originalValues.ContainsKey(nameof(CaregiverMobilePhoneNumber))) CaregiverMobilePhoneNumber = (string)_originalValues[nameof(CaregiverMobilePhoneNumber)];
            if (_originalValues.ContainsKey(nameof(ReferralPhoneDialCode))) ReferralPhoneDialCode = (string)_originalValues[nameof(ReferralPhoneDialCode)];
            if (_originalValues.ContainsKey(nameof(ReferralPhoneNumber))) ReferralPhoneNumber = (string)_originalValues[nameof(ReferralPhoneNumber)];

            if (_originalValues.ContainsKey(nameof(FamilyHistory))) FamilyHistory = (string)_originalValues[nameof(FamilyHistory)];
            if (_originalValues.ContainsKey(nameof(BacterialMeningitis))) BacterialMeningitis = (string)_originalValues[nameof(BacterialMeningitis)];
            if (_originalValues.ContainsKey(nameof(CraniofacialAnomalies))) CraniofacialAnomalies = (string)_originalValues[nameof(CraniofacialAnomalies)];
            if (_originalValues.ContainsKey(nameof(Hyperbilirubinemia))) Hyperbilirubinemia = (string)_originalValues[nameof(Hyperbilirubinemia)];
            if (_originalValues.ContainsKey(nameof(LowBirthWeight))) LowBirthWeight = (string)_originalValues[nameof(LowBirthWeight)];
            if (_originalValues.ContainsKey(nameof(OtotoxicMedications))) OtotoxicMedications = (string)_originalValues[nameof(OtotoxicMedications)];
            if (_originalValues.ContainsKey(nameof(PerinatalInfection))) PerinatalInfection = (string)_originalValues[nameof(PerinatalInfection)];
            if (_originalValues.ContainsKey(nameof(ProlongedVentilation))) ProlongedVentilation = (string)_originalValues[nameof(ProlongedVentilation)];
            if (_originalValues.ContainsKey(nameof(Asphyxia))) Asphyxia = (string)_originalValues[nameof(Asphyxia)];
            if (_originalValues.ContainsKey(nameof(Syndromes))) Syndromes = (string)_originalValues[nameof(Syndromes)];
            if (_originalValues.ContainsKey(nameof(Aminoglycosides))) Aminoglycosides = (string)_originalValues[nameof(Aminoglycosides)];
            if (_originalValues.ContainsKey(nameof(InUteroInfections))) InUteroInfections = (string)_originalValues[nameof(InUteroInfections)];
            if (_originalValues.ContainsKey(nameof(CaregiverConcernRisk))) CaregiverConcernRisk = (string)_originalValues[nameof(CaregiverConcernRisk)];
            if (_originalValues.ContainsKey(nameof(NICUStay))) NICUStay = (string)_originalValues[nameof(NICUStay)];
            if (_originalValues.ContainsKey(nameof(HeadTrauma))) HeadTrauma = (string)_originalValues[nameof(HeadTrauma)];
            if (_originalValues.ContainsKey(nameof(ECMO))) ECMO = (string)_originalValues[nameof(ECMO)];

            if (_originalValues.ContainsKey(nameof(LeftEarResult))) LeftEarResult = (string)_originalValues[nameof(LeftEarResult)];
            if (_originalValues.ContainsKey(nameof(RightEarResult))) RightEarResult = (string)_originalValues[nameof(RightEarResult)];

            ClearUndoHistory();
            IsDirty = false;
        }

        /// <summary>
        /// Reverts only the most recent field change, leaving all earlier edits intact.
        /// Returns false when there is nothing to undo.
        /// </summary>
        public bool Undo()
        {
            if (_undoStack.Count == 0) return false;

            var change = _undoStack.Pop();
            _isUndoing = true;
            try
            {
                // Restore through the normal setter so dirty tracking and change
                // notifications update; the _isUndoing guard keeps SetProperty from
                // recording this restore as a new undo entry.
                var prop = GetType().GetProperty(change.Key);
                if (prop != null && prop.CanWrite)
                    prop.SetValue(this, change.Value);
            }
            finally
            {
                _isUndoing = false;
            }

            OnPropertyChanged(nameof(CanUndo));
            return true;
        }

        private void ClearUndoHistory()
        {
            if (_undoStack.Count == 0) return;
            _undoStack.Clear();
            OnPropertyChanged(nameof(CanUndo));
        }

        /// <summary>
        /// Accepts current values as the new clean baseline.
        /// </summary>
        public void AcceptChanges()
        {
            BeginEdit();
        }

        // Validation (IDataErrorInfo)

        public string Error => null;

        public string this[string propertyName]
        {
            get
            {
                if (_validationErrors.ContainsKey(propertyName))
                    return _validationErrors[propertyName];
                return null;
            }
        }

        private void ValidateProperty(string value, string errorMessage, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (!_validationErrors.ContainsKey(propertyName))
                {
                    _validationErrors[propertyName] = errorMessage;
                }
            }
            else
            {
                if (_validationErrors.ContainsKey(propertyName))
                {
                    _validationErrors.Remove(propertyName);
                }
            }

            // Raises the indexer change so WPF validation adorners update
            OnPropertyChanged($"Item[{propertyName}]");
        }

        // Infrastructure

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            T oldValue = storage;
            storage = value;
            OnPropertyChanged(propertyName);

            // Only track dirty state once a baseline has been snapshotted (BeginEdit).
            // Compare against the original so reverting a field back to its snapshot
            // value clears its dirty mark.
            if (_originalValues.Count > 0 && propertyName != null &&
                _originalValues.TryGetValue(propertyName, out var originalValue))
            {
                // Record the prior value so a single change can be rolled back. Skip
                // while applying an undo, otherwise the restore would push its own
                // reverse entry and the stack would never drain.
                if (!_isUndoing)
                {
                    _undoStack.Push(new KeyValuePair<string, object>(propertyName, oldValue));
                    OnPropertyChanged(nameof(CanUndo));
                }

                if (Equals(originalValue, value))
                    _dirtyProperties.Remove(propertyName);
                else
                    _dirtyProperties.Add(propertyName);

                IsDirty = _dirtyProperties.Count > 0;
            }

            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}