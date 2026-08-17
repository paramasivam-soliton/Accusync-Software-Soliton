// --------------------------------------------------------------------------------
// <copyright file="PatientMapper.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AccuSync.Application.Abstractions.Parsing;
using AccuSync.Core.Entities;
using AccuSync.Presentation.ViewModels;
using CoreEntities = AccuSync.Core.Entities.Patients;
using PatientListRow = AccuSync.Application.Models.Patient;

namespace AccuSync.Presentation.Mapping
{
    /// <summary>
    /// Every conversion between the <see cref="CoreEntities.Patient"/> EF entity and the three
    /// pre-existing UI-facing Patient shapes, held in one place — replacing the scattered inline
    /// object-initializers this story removes from <c>PatientsView</c>. No mapping library is
    /// introduced; see the HLD's "Alternative implementations" for why.
    ///
    /// Fields with no corresponding column in Databases/PatientDatabase.sql today (e.g.
    /// <see cref="PatientListRow.BirthTime"/>, <c>PatientViewModel.Physician</c>,
    /// <c>PatientViewModel.AudiologyReferral</c>) are left unmapped rather than guessed at.
    /// <see cref="PatientViewModel.Deceased"/> (date of death) and
    /// <see cref="CoreEntities.Patient.Deceased"/> (yes/no flag) are a genuine type mismatch,
    /// also left unmapped rather than silently coerced.
    /// </summary>
    public static class PatientMapper
    {
        // ---- Entity -> lightweight list row ----------------------------------------------

        public static PatientListRow ToListRow(this CoreEntities.Patient entity)
        {
            var patientContact = entity.Contacts.FirstOrDefault(c => c.ContactType == CoreEntities.PatientContactType.Patient);
            var (leftEarResult, rightEarResult, dateOfScreen) = LatestScreeningSummary(entity);

            return new PatientListRow
            {
                PatientId = entity.PatientId.ToString(),
                HospitalId = entity.HospitalId,
                FirstName = patientContact?.Forename1 ?? string.Empty,
                LastName = patientContact?.Surname ?? string.Empty,
                BirthDate = patientContact?.DateOfBirth ?? DateTime.MinValue,
                Gender = patientContact?.Gender,
                Title = patientContact?.Title,
                BirthLocation = patientContact?.BirthLocation,
                Weight = patientContact?.Weight?.ToString(),
                Height = patientContact?.Height?.ToString(),
                GestationalAge = entity.GestationalAge?.ToString(),
                NICU = entity.NicuStatus,
                ScreeningConsent = entity.ScreeningConsent,
                TrackingConsent = entity.TrackingConsent,
                LeftEarResult = leftEarResult,
                RightEarResult = rightEarResult,
                DateOfScreen = dateOfScreen,
                Tests = entity.TestSessions
                    .SelectMany(s => s.TestRecords)
                    .Select(ToDisplayTestRecord)
                    .ToList()
            };
        }

        // ---- Entity -> detail/edit form view model ----------------------------------------

