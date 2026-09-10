// --------------------------------------------------------------------------------
// <copyright file="UserPermissionsViewModelTests.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Presentation.ViewModels;

namespace AccuSync.Presentation.Tests.ViewModels
{
    public class UserPermissionsViewModelTests
    {
        [Fact]
        public void Admin_Preset_EnablesEveryNavItemAndFeatureFlag()
        {
            var permissions = UserPermissionsViewModel.Admin();

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
        public void Screener_Preset_DisablesAdminOnlyNavItems()
        {
            var permissions = UserPermissionsViewModel.Screener();

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
        public void Screener_Preset_KeepsPatientWorkflowFeaturesEnabled()
        {
            var permissions = UserPermissionsViewModel.Screener();

            Assert.True(permissions.CanAccessDashboard);
            Assert.True(permissions.CanAccessPatients);
            Assert.True(permissions.CanAddPatient);
            Assert.True(permissions.CanEditPatient);
            Assert.True(permissions.CanExportPatient);
            Assert.True(permissions.CanAccessAbout);
        }

        [Fact]
        public void Construct_NoPresetUsed_DefaultsToTheLeastPrivilegedScreenerShape()
        {
            var permissions = new UserPermissionsViewModel();

            Assert.Equal("Screener", permissions.Role);
            Assert.False(permissions.CanAccessUsers);
            Assert.False(permissions.CanAccessSysConfig);
            Assert.False(permissions.CanDeletePatient);
        }
    }
}
