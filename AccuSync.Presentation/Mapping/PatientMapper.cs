// --------------------------------------------------------------------------------
// <copyright file="PatientMapper.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using AccuSync.Application.Abstractions.Parsing;
using AccuSync.Core.Entities;
using AccuSync.Presentation.ViewModels;
using CoreEntities = AccuSync.Core.Entities.Patients;

namespace AccuSync.Presentation.Mapping
{
    /// <summary>
    /// The single place that converts between the Patient persistence entity
    /// (<see cref="CoreEntities.Patient"/>) and each UI-facing representation:
    /// <see cref="AccuSync.Application.Models.Patient"/> for the list grid,
    /// <see cref="PatientViewModel"/> for the detail/edit panel, and
    /// <see cref="PatientData"/> for the import pipeline. Nothing outside this class should
    /// construct one of those UI-facing types from a persistence entity by hand.
    /// </summary>
    public static class PatientMapper
    {
        private const string PatientContactType = "Patient";
        private const string MotherContactType = "Mother";
        private const string CaregiverContactType = "Caregiver";

        public static AccuSync.Application.Models.Patient ToListRow(this CoreEntities.Patient entity)
        {
            var contact = FindContact(entity, PatientContactType);
            var latestSession = entity.TestSessions?.OrderByDescending(s => s.SessionDate).FirstOrDefault();

            return new AccuSync.Application.Models.Patient
            {
                PatientId = entity.PatientId.ToString(),
                HospitalId = entity.HospitalId,
                FirstName = contact?.Forename1,
                LastName = contact?.Surname,
                BirthDate = contact?.DateOfBirth ?? DateTime.MinValue,
                Gender = contact?.Gender,
                BirthLocation = contact?.BirthLocation,
                GestationalAge = entity.GestationalAge?.ToString(),
                NICU = entity.NicuStatus,
                ScreeningConsent = entity.ScreeningConsent,
                TrackingConsent = entity.TrackingConsent,
                DateOfScreen = latestSession?.SessionDate,
                Tests = (entity.TestSessions ?? new List<CoreEntities.TestSession>())
                    .SelectMany(s => s.TestRecords ?? new List<CoreEntities.TestRecord>())
                    .Select(ToDisplayTestRecord)
                    .ToList()
            };
        }

        public static PatientViewModel ToViewModel(this CoreEntities.Patient entity, IQrCodeGenerator qrCodeGenerator)
        {
            var patientContact = FindContact(entity, PatientContactType);
            var motherContact = FindContact(entity, MotherContactType);
            var caregiverContact = FindContact(entity, CaregiverContactType);

            var viewModel = new PatientViewModel(qrCodeGenerator)
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
                // Deceased is a DateTime? on the view model (date of death) but a bool? flag on
                // the entity — different representations that don't have a clean conversion.
                // Left unmapped until the view model's shape is reconciled with the schema.
                TrackingConsent = entity.TrackingConsent,
                Comments = entity.PredefinedComments,

                MotherTitle = motherContact?.Title,
                MotherSSN = motherContact?.SocialSecurityNumber,
                MotherFirstName = motherContact?.Forename1,
                MotherLastName = motherContact?.Surname,
                MotherDateOfBirth = motherContact?.DateOfBirth,
                MotherLanguage = motherContact?.LanguageCode,
                MotherAddress1 = motherContact?.Address1,
                MotherCity = motherContact?.City,
                MotherState = motherContact?.State,
                MotherZipCode = motherContact?.Zip,
                MotherCountry = motherContact?.Country,
                MotherPhone = motherContact?.Phone,
                MotherMobilePhone = motherContact?.CellPhone,
                MotherFax = motherContact?.Fax,
                MotherEmail = motherContact?.Email,

                CaregiverTitle = caregiverContact?.Title,
                CaregiverSSN = caregiverContact?.SocialSecurityNumber,
                CaregiverFirstName = caregiverContact?.Forename1,
                CaregiverLastName = caregiverContact?.Surname,
                CaregiverLanguage = caregiverContact?.LanguageCode,
                CaregiverAddress1 = caregiverContact?.Address1,
                CaregiverCity = caregiverContact?.City,
                CaregiverState = caregiverContact?.State,
                CaregiverZipCode = caregiverContact?.Zip,
                CaregiverCountry = caregiverContact?.Country,
                CaregiverPhone = caregiverContact?.Phone,
                CaregiverMobilePhone = caregiverContact?.CellPhone,
                CaregiverFax = caregiverContact?.Fax,
                CaregiverEmail = caregiverContact?.Email,

                ReferralFrom = entity.ReferralFrom,
                ReferralTo = entity.ReferralTo,
                ReferralPhone = entity.ReferralPhone,
                Medication = entity.Medication
            };

            viewModel.BeginEdit();
            return viewModel;
        }