        public static PatientViewModel ToViewModel(this CoreEntities.Patient entity, IQrCodeGenerator qrGenerator)
        {
            var patientContact = entity.Contacts.FirstOrDefault(c => c.ContactType == CoreEntities.PatientContactType.Patient);
            var mother = entity.Contacts.FirstOrDefault(c => c.ContactType == CoreEntities.PatientContactType.Mother);
            var caregiver = entity.Contacts.FirstOrDefault(c => c.ContactType == CoreEntities.PatientContactType.Caregiver);
            var riskFactors = DeserializeRiskFactors(entity.PatientRiskFactors);
            var (leftEarResult, rightEarResult, _) = LatestScreeningSummary(entity);

            var viewModel = new PatientViewModel(qrGenerator)
            {
                PatientId = entity.PatientId.ToString(),
                HospitalId = entity.HospitalId,
                FirstName = patientContact?.Forename1,
                LastName = patientContact?.Surname,
                DateOfBirth = patientContact?.DateOfBirth,
                Gender = patientContact?.Gender,
                GestationalAge = entity.GestationalAge?.ToString(),
                Weight = patientContact?.Weight?.ToString(),
                Height = patientContact?.Height?.ToString(),
                BirthLocation = patientContact?.BirthLocation,
                Nationality = patientContact?.NationalityCode,
                ScreeningConsent = entity.ScreeningConsent,
                ConsentState = entity.ConsentState,
                NICU = entity.NicuStatus,
                Discharged = entity.Discharged,
                TrackingConsent = entity.TrackingConsent,
                Comments = entity.PredefinedComments,

                MotherTitle = mother?.Title,
                MotherSSN = mother?.SocialSecurityNumber,
                MotherId = mother?.IdNumber,
                MotherFirstName = mother?.Forename1,
                MotherLastName = mother?.Surname,
                MotherDateOfBirth = mother?.DateOfBirth,
                MotherLanguage = mother?.LanguageCode,
                MotherAddress1 = mother?.Address1,
                MotherCity = mother?.City,
                MotherState = mother?.State,
                MotherZipCode = mother?.Zip,
                MotherCountry = mother?.Country,
                MotherPhone = mother?.Phone,
                MotherMobilePhone = mother?.CellPhone,
                MotherFax = mother?.Fax,
                MotherEmail = mother?.Email,

                CaregiverTitle = caregiver?.Title,
                CaregiverSSN = caregiver?.SocialSecurityNumber,
                CaregiverFirstName = caregiver?.Forename1,
                CaregiverLastName = caregiver?.Surname,
                CaregiverLanguage = caregiver?.LanguageCode,
                CaregiverAddress1 = caregiver?.Address1,
                CaregiverCity = caregiver?.City,
                CaregiverState = caregiver?.State,
                CaregiverZipCode = caregiver?.Zip,
                CaregiverCountry = caregiver?.Country,
                CaregiverPhone = caregiver?.Phone,
                CaregiverMobilePhone = caregiver?.CellPhone,
                CaregiverFax = caregiver?.Fax,
                CaregiverEmail = caregiver?.Email,

                ReferralTo = entity.ReferralTo,
                ReferralFrom = entity.ReferralFrom,
                ReferralPhone = entity.ReferralPhone,

                Medication = entity.Medication,

                FamilyHistory = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.FamilyHistory)),
                LowBirthWeight = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.LowBirthWeight)),
                Hyperbilirubinemia = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.Hyperbilirubinemia)),
                Asphyxia = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.Asphyxia)),
                CraniofacialAnomalies = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.CraniofacialAnomalies)),
                Syndromes = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.Syndromes)),
                InUteroInfections = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.InUteroInfections)),
                BacterialMeningitis = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.BacterialMeningitis)),
                PerinatalInfection = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.PerinatalInfection)),
                OtotoxicMedications = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.OtotoxicMedications)),
                Aminoglycosides = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.Aminoglycosides)),
                ProlongedVentilation = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.ProlongedVentilation)),
                ECMO = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.ECMO)),
                NICUStay = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.NICUStay)),
                HeadTrauma = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.HeadTrauma)),
                CaregiverConcernRisk = RiskFactorOrUnknown(riskFactors, nameof(PatientViewModel.CaregiverConcernRisk)),

                LeftEarResult = leftEarResult,
                RightEarResult = rightEarResult
            };

            // Snapshots the values just set above as the clean baseline, so the detail panel
            // doesn't open already marked dirty.
            viewModel.BeginEdit();
            return viewModel;
        }

        // ---- Edited view model -> write changes back onto the entity ----------------------

        /// <summary>
        /// Writes the view model's editable fields back onto <paramref name="entity"/>, including
        /// its Patient/Mother/Caregiver contact rows. Does not touch <c>PatientId</c>,
        /// <c>CreatedAt</c>, or <c>ModifiedAt</c>. Note: persisting the resulting Contacts changes
        /// requires a repository method that saves child rows — <c>IPatientRepository.UpdateAsync</c>
        /// in this story only persists the Patient row's own scalar fields (see its remarks); wiring
        /// contact edits through to the database is left for the edit/save UI story that uses this.
        /// </summary>
        public static void ApplyTo(this PatientViewModel viewModel, CoreEntities.Patient entity)
        {
            entity.HospitalId = viewModel.HospitalId;
            entity.GestationalAge = int.TryParse(viewModel.GestationalAge, out int weeks) ? weeks : null;
            entity.NicuStatus = viewModel.NICU;
            entity.ScreeningConsent = viewModel.ScreeningConsent;
            entity.ConsentState = viewModel.ConsentState;
            entity.Discharged = viewModel.Discharged;
            entity.TrackingConsent = viewModel.TrackingConsent;
            entity.PredefinedComments = viewModel.Comments;
            entity.ReferralTo = viewModel.ReferralTo;
            entity.ReferralFrom = viewModel.ReferralFrom;
            entity.ReferralPhone = viewModel.ReferralPhone;
            entity.Medication = viewModel.Medication;

            entity.PatientRiskFactors = SerializeRiskFactors(viewModel);

            UpsertContact(entity, CoreEntities.PatientContactType.Patient, c =>
            {
                c.Forename1 = viewModel.FirstName;
                c.Surname = viewModel.LastName;
                c.DateOfBirth = viewModel.DateOfBirth;
                c.Gender = viewModel.Gender;
                c.BirthLocation = viewModel.BirthLocation;
                c.NationalityCode = viewModel.Nationality;
                double.TryParse(viewModel.Weight, out double weight);
                c.Weight = weight == 0 ? null : weight;
                double.TryParse(viewModel.Height, out double height);
                c.Height = height == 0 ? null : height;
            });

            UpsertContact(entity, CoreEntities.PatientContactType.Mother, c =>
            {
                c.Title = viewModel.MotherTitle;
                c.SocialSecurityNumber = viewModel.MotherSSN;
                c.IdNumber = viewModel.MotherId;
                c.Forename1 = viewModel.MotherFirstName;
                c.Surname = viewModel.MotherLastName;
                c.DateOfBirth = viewModel.MotherDateOfBirth;
                c.LanguageCode = viewModel.MotherLanguage;
                c.Address1 = viewModel.MotherAddress1;
                c.City = viewModel.MotherCity;
                c.State = viewModel.MotherState;
                c.Zip = viewModel.MotherZipCode;
                c.Country = viewModel.MotherCountry;
                c.Phone = viewModel.MotherPhone;
                c.CellPhone = viewModel.MotherMobilePhone;
                c.Fax = viewModel.MotherFax;
                c.Email = viewModel.MotherEmail;
            });

            UpsertContact(entity, CoreEntities.PatientContactType.Caregiver, c =>
            {
                c.Title = viewModel.CaregiverTitle;
                c.SocialSecurityNumber = viewModel.CaregiverSSN;
                c.Forename1 = viewModel.CaregiverFirstName;
                c.Surname = viewModel.CaregiverLastName;
                c.LanguageCode = viewModel.CaregiverLanguage;
                c.Address1 = viewModel.CaregiverAddress1;
                c.City = viewModel.CaregiverCity;
                c.State = viewModel.CaregiverState;
                c.Zip = viewModel.CaregiverZipCode;
                c.Country = viewModel.CaregiverCountry;
                c.Phone = viewModel.CaregiverPhone;
                c.CellPhone = viewModel.CaregiverMobilePhone;
                c.Fax = viewModel.CaregiverFax;
                c.Email = viewModel.CaregiverEmail;
            });
        }

        // ---- Import DTO -> new entity -------------------------------------------------------

        public static CoreEntities.Patient ToEntity(this PatientData importData)
        {
            var entity = new CoreEntities.Patient
            {
                SourceId = importData.SourceId,
                HospitalId = importData.HospitalId,
                GestationalAge = int.TryParse(importData.GestationalAge, out int weeks) ? weeks : null,
                ScreeningConsent = importData.ScreeningConsent,
                ConsentState = importData.ConsentState,
                NicuStatus = importData.NICU,
                TrackingConsent = importData.TrackingConsent,
                PredefinedComments = importData.Comments,
                ReferralFrom = importData.ReferralFrom,
                ReferralTo = importData.ReferralTo,
                ReferralPhone = importData.ReferralPhone,
                Medication = importData.Medication,
                PatientRiskFactors = JsonSerializer.Serialize(importData.RiskFactors)
            };

            entity.Contacts.Add(new CoreEntities.PatientContact
            {
                ContactType = CoreEntities.PatientContactType.Patient,
                Forename1 = importData.FirstName,
                Surname = importData.LastName,
                Gender = importData.Gender,
                BirthLocation = importData.BirthLocation,
                NationalityCode = importData.Nationality,
                Weight = double.TryParse(importData.Weight, out double weight) ? weight : null,
                Height = double.TryParse(importData.Height, out double height) ? height : null
            });

            if (!string.IsNullOrWhiteSpace(importData.MotherFirstName) || !string.IsNullOrWhiteSpace(importData.MotherLastName))
            {
                entity.Contacts.Add(new CoreEntities.PatientContact
                {
                    ContactType = CoreEntities.PatientContactType.Mother,
                    Title = importData.MotherTitle,
                    SocialSecurityNumber = importData.MotherSSN,
                    IdNumber = importData.MotherId,
                    Forename1 = importData.MotherFirstName,
                    Surname = importData.MotherLastName,
                    LanguageCode = importData.MotherLanguage,
                    Address1 = importData.MotherAddress1,
                    City = importData.MotherCity,
                    State = importData.MotherState,
                    Zip = importData.MotherZip,
                    Country = importData.MotherCountry,
                    Phone = importData.MotherPhone,
                    CellPhone = importData.MotherMobile,
                    Fax = importData.MotherFax,
                    Email = importData.MotherEmail
                });
            }

            if (!string.IsNullOrWhiteSpace(importData.CaregiverFirstName) || !string.IsNullOrWhiteSpace(importData.CaregiverLastName))
            {
                entity.Contacts.Add(new CoreEntities.PatientContact
                {
                    ContactType = CoreEntities.PatientContactType.Caregiver,
                    Title = importData.CaregiverTitle,
                    SocialSecurityNumber = importData.CaregiverSSN,
                    Forename1 = importData.CaregiverFirstName,
                    Surname = importData.CaregiverLastName,
                    LanguageCode = importData.CaregiverLanguage,
                    Address1 = importData.CaregiverAddress1,
                    City = importData.CaregiverCity,
                    State = importData.CaregiverState,
                    Zip = importData.CaregiverZip,
                    Country = importData.CaregiverCountry,
                    Phone = importData.CaregiverPhone,
                    CellPhone = importData.CaregiverMobile,
                    Fax = importData.CaregiverFax,
                    Email = importData.CaregiverEmail
                });
            }

            return entity;
        }

        // ---- Helpers -------------------------------------------------------------------------

        private static void UpsertContact(CoreEntities.Patient entity, string contactType, Action<CoreEntities.PatientContact> apply)
        {
            var contact = entity.Contacts.FirstOrDefault(c => c.ContactType == contactType);
            if (contact == null)
            {
                contact = new CoreEntities.PatientContact { ContactType = contactType, PatientId = entity.PatientId };
                entity.Contacts.Add(contact);
            }

            apply(contact);
        }

        /// <summary>
        /// TestFacility/TestLocation/Examiner have no source column in the new TestRecord
        /// schema yet — left blank rather than guessed at.
        /// </summary>
        private static TestRecord ToDisplayTestRecord(CoreEntities.TestRecord entity)
        {
            return new TestRecord
            {
                Id = entity.TestRecordId.ToString(),
                TestType = entity.TestType,
                Ear = entity.TestObject,
                TestResult = entity.TestResult,
                TestDate = entity.TestDate,
                DurationMs = entity.Duration ?? 0,
                DeviceSerial = entity.InstrumentSerial,
                DeviceName = entity.InstrumentName,
                ProbeSerial = entity.TransducerSerial,
                ProbeType = entity.TransducerTypeName,
                ProbeCalibrationDate = entity.TransducerCalDate,
                ProbeNextCalibrationDate = entity.TransducerNextCalDate
            };
        }

        /// <summary>
        /// Left/right ear results and overall screen date, derived from the most recent test
        /// record per ear across all of the patient's sessions.
        /// </summary>
        private static (string? LeftEarResult, string? RightEarResult, DateTime? DateOfScreen) LatestScreeningSummary(CoreEntities.Patient entity)
        {
            var allRecords = entity.TestSessions.SelectMany(s => s.TestRecords).ToList();

            string? left = allRecords.Where(r => r.TestObject == "Left Ear")
                .OrderByDescending(r => r.TestDate).FirstOrDefault()?.TestResult;
            string? right = allRecords.Where(r => r.TestObject == "Right Ear")
                .OrderByDescending(r => r.TestDate).FirstOrDefault()?.TestResult;
            DateTime? dateOfScreen = entity.TestSessions.Select(s => (DateTime?)s.SessionDate).OrderByDescending(d => d).FirstOrDefault();

            return (left, right, dateOfScreen);
        }

        private static string RiskFactorOrUnknown(Dictionary<string, string> riskFactors, string key)
        {
            return riskFactors.TryGetValue(key, out string? value) ? value : "Unknown";
        }

        /// <summary>
        /// Risk factors are stored as a JSON object (property name -&gt; "Yes"/"No"/"Unknown")
        /// inside the existing <see cref="CoreEntities.Patient.PatientRiskFactors"/> text column —
        /// no new table, per this story's descoped decision. A dictionary (rather than the
        /// column's originally-documented "array of codes" shape) is what actually preserves the
        /// tri-state values <see cref="PatientViewModel"/> already has; the known gap this doesn't
        /// solve is described in the HLD's Open Issues.
        /// </summary>
        private static Dictionary<string, string> DeserializeRiskFactors(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, string>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                return new Dictionary<string, string>();
            }
        }

        private static string SerializeRiskFactors(PatientViewModel viewModel)
        {
            var riskFactors = new Dictionary<string, string>
            {
                [nameof(PatientViewModel.FamilyHistory)] = viewModel.FamilyHistory,
                [nameof(PatientViewModel.LowBirthWeight)] = viewModel.LowBirthWeight,
                [nameof(PatientViewModel.Hyperbilirubinemia)] = viewModel.Hyperbilirubinemia,
                [nameof(PatientViewModel.Asphyxia)] = viewModel.Asphyxia,
                [nameof(PatientViewModel.CraniofacialAnomalies)] = viewModel.CraniofacialAnomalies,
                [nameof(PatientViewModel.Syndromes)] = viewModel.Syndromes,
                [nameof(PatientViewModel.InUteroInfections)] = viewModel.InUteroInfections,
                [nameof(PatientViewModel.BacterialMeningitis)] = viewModel.BacterialMeningitis,
                [nameof(PatientViewModel.PerinatalInfection)] = viewModel.PerinatalInfection,
                [nameof(PatientViewModel.OtotoxicMedications)] = viewModel.OtotoxicMedications,
                [nameof(PatientViewModel.Aminoglycosides)] = viewModel.Aminoglycosides,
                [nameof(PatientViewModel.ProlongedVentilation)] = viewModel.ProlongedVentilation,
                [nameof(PatientViewModel.ECMO)] = viewModel.ECMO,
                [nameof(PatientViewModel.NICUStay)] = viewModel.NICUStay,
                [nameof(PatientViewModel.HeadTrauma)] = viewModel.HeadTrauma,
                [nameof(PatientViewModel.CaregiverConcernRisk)] = viewModel.CaregiverConcernRisk
            };

            return JsonSerializer.Serialize(riskFactors);
        }
    }
}
