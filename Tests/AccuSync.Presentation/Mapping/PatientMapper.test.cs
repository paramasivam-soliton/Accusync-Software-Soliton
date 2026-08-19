// --------------------------------------------------------------------------------
// <copyright file="PatientMapper.test.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Abstractions.Parsing;
using AccuSync.Core.Entities;
using AccuSync.Presentation.Mapping;
using AccuSync.Presentation.ViewModels;
using Moq;
using CoreEntities = AccuSync.Core.Entities.Patients;

namespace AccuSync.Presentation.Tests.Mapping
{
    /// <summary>
    /// Covers the mapping layer's risk-factor tri-state round trip — the specific gap
    /// (Yes/No/Unknown collapsing into "flagged or not") that the old JSON-array-of-codes
    /// design couldn't represent. See the HLD's Open Issues for the residual limitation this
    /// dictionary-shaped JSON still doesn't solve (schema-level validation).
    /// </summary>
    public class PatientMapperTests
    {
        private static IQrCodeGenerator QrGenerator => Mock.Of<IQrCodeGenerator>();

        [Fact]
        public void GivenAnEntityWithDistinctRiskFactorValues_WhenMappedToViewModel_ThenEachFactorKeepsItsOwnValue()
        {
            // Arrange
            var entity = new CoreEntities.Patient
            {
                PatientRiskFactors = "{\"FamilyHistory\":\"Yes\",\"NICUStay\":\"No\",\"Asphyxia\":\"Unknown\"}"
            };

            // Act
            var viewModel = entity.ToViewModel(QrGenerator);

            // Assert — all three distinct values survive, not collapsed to one.
            Assert.Equal("Yes", viewModel.FamilyHistory);
            Assert.Equal("No", viewModel.NICUStay);
            Assert.Equal("Unknown", viewModel.Asphyxia);
        }

        [Fact]
        public void GivenAnEntityWithNoStoredRiskFactors_WhenMappedToViewModel_ThenEveryFactorDefaultsToUnknown()
        {
            // Arrange
            var entity = new CoreEntities.Patient { PatientRiskFactors = null };

            // Act
            var viewModel = entity.ToViewModel(QrGenerator);

            // Assert
            Assert.Equal("Unknown", viewModel.FamilyHistory);
            Assert.Equal("Unknown", viewModel.HeadTrauma);
        }

        [Fact]
        public void GivenAViewModelWithDistinctRiskFactorValues_WhenAppliedToAnEntity_ThenAllThreeStatesRoundTripThroughStorage()
        {
            // Arrange
            var entity = new CoreEntities.Patient();
            var viewModel = entity.ToViewModel(QrGenerator);
            viewModel.FamilyHistory = "Yes";
            viewModel.NICUStay = "No";
            viewModel.Asphyxia = "Unknown";

            // Act — write back onto the entity, then re-read through the same deserialization
            // path ToViewModel uses, confirming the stored JSON actually preserves all three states.
            viewModel.ApplyTo(entity);
            var roundTripped = entity.ToViewModel(QrGenerator);

            // Assert
            Assert.Equal("Yes", roundTripped.FamilyHistory);
            Assert.Equal("No", roundTripped.NICUStay);
            Assert.Equal("Unknown", roundTripped.Asphyxia);
        }

        [Fact]
        public void GivenAnEntityWithAPatientContact_WhenMappedToListRow_ThenNameAndBirthDateComeFromTheContactNotThePatientRow()
        {
            // Arrange — name/DOB live on PatientContacts (ContactType = "Patient"), not Patients.
            var entity = new CoreEntities.Patient
            {
                PatientId = 42,
                HospitalId = "HOSP-100",
                Contacts = new List<CoreEntities.PatientContact>
                {
                    new() { ContactType = "Patient", Forename1 = "Ada", Surname = "Lovelace", DateOfBirth = new DateTime(2026, 1, 1) }
                }
            };

            // Act
            var listRow = entity.ToListRow();

            // Assert
            Assert.Equal("42", listRow.PatientId);
            Assert.Equal("Ada", listRow.FirstName);
            Assert.Equal("Lovelace", listRow.LastName);
            Assert.Equal(new DateTime(2026, 1, 1), listRow.BirthDate);
        }

        [Fact]
        public void GivenAnEntityWithNoMotherOrCaregiverContact_WhenMappedToViewModel_ThenTheirFieldsAreEmptyNotThrowing()
        {
            // Arrange — only a Patient contact exists; no Mother/Caregiver row at all.
            var entity = new CoreEntities.Patient
            {
                Contacts = new List<CoreEntities.PatientContact>
                {
                    new() { ContactType = "Patient", Forename1 = "Ada", Surname = "Lovelace" }
                }
            };

            // Act
            var viewModel = entity.ToViewModel(QrGenerator);

            // Assert
            Assert.Null(viewModel.MotherFirstName);
            Assert.Null(viewModel.MotherPhone);
            Assert.Null(viewModel.CaregiverFirstName);
            Assert.Null(viewModel.CaregiverPhone);
        }

        [Fact]
        public void GivenAnEntityWithNoContactsAtAll_WhenMappedToViewModel_ThenPatientFieldsAreEmptyNotThrowing()
        {
            // Arrange
            var entity = new CoreEntities.Patient();

            // Act
            var viewModel = entity.ToViewModel(QrGenerator);

            // Assert
            Assert.Null(viewModel.FirstName);
            Assert.Null(viewModel.LastName);
            Assert.Null(viewModel.DateOfBirth);
        }

        [Fact]
        public void GivenAViewModelWithNoMotherOrCaregiverNameEntered_WhenAppliedToANewEntity_ThenNoMotherOrCaregiverContactIsCreated()
        {
            // Arrange — a brand-new entity with only the Patient contact created by ToViewModel;
            // the user never entered a Mother or Caregiver name.
            var entity = new CoreEntities.Patient();
            var viewModel = entity.ToViewModel(QrGenerator);
            viewModel.FirstName = "Ada";
            viewModel.LastName = "Lovelace";

            // Act
            viewModel.ApplyTo(entity);

            // Assert
            Assert.DoesNotContain(entity.Contacts, c => c.ContactType == CoreEntities.PatientContactType.Mother);
            Assert.DoesNotContain(entity.Contacts, c => c.ContactType == CoreEntities.PatientContactType.Caregiver);
        }

        [Fact]
        public void GivenAViewModelWithAMotherNameEntered_WhenAppliedToANewEntity_ThenAMotherContactIsCreatedWithHerData()
        {
            // Arrange
            var entity = new CoreEntities.Patient();
            var viewModel = entity.ToViewModel(QrGenerator);
            viewModel.MotherFirstName = "Jill";
            viewModel.MotherLastName = "Baker";
            viewModel.MotherPhone = "555-0100";

            // Act
            viewModel.ApplyTo(entity);

            // Assert
            var mother = Assert.Single(entity.Contacts, c => c.ContactType == CoreEntities.PatientContactType.Mother);
            Assert.Equal("Jill", mother.Forename1);
            Assert.Equal("Baker", mother.Surname);
            Assert.Equal("555-0100", mother.Phone);
        }

        [Fact]
        public void GivenAnEntityWithAnExistingCaregiverContact_WhenAppliedFromAViewModelWithBlankCaregiverFields_ThenTheExistingContactIsUpdatedNotRemoved()
        {
            // Arrange — caregiver was saved previously; the user has since cleared the fields
            // in the form. The existing row must still be updated (to blank), not deleted —
            // ApplyTo only guards against *creating* a new row out of nothing.
            var entity = new CoreEntities.Patient
            {
                Contacts = new List<CoreEntities.PatientContact>
                {
                    new() { ContactType = CoreEntities.PatientContactType.Caregiver, Forename1 = "Sam", Surname = "Reed" }
                }
            };
            var viewModel = entity.ToViewModel(QrGenerator);
            viewModel.CaregiverFirstName = string.Empty;
            viewModel.CaregiverLastName = string.Empty;

            // Act
            viewModel.ApplyTo(entity);

            // Assert
            var caregiver = Assert.Single(entity.Contacts, c => c.ContactType == CoreEntities.PatientContactType.Caregiver);
            Assert.Equal(string.Empty, caregiver.Forename1);
            Assert.Equal(string.Empty, caregiver.Surname);
        }

        [Fact]
        public void GivenImportDataWithNoMotherOrCaregiverName_WhenConvertedToEntity_ThenNoMotherOrCaregiverContactIsAdded()
        {
            // Arrange
            var importData = new PatientData { FirstName = "Ada", LastName = "Lovelace" };

            // Act
            var entity = importData.ToEntity();

            // Assert
            Assert.DoesNotContain(entity.Contacts, c => c.ContactType == CoreEntities.PatientContactType.Mother);
            Assert.DoesNotContain(entity.Contacts, c => c.ContactType == CoreEntities.PatientContactType.Caregiver);
        }

        [Fact]
        public void GivenImportDataWithMissingWeightAndHeight_WhenConvertedToEntity_ThenTheyAreLeftNullNotZero()
        {
            // Arrange — unparseable/empty weight and height must not silently become 0.
            var importData = new PatientData { FirstName = "Ada", LastName = "Lovelace", Weight = string.Empty, Height = string.Empty };

            // Act
            var entity = importData.ToEntity();
            var patientContact = entity.Contacts.Single(c => c.ContactType == CoreEntities.PatientContactType.Patient);

            // Assert
            Assert.Null(patientContact.Weight);
            Assert.Null(patientContact.Height);
        }

        [Fact]
        public void GivenImportDataWithMissingGestationalAge_WhenConvertedToEntity_ThenItIsLeftNull()
        {
            // Arrange
            var importData = new PatientData { FirstName = "Ada", LastName = "Lovelace", GestationalAge = string.Empty };

            // Act
            var entity = importData.ToEntity();

            // Assert
            Assert.Null(entity.GestationalAge);
        }
    }
}
