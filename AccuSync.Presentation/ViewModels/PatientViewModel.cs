// --------------------------------------------------------------------------------
// <copyright file="PatientViewModel.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Helpers;
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

        /// <summary>
        /// True when at least one property differs from the snapshot taken by <see cref="BeginEdit"/>.
        /// </summary>
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

        /// <summary>True when any field currently fails required-field validation.</summary>
        public bool HasValidationErrors => _validationErrors.Any();

        /// <summary>
        /// True when there is at least one recorded field change that <see cref="Undo"/>
        /// can roll back. Independent of <see cref="IsDirty"/> — undoing every change
        /// returns to the snapshot, but a field manually edited back to its original
        /// value is no longer dirty yet still has an undo entry.
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>Raised whenever a bound property's value changes.</summary>
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

        /// <summary>QR code image encoding the current patient's identifying details.</summary>
        public BitmapImage QRCodeImage
        {
            get => _qrCodeImage;
            private set => SetProperty(ref _qrCodeImage, value);
        }

        /// <summary>
        /// Builds <see cref="QRCodeImage"/> from the current patient identity fields.
        /// </summary>
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
        /// <summary>Country dial code for <see cref="MotherPhoneNumber"/>, e.g. "+1".</summary>
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
        /// <summary>Mother's phone number, unformatted digits as entered.</summary>
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

        /// <summary>Mother's phone number formatted for display.</summary>
        public string MotherPhoneFormatted
        {
            get => PhoneNumberFormatter.Format(MotherPhoneNumber, MotherPhoneDialCode);
        }

        /// <summary>Mother's phone number with dial code, formatted for reports.</summary>
        public string MotherPhoneFull
        {
            get => string.IsNullOrEmpty(MotherPhoneNumber)
                ? string.Empty
                : $"{MotherPhoneDialCode} {MotherPhoneFormatted}";
        }

        // Mother mobile
        private string _motherMobilePhoneDialCode = "+1";
        /// <summary>Country dial code for <see cref="MotherMobilePhoneNumber"/>, e.g. "+1".</summary>
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
        /// <summary>Mother's mobile number, unformatted digits as entered.</summary>
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

        /// <summary>Mother's mobile number formatted for display.</summary>
        public string MotherMobilePhoneFormatted
        {
            get => PhoneNumberFormatter.Format(MotherMobilePhoneNumber, MotherMobilePhoneDialCode);
        }

        /// <summary>Mother's mobile number with dial code, formatted for reports.</summary>
        public string MotherMobilePhoneFull
        {
            get => string.IsNullOrEmpty(MotherMobilePhoneNumber)
                ? string.Empty
                : $"{MotherMobilePhoneDialCode} {MotherMobilePhoneFormatted}";
        }

        // Caregiver phone
        private string _caregiverPhoneDialCode = "+1";
        /// <summary>Country dial code for <see cref="CaregiverPhoneNumber"/>, e.g. "+1".</summary>
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
        /// <summary>Caregiver's phone number, unformatted digits as entered.</summary>
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

        /// <summary>Caregiver's phone number formatted for display.</summary>
        public string CaregiverPhoneFormatted
        {
            get => PhoneNumberFormatter.Format(CaregiverPhoneNumber, CaregiverPhoneDialCode);
        }

        /// <summary>Caregiver's phone number with dial code, formatted for reports.</summary>
        public string CaregiverPhoneFull
        {
            get => string.IsNullOrEmpty(CaregiverPhoneNumber)
                ? string.Empty
                : $"{CaregiverPhoneDialCode} {CaregiverPhoneFormatted}";
        }

        // Caregiver mobile
        private string _caregiverMobilePhoneDialCode = "+1";
        /// <summary>Country dial code for <see cref="CaregiverMobilePhoneNumber"/>, e.g. "+1".</summary>
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
        /// <summary>Caregiver's mobile number, unformatted digits as entered.</summary>
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

        /// <summary>Caregiver's mobile number formatted for display.</summary>
        public string CaregiverMobilePhoneFormatted
        {
            get => PhoneNumberFormatter.Format(CaregiverMobilePhoneNumber, CaregiverMobilePhoneDialCode);
        }

        /// <summary>Caregiver's mobile number with dial code, formatted for reports.</summary>
        public string CaregiverMobilePhoneFull
        {
            get => string.IsNullOrEmpty(CaregiverMobilePhoneNumber)
                ? string.Empty
                : $"{CaregiverMobilePhoneDialCode} {CaregiverMobilePhoneFormatted}";
        }

        // Referral phone
        private string _referralPhoneDialCode = "+1";
        /// <summary>Country dial code for <see cref="ReferralPhoneNumber"/>, e.g. "+1".</summary>
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
        /// <summary>Referral contact's phone number, unformatted digits as entered.</summary>
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

        /// <summary>Referral contact's phone number formatted for display.</summary>
        public string ReferralPhoneFormatted
        {
            get => PhoneNumberFormatter.Format(ReferralPhoneNumber, ReferralPhoneDialCode);
        }

        /// <summary>Referral contact's phone number with dial code, formatted for reports.</summary>
        public string ReferralPhoneFull
        {
            get => string.IsNullOrEmpty(ReferralPhoneNumber)
                ? string.Empty
                : $"{ReferralPhoneDialCode} {ReferralPhoneFormatted}";
        }

        #endregion

        // Core properties with validation

        /// <summary>Required unique identifier for the patient record. Triggers validation on set.</summary>
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

        /// <summary>Required identifier assigned by the hospital. Triggers validation on set.</summary>
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

        /// <summary>Patient's given name.</summary>
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        /// <summary>Required family name. Triggers validation on set.</summary>
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

        /// <summary>Patient's date of birth.</summary>
        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value);
        }

        /// <summary>Patient's recorded gender.</summary>
        public string Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        /// <summary>Gestational age at birth.</summary>
        public string GestationalAge
        {
            get => _gestationalAge;
            set => SetProperty(ref _gestationalAge, value);
        }

        /// <summary>Patient's birth weight.</summary>
        public string Weight
        {
            get => _weight;
            set => SetProperty(ref _weight, value);
        }

        /// <summary>Patient's height/length at birth.</summary>
        public string Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        /// <summary>Facility or location where the patient was born.</summary>
        public string BirthLocation
        {
            get => _birthLocation;
            set => SetProperty(ref _birthLocation, value);
        }

        /// <summary>Patient's nationality.</summary>
        public string Nationality
        {
            get => _nationality;
            set => SetProperty(ref _nationality, value);
        }

        /// <summary>Consent status recorded for hearing screening.</summary>
        public string ScreeningConsent
        {
            get => _screeningConsent;
            set => SetProperty(ref _screeningConsent, value);
        }

        /// <summary>State/region under which consent was obtained.</summary>
        public string ConsentState
        {
            get => _consentState;
            set => SetProperty(ref _consentState, value);
        }

        /// <summary>NICU (Neonatal Intensive Care Unit) status or identifier for the patient.</summary>
        public string NICU
        {
            get => _nicu;
            set => SetProperty(ref _nicu, value);
        }

        /// <summary>Date the patient was discharged, if applicable.</summary>
        public DateTime? Discharged
        {
            get => _discharged;
            set => SetProperty(ref _discharged, value);
        }

        /// <summary>Date of death, if applicable.</summary>
        public DateTime? Deceased
        {
            get => _deceased;
            set => SetProperty(ref _deceased, value);
        }

        /// <summary>Consent status recorded for follow-up tracking.</summary>
        public string TrackingConsent
        {
            get => _trackingConsent;
            set => SetProperty(ref _trackingConsent, value);
        }

        /// <summary>Free-text notes about the patient.</summary>
        public string Comments
        {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }

        // Mother properties

        /// <summary>Mother's title (e.g. Mrs., Ms., Dr.).</summary>
        public string MotherTitle
        {
            get => _motherTitle;
            set => SetProperty(ref _motherTitle, value);
        }

        /// <summary>Mother's social security number.</summary>
        public string MotherSSN
        {
            get => _motherSSN;
            set => SetProperty(ref _motherSSN, value);
        }

        /// <summary>Mother's identifier in the hospital system.</summary>
        public string MotherId
        {
            get => _motherId;
            set => SetProperty(ref _motherId, value);
        }

        /// <summary>Mother's given name.</summary>
        public string MotherFirstName
        {
            get => _motherFirstName;
            set => SetProperty(ref _motherFirstName, value);
        }

        /// <summary>Mother's family name.</summary>
        public string MotherLastName
        {
            get => _motherLastName;
            set => SetProperty(ref _motherLastName, value);
        }

        /// <summary>Mother's date of birth.</summary>
        public DateTime? MotherDateOfBirth
        {
            get => _motherDateOfBirth;
            set => SetProperty(ref _motherDateOfBirth, value);
        }

        /// <summary>Mother's preferred language.</summary>
        public string MotherLanguage
        {
            get => _motherLanguage;
            set => SetProperty(ref _motherLanguage, value);
        }

        /// <summary>Mother's primary address line.</summary>
        public string MotherAddress1
        {
            get => _motherAddress1;
            set => SetProperty(ref _motherAddress1, value);
        }

        /// <summary>Mother's secondary address line (apartment, suite, etc.).</summary>
        public string MotherAddress2
        {
            get => _motherAddress2;
            set => SetProperty(ref _motherAddress2, value);
        }

        /// <summary>Mother's city of residence.</summary>
        public string MotherCity
        {
            get => _motherCity;
            set => SetProperty(ref _motherCity, value);
        }

        /// <summary>Mother's state or province of residence.</summary>
        public string MotherState
        {
            get => _motherState;
            set => SetProperty(ref _motherState, value);
        }

        /// <summary>Mother's postal/zip code.</summary>
        public string MotherZipCode
        {
            get => _motherZipCode;
            set => SetProperty(ref _motherZipCode, value);
        }

        /// <summary>Mother's country of residence.</summary>
        public string MotherCountry
        {
            get => _motherCountry;
            set => SetProperty(ref _motherCountry, value);
        }

        /// <summary>Mother's raw phone number, as stored on the patient record.</summary>
        public string MotherPhone
        {
            get => _motherPhone;
            set => SetProperty(ref _motherPhone, value);
        }

        /// <summary>Mother's raw mobile number, as stored on the patient record.</summary>
        public string MotherMobilePhone
        {
            get => _motherMobilePhone;
            set => SetProperty(ref _motherMobilePhone, value);
        }

        /// <summary>Mother's fax number.</summary>
        public string MotherFax
        {
            get => _motherFax;
            set => SetProperty(ref _motherFax, value);
        }

        /// <summary>Mother's email address.</summary>
        public string MotherEmail
        {
            get => _motherEmail;
            set => SetProperty(ref _motherEmail, value);
        }

        // Caregiver properties

        /// <summary>Caregiver's title (e.g. Mr., Mrs., Dr.).</summary>
        public string CaregiverTitle
        {
            get => _caregiverTitle;
            set => SetProperty(ref _caregiverTitle, value);
        }

        /// <summary>Caregiver's social security number.</summary>
        public string CaregiverSSN
        {
            get => _caregiverSSN;
            set => SetProperty(ref _caregiverSSN, value);
        }

        /// <summary>Caregiver's given name.</summary>
        public string CaregiverFirstName
        {
            get => _caregiverFirstName;
            set => SetProperty(ref _caregiverFirstName, value);
        }

        /// <summary>Caregiver's family name.</summary>
        public string CaregiverLastName
        {
            get => _caregiverLastName;
            set => SetProperty(ref _caregiverLastName, value);
        }

        /// <summary>Caregiver's preferred language.</summary>
        public string CaregiverLanguage
        {
            get => _caregiverLanguage;
            set => SetProperty(ref _caregiverLanguage, value);
        }

        /// <summary>Caregiver's primary address line.</summary>
        public string CaregiverAddress1
        {
            get => _caregiverAddress1;
            set => SetProperty(ref _caregiverAddress1, value);
        }

        /// <summary>Caregiver's secondary address line (apartment, suite, etc.).</summary>
        public string CaregiverAddress2
        {
            get => _caregiverAddress2;
            set => SetProperty(ref _caregiverAddress2, value);
        }

        /// <summary>Caregiver's city of residence.</summary>
        public string CaregiverCity
        {
            get => _caregiverCity;
            set => SetProperty(ref _caregiverCity, value);
        }

        /// <summary>Caregiver's state or province of residence.</summary>
        public string CaregiverState
        {
            get => _caregiverState;
            set => SetProperty(ref _caregiverState, value);
        }

        /// <summary>Caregiver's postal/zip code.</summary>
        public string CaregiverZipCode
        {
            get => _caregiverZipCode;
            set => SetProperty(ref _caregiverZipCode, value);
        }

        /// <summary>Caregiver's country of residence.</summary>
        public string CaregiverCountry
        {
            get => _caregiverCountry;
            set => SetProperty(ref _caregiverCountry, value);
        }

        /// <summary>Caregiver's raw phone number, as stored on the patient record.</summary>
        public string CaregiverPhone
        {
            get => _caregiverPhone;
            set => SetProperty(ref _caregiverPhone, value);
        }

        /// <summary>Caregiver's raw mobile number, as stored on the patient record.</summary>
        public string CaregiverMobilePhone
        {
            get => _caregiverMobilePhone;
            set => SetProperty(ref _caregiverMobilePhone, value);
        }

        /// <summary>Caregiver's fax number.</summary>
        public string CaregiverFax
        {
            get => _caregiverFax;
            set => SetProperty(ref _caregiverFax, value);
        }

        /// <summary>Caregiver's email address.</summary>
        public string CaregiverEmail
        {
            get => _caregiverEmail;
            set => SetProperty(ref _caregiverEmail, value);
        }

        // Referral properties

        /// <summary>Whether the patient was referred for further audiology evaluation.</summary>
        public string AudiologyReferral
        {
            get => _audiologyReferral;
            set => SetProperty(ref _audiologyReferral, value);
        }

        /// <summary>Date the audiology referral was made.</summary>
        public DateTime? ReferralDate
        {
            get => _referralDate;
            set => SetProperty(ref _referralDate, value);
        }

        /// <summary>Provider or facility the patient was referred to.</summary>
        public string ReferralTo
        {
            get => _referralTo;
            set => SetProperty(ref _referralTo, value);
        }

        /// <summary>Provider or facility that initiated the referral.</summary>
        public string ReferralFrom
        {
            get => _referralFrom;
            set => SetProperty(ref _referralFrom, value);
        }

        /// <summary>Referral contact's raw phone number, as stored on the patient record.</summary>
        public string ReferralPhone
        {
            get => _referralPhone;
            set => SetProperty(ref _referralPhone, value);
        }

        // Medical properties

        /// <summary>Medications currently prescribed to the patient.</summary>
        public string Medication
        {
            get => _medication;
            set => SetProperty(ref _medication, value);
        }

        /// <summary>Patient's attending physician.</summary>
        public string Physician
        {
            get => _physician;
            set => SetProperty(ref _physician, value);
        }

        /// <summary>Audiologist assigned to the patient's screening.</summary>
        public string Audiologist
        {
            get => _audiologist;
            set => SetProperty(ref _audiologist, value);
        }

        // Risk factor properties — each setter updates summary counts

        /// <summary>Perinatal risk factor: family history of hearing loss ("Yes"/"No"/"Unknown").</summary>
        public string FamilyHistory
        {
            get => _familyHistory;
            set { if (SetProperty(ref _familyHistory, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Perinatal risk factor: low birth weight ("Yes"/"No"/"Unknown").</summary>
        public string LowBirthWeight
        {
            get => _lowBirthWeight;
            set { if (SetProperty(ref _lowBirthWeight, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Perinatal risk factor: hyperbilirubinemia ("Yes"/"No"/"Unknown").</summary>
        public string Hyperbilirubinemia
        {
            get => _hyperbilirubinemia;
            set { if (SetProperty(ref _hyperbilirubinemia, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Perinatal risk factor: asphyxia ("Yes"/"No"/"Unknown").</summary>
        public string Asphyxia
        {
            get => _asphyxia;
            set { if (SetProperty(ref _asphyxia, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Perinatal risk factor: craniofacial anomalies ("Yes"/"No"/"Unknown").</summary>
        public string CraniofacialAnomalies
        {
            get => _craniofacialAnomalies;
            set { if (SetProperty(ref _craniofacialAnomalies, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Perinatal risk factor: associated syndromes ("Yes"/"No"/"Unknown").</summary>
        public string Syndromes
        {
            get => _syndromes;
            set { if (SetProperty(ref _syndromes, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Perinatal risk factor: in-utero infections ("Yes"/"No"/"Unknown").</summary>
        public string InUteroInfections
        {
            get => _inUteroInfections;
            set { if (SetProperty(ref _inUteroInfections, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Postnatal risk factor: bacterial meningitis ("Yes"/"No"/"Unknown").</summary>
        public string BacterialMeningitis
        {
            get => _bacterialMeningitis;
            set { if (SetProperty(ref _bacterialMeningitis, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Postnatal risk factor: perinatal infection ("Yes"/"No"/"Unknown").</summary>
        public string PerinatalInfection
        {
            get => _perinatalInfection;
            set { if (SetProperty(ref _perinatalInfection, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Postnatal risk factor: ototoxic medication exposure ("Yes"/"No"/"Unknown").</summary>
        public string OtotoxicMedications
        {
            get => _ototoxicMedications;
            set { if (SetProperty(ref _ototoxicMedications, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Postnatal risk factor: aminoglycoside exposure ("Yes"/"No"/"Unknown").</summary>
        public string Aminoglycosides
        {
            get => _aminoglycosides;
            set { if (SetProperty(ref _aminoglycosides, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Postnatal risk factor: prolonged mechanical ventilation ("Yes"/"No"/"Unknown").</summary>
        public string ProlongedVentilation
        {
            get => _prolongedVentilation;
            set { if (SetProperty(ref _prolongedVentilation, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Postnatal risk factor: ECMO (extracorporeal membrane oxygenation) treatment ("Yes"/"No"/"Unknown").</summary>
        public string ECMO
        {
            get => _ecmo;
            set { if (SetProperty(ref _ecmo, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Postnatal risk factor: NICU stay ("Yes"/"No"/"Unknown").</summary>
        public string NICUStay
        {
            get => _nicuStay;
            set { if (SetProperty(ref _nicuStay, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Postnatal risk factor: head trauma ("Yes"/"No"/"Unknown").</summary>
        public string HeadTrauma
        {
            get => _headTrauma;
            set { if (SetProperty(ref _headTrauma, value)) UpdateRiskFactorCounts(); }
        }

        /// <summary>Other risk factor: caregiver-reported concern about hearing ("Yes"/"No"/"Unknown").</summary>
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

        /// <summary>Count of all risk factors answered "Yes".</summary>
        public int TotalYesCount
        {
            get => _totalYesCount;
            private set => SetProperty(ref _totalYesCount, value);
        }

        /// <summary>Count of all risk factors answered "No".</summary>
        public int TotalNoCount
        {
            get => _totalNoCount;
            private set => SetProperty(ref _totalNoCount, value);
        }

        /// <summary>Count of all risk factors left as "Unknown".</summary>
        public int TotalUnknownCount
        {
            get => _totalUnknownCount;
            private set => SetProperty(ref _totalUnknownCount, value);
        }

        /// <summary>Count of all risk factors that have been answered (not "Unknown").</summary>
        public int TotalAnsweredCount
        {
            get => _totalAnsweredCount;
            private set => SetProperty(ref _totalAnsweredCount, value);
        }

        /// <summary>Count of perinatal risk factors answered "Yes".</summary>
        public int PerinatalYesCount
        {
            get => _perinatalYesCount;
            private set => SetProperty(ref _perinatalYesCount, value);
        }

        /// <summary>Count of postnatal risk factors answered "Yes".</summary>
        public int PostnatalYesCount
        {
            get => _postnatalYesCount;
            private set => SetProperty(ref _postnatalYesCount, value);
        }

        /// <summary>Count of other-category risk factors answered "Yes".</summary>
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

        /// <summary>
        /// Returns how many perinatal risk factors have been answered (not "Unknown").
        /// </summary>
        /// <returns>The number of answered perinatal risk factors.</returns>
        public int GetPerinatalAnsweredCount()
        {
            var risks = new[] { FamilyHistory, LowBirthWeight, Hyperbilirubinemia,
                       Asphyxia, CraniofacialAnomalies, Syndromes, InUteroInfections };
            return risks.Count(r => r != "Unknown");
        }

        /// <summary>
        /// Returns how many postnatal risk factors have been answered (not "Unknown").
        /// </summary>
        /// <returns>The number of answered postnatal risk factors.</returns>
        public int GetPostnatalAnsweredCount()
        {
            var risks = new[] { BacterialMeningitis, PerinatalInfection, OtotoxicMedications,
                       Aminoglycosides, ProlongedVentilation, ECMO, NICUStay, HeadTrauma };
            return risks.Count(r => r != "Unknown");
        }

        /// <summary>
        /// Returns whether the other-category risk factor has been answered (not "Unknown").
        /// </summary>
        /// <returns>1 if answered, otherwise 0.</returns>
        public int GetOtherAnsweredCount()
        {
            return CaregiverConcernRisk != "Unknown" ? 1 : 0;
        }

        // Screening results

        /// <summary>Screening result recorded for the left ear.</summary>
        public string LeftEarResult
        {
            get => _leftEarResult;
            set => SetProperty(ref _leftEarResult, value);
        }

        /// <summary>Screening result recorded for the right ear.</summary>
        public string RightEarResult
        {
            get => _rightEarResult;
            set => SetProperty(ref _rightEarResult, value);
        }

        // Required fields tracking

        private int _requiredFieldsCount;
        /// <summary>Number of required fields currently left empty.</summary>
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

        /// <summary>True when one or more required fields are currently empty.</summary>
        public bool HasRequiredFieldsEmpty => RequiredFieldsCount > 0;

        /// <summary>
        /// Returns the display names of required fields that are currently empty.
        /// </summary>
        /// <returns>A list of missing required field names.</returns>
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

        /// <summary>Entity-level error message. Always <see langword="null"/> — validation is per-property.</summary>
        public string Error => null;

        /// <summary>Gets the validation error message for the property named <paramref name="propertyName"/>, or <see langword="null"/> if it is valid.</summary>
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