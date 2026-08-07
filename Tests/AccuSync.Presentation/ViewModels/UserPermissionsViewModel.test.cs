// --------------------------------------------------------------------------------
// <copyright file="UserPermissionsViewModel.test.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Presentation.ViewModels;

namespace AccuSync.Presentation.Tests.ViewModels
{
    public class UserPermissionsViewModelTests
    {
        [Fact]
        public void Admin_GivenTheAdminPreset_WhenCreated_ThenEveryNavItemAndFeatureFlagIsEnabled()
        {
            // Act
            var permissions = UserPermissionsViewModel.Admin();

            // Assert — an Admin must be able to access every app feature, per ASWD-36's AC.
            Assert.Equal("Admin", permissions.Role);
            Assert.True(permissions.CanAccessDashboard);
            Assert.True(permissions.CanAccessPatients);
            Assert.True(permissions.CanAccessUsers);
            Assert.True(permissions.CanAccessSites);
            Assert.True(permissions.CanAccessDevices);
            Assert.True(permissions.CanAccessSysConfig);
            Assert.True(permissions.CanAccessSettings);
            Assert.True(permissions.CanAccessAbout);
            Assert.True(permissions.CanAddPatient);
            Assert.True(permissions.CanEditPatient);
            Assert.True(permissions.CanDeletePatient);
            Assert.True(permissions.CanExportPatient);
            Assert.True(permissions.CanImportPatient);
            Assert.True(permissions.CanViewReports);
        }

        [Fact]
        public void Screener_GivenTheScreenerPreset_WhenCreated_ThenAdminOnlyNavItemsAreDisabled()
        {
            // Act
            var permissions = UserPermissionsViewModel.Screener();

            // Assert — a Screener must not see administrative navigation, per ASWD-36's AC.
            Assert.Equal("Screener", permissions.Role);
            Assert.False(permissions.CanAccessUsers);
            Assert.False(permissions.CanAccessSites);
            Assert.False(permissions.CanAccessDevices);
            Assert.False(permissions.CanAccessSysConfig);
            Assert.False(permissions.CanAccessSettings);
            Assert.False(permissions.CanDeletePatient);
            Assert.False(permissions.CanImportPatient);
            Assert.False(permissions.CanViewReports);
        }

        [Fact]
        public void Screener_GivenTheScreenerPreset_WhenCreated_ThenPatientWorkflowFeaturesAreStillEnabled()
        {
            // Act
            var permissions = UserPermissionsViewModel.Screener();

            // Assert — restricted from admin features, but not locked out of their own job.
            Assert.True(permissions.CanAccessDashboard);
            Assert.True(permissions.CanAccessPatients);
            Assert.True(permissions.CanAddPatient);
            Assert.True(permissions.CanEditPatient);
            Assert.True(permissions.CanExportPatient);
            Assert.True(permissions.CanAccessAbout);
        }

        [Fact]
        public void Constructor_GivenNoPresetIsUsed_WhenCreatedDirectly_ThenDefaultsToTheLeastPrivilegedScreenerShape()
        {
            // Act — this is the shape a not-yet-permission-assigned view starts from.
            var permissions = new UserPermissionsViewModel();

            // Assert
            Assert.Equal("Screener", permissions.Role);
            Assert.False(permissions.CanAccessUsers);
            Assert.False(permissions.CanAccessSysConfig);
            Assert.False(permissions.CanDeletePatient);
        }
    }
}
