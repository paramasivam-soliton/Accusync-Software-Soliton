// --------------------------------------------------------------------------------
// <copyright file="PatientMapper.test.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Abstractions.Parsing;
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
    }
}