        /// <summary>
        /// Writes editable view model fields back onto the entity, ready to save. Does not
        /// touch PatientId/CreatedAt/ModifiedAt — those are owned by the database/interceptor,
        /// never overwritten from the UI.
        /// </summary>
        public static void ApplyTo(this PatientViewModel viewModel, CoreEntities.Patient entity)
        {
            entity.HospitalId = viewModel.HospitalId;
            entity.ScreeningConsent = viewModel.ScreeningConsent;
            entity.ConsentState = viewModel.ConsentState;
            entity.NicuStatus = viewModel.NICU;
            entity.Discharged = viewModel.Discharged;
            entity.TrackingConsent = viewModel.TrackingConsent;
            entity.PredefinedComments = viewModel.Comments;
            entity.ReferralFrom = viewModel.ReferralFrom;
            entity.ReferralTo = viewModel.ReferralTo;
            entity.ReferralPhone = viewModel.ReferralPhone;
            entity.Medication = viewModel.Medication;

            if (int.TryParse(viewModel.GestationalAge, out int weeks))
            {
                entity.GestationalAge = weeks;
            }

            var patientContact = FindContact(entity, PatientContactType);
            if (patientContact != null)
            {
                patientContact.Forename1 = viewModel.FirstName;
                patientContact.Surname = viewModel.LastName;
                patientContact.DateOfBirth = viewModel.DateOfBirth;
                patientContact.Gender = viewModel.Gender;
                patientContact.BirthLocation = viewModel.BirthLocation;
                patientContact.NationalityCode = viewModel.Nationality;
            }
        }

        /// <summary>
        /// Builds a new entity, including its "Patient"-type contact row, from an import DTO.
        /// </summary>
        public static CoreEntities.Patient ToEntity(this PatientData importData)
        {
            var entity = new CoreEntities.Patient
            {
                SourceId = importData.SourceId,
                HospitalId = importData.HospitalId,
                PatientRecordNumber = importData.PatientId,
                ScreeningConsent = importData.ScreeningConsent,
                ConsentState = importData.ConsentState,
                NicuStatus = importData.NICU,
                Discharged = DateTime.TryParse(importData.Discharged, out var discharged) ? discharged : (DateTime?)null,
                TrackingConsent = importData.TrackingConsent,
                PredefinedComments = importData.Comments,
                ReferralFrom = importData.ReferralFrom,
                ReferralTo = importData.ReferralTo,
                ReferralPhone = importData.ReferralPhone,
                Medication = importData.Medication,
                SourceCreatedAt = importData.SourceCreatedAt,
                SourceModifiedAt = importData.SourceModifiedAt
            };

            if (int.TryParse(importData.GestationalAge, out int weeks))
            {
                entity.GestationalAge = weeks;
            }

            entity.Contacts.Add(new CoreEntities.PatientContact
            {
                ContactType = PatientContactType,
                Forename1 = importData.FirstName,
                Surname = importData.LastName,
                DateOfBirth = DateTime.TryParse(importData.DateOfBirth, out var dob) ? dob : (DateTime?)null,
                Gender = importData.Gender,
                BirthLocation = importData.BirthLocation,
                NationalityCode = importData.Nationality
            });

            if (!string.IsNullOrWhiteSpace(importData.MotherFirstName) || !string.IsNullOrWhiteSpace(importData.MotherLastName))
            {
                entity.Contacts.Add(new CoreEntities.PatientContact
                {
                    ContactType = MotherContactType,
                    Title = importData.MotherTitle,
                    SocialSecurityNumber = importData.MotherSSN,
                    Forename1 = importData.MotherFirstName,
                    Surname = importData.MotherLastName,
                    DateOfBirth = DateTime.TryParse(importData.MotherDOB, out var motherDob) ? motherDob : (DateTime?)null,
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
                    ContactType = CaregiverContactType,
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

        private static CoreEntities.PatientContact FindContact(CoreEntities.Patient entity, string contactType)
        {
            return entity.Contacts?.FirstOrDefault(c => c.ContactType == contactType);
        }

        /// <summary>
        /// Converts a persisted test record to the lightweight display shape
        /// (<see cref="AccuSync.Core.Entities.TestRecord"/>) that <c>TestResultsPanel</c> and
        /// the patient list already bind to. TestFacility/TestLocation/Examiner have no
        /// direct source on the persistence entity (it only stores source system reference
        /// GUIDs, not resolved names) and are left blank until that resolution exists.
        /// </summary>
        private static AccuSync.Core.Entities.TestRecord ToDisplayTestRecord(CoreEntities.TestRecord record)
        {
            return new AccuSync.Core.Entities.TestRecord
            {
                Id = record.TestRecordId.ToString(),
                TestType = record.TestType,
                Ear = record.TestObject,
                TestResult = record.TestResult,
                TestDate = record.TestDate,
                DurationMs = record.Duration ?? 0,
                DeviceSerial = record.InstrumentSerial,
                DeviceName = record.InstrumentName,
                ProbeSerial = record.TransducerSerial,
                ProbeType = record.TransducerTypeName,
                ProbeCalibrationDate = record.TransducerCalDate,
                ProbeNextCalibrationDate = record.TransducerNextCalDate
            };
        }
    }
}
