// --------------------------------------------------------------------------------
// <copyright file="ProfileConfiguration.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuSync.EF.Configurations
{
    /// <summary>
    /// Maps <see cref="Profile"/> to the Profiles table and seeds the two
    /// built-in profiles (Admin, Screener) that replace the old fixed role enum.
    /// Screener's grants mirror ProfilesContentView.xaml.cs's existing
    /// BuildScreenerPermissions() preset, so behavior is unchanged from what that
    /// (currently mock) screen already demonstrates.
    /// </summary>
    public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
    {
        /// <summary>Applies the EF Core mapping for <see cref="Profile"/> to the given builder.</summary>
        public void Configure(EntityTypeBuilder<Profile> builder)
        {
            builder.ToTable("Profiles");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name).IsRequired();
            builder.HasIndex(p => p.Name).IsUnique();

            builder.Property(p => p.Description).HasDefaultValue(string.Empty);
            builder.Property(p => p.IsSystemDefined).HasDefaultValue(false);

            builder.HasData(
                new Profile
                {
                    Id = "Admin",
                    Name = "Admin",
                    Description = "Full access, including user and system administration.",
                    IsSystemDefined = true,
                    AccuScreenAllTests = true,
                    AccuScreenQuickTest = true,
                    AccuScreenBasicTests = true,
                    AccuScreenDeletePatients = true,
                    AccuScreenEditPatients = true,
                    DeviceAddEdit = true,
                    DeviceConfigureTestModules = true,
                    DeviceDelete = true,
                    DeviceView = true,
                    PatientsAddEdit = true,
                    PatientsCommentMaintenance = true,
                    PatientsConfigurePatientManagement = true,
                    PatientsDelete = true,
                    PatientsReassignTests = true,
                    PatientsRiskFactorMaintenance = true,
                    PatientsView = true,
                    SitesAddEditFacilities = true,
                    SitesAddEditSites = true,
                    SitesConfigureSiteFacilityManagement = true,
                    SitesDeleteFacilities = true,
                    SitesDeleteSites = true,
                    SitesViewFacilities = true,
                    SitesViewSites = true,
                    SysConfigConfigureApplication = true,
                    SysConfigViewConfiguration = true,
                    UsersProfilesAddEditProfiles = true,
                    UsersProfilesAddEditUsers = true,
                    UsersProfilesConfigureUserProfileManagement = true,
                    UsersProfilesDeleteProfiles = true,
                    UsersProfilesDeleteUsers = true,
                    UsersProfilesResetUsers = true,
                    UsersProfilesViewProfiles = true,
                    UsersProfilesViewUsers = true
                },
                new Profile
                {
                    Id = "Screener",
                    Name = "Screener",
                    Description = "Least-privileged profile; sees only day-to-day screening features.",
                    IsSystemDefined = true,
                    AccuScreenAllTests = true,
                    AccuScreenQuickTest = true,
                    AccuScreenBasicTests = true,
                    AccuScreenEditPatients = true,
                    DeviceView = true,
                    PatientsAddEdit = true,
                    PatientsCommentMaintenance = true,
                    PatientsView = true,
                    SitesViewFacilities = true,
                    SitesViewSites = true,
                    SysConfigViewConfiguration = true,
                    UsersProfilesViewProfiles = true,
                    UsersProfilesViewUsers = true
                });
        }
    }
}
