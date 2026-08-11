// --------------------------------------------------------------------------------
// <copyright file="Profile.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Core.Entities
{
    /// <summary>
    /// A named, assignable set of permissions. <see cref="User.ProfileId"/> is a
    /// foreign key into this table — replaces the fixed <c>UserRole</c> enum with a
    /// real row, so new profiles can be added as data, without a code change. The
    /// permission list itself (the columns below) mirrors the (currently mock)
    /// Profiles screen's permission matrix (see ProfilesContentView.xaml.cs).
    /// </summary>
    public class Profile
    {
        /// <summary>The profile's unique identifier.</summary>
        public string Id { get; set; }

        /// <summary>The profile's display name.</summary>
        public string Name { get; set; }

        /// <summary>A short description of the profile's intended use.</summary>
        public string Description { get; set; }

        /// <summary>
        /// Whether this is one of the built-in profiles (Admin/Screener) seeded by
        /// the application, as opposed to one created by an administrator.
        /// </summary>
        public bool IsSystemDefined { get; set; }

        // AccuScreen Management

        /// <summary>Grants access to all AccuScreen test types.</summary>
        public bool AccuScreenAllTests { get; set; }

        /// <summary>Grants permission to perform a quick test.</summary>
        public bool AccuScreenQuickTest { get; set; }

        /// <summary>Grants access to basic AccuScreen tests.</summary>
        public bool AccuScreenBasicTests { get; set; }

        /// <summary>Grants permission to delete patients from AccuScreen.</summary>
        public bool AccuScreenDeletePatients { get; set; }

        /// <summary>Grants permission to edit patients from AccuScreen.</summary>
        public bool AccuScreenEditPatients { get; set; }

        // Device Management

        /// <summary>Grants permission to add and edit devices.</summary>
        public bool DeviceAddEdit { get; set; }

        /// <summary>Grants permission to configure test modules.</summary>
        public bool DeviceConfigureTestModules { get; set; }

        /// <summary>Grants permission to delete devices.</summary>
        public bool DeviceDelete { get; set; }

        /// <summary>Grants permission to view devices.</summary>
        public bool DeviceView { get; set; }

        // Patients and Tests

        /// <summary>Grants permission to add and edit patients.</summary>
        public bool PatientsAddEdit { get; set; }

        /// <summary>Grants permission to maintain patient comments.</summary>
        public bool PatientsCommentMaintenance { get; set; }

        /// <summary>Grants permission to configure patient management.</summary>
        public bool PatientsConfigurePatientManagement { get; set; }

        /// <summary>Grants permission to delete patients.</summary>
        public bool PatientsDelete { get; set; }

        /// <summary>Grants permission to reassign tests.</summary>
        public bool PatientsReassignTests { get; set; }

        /// <summary>Grants permission to maintain risk factors.</summary>
        public bool PatientsRiskFactorMaintenance { get; set; }

        /// <summary>Grants permission to view patients.</summary>
        public bool PatientsView { get; set; }

        // Sites and Facilities

        /// <summary>Grants permission to add and edit facilities.</summary>
        public bool SitesAddEditFacilities { get; set; }

        /// <summary>Grants permission to add and edit sites.</summary>
        public bool SitesAddEditSites { get; set; }

        /// <summary>Grants permission to configure site and facility management.</summary>
        public bool SitesConfigureSiteFacilityManagement { get; set; }

        /// <summary>Grants permission to delete facilities.</summary>
        public bool SitesDeleteFacilities { get; set; }

        /// <summary>Grants permission to delete sites.</summary>
        public bool SitesDeleteSites { get; set; }

        /// <summary>Grants permission to view facilities.</summary>
        public bool SitesViewFacilities { get; set; }

        /// <summary>Grants permission to view sites.</summary>
        public bool SitesViewSites { get; set; }

        // System Configuration

        /// <summary>Grants permission to configure the application.</summary>
        public bool SysConfigConfigureApplication { get; set; }

        /// <summary>Grants permission to view application configuration.</summary>
        public bool SysConfigViewConfiguration { get; set; }

        // Users and Profiles

        /// <summary>Grants permission to add and edit profiles.</summary>
        public bool UsersProfilesAddEditProfiles { get; set; }

        /// <summary>Grants permission to add and edit users.</summary>
        public bool UsersProfilesAddEditUsers { get; set; }

        /// <summary>Grants permission to configure user and profile management.</summary>
        public bool UsersProfilesConfigureUserProfileManagement { get; set; }

        /// <summary>Grants permission to delete profiles.</summary>
        public bool UsersProfilesDeleteProfiles { get; set; }

        /// <summary>Grants permission to delete users.</summary>
        public bool UsersProfilesDeleteUsers { get; set; }

        /// <summary>Grants permission to reset users (e.g. unlock, password reset).</summary>
        public bool UsersProfilesResetUsers { get; set; }

        /// <summary>Grants permission to view profiles.</summary>
        public bool UsersProfilesViewProfiles { get; set; }

        /// <summary>Grants permission to view users.</summary>
        public bool UsersProfilesViewUsers { get; set; }

        /// <summary>Creates a new profile with default field values (no permissions granted).</summary>
        public Profile()
        {
            Id = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            IsSystemDefined = false;
        }
    }
}
