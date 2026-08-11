using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AccuSync.EF.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    IsSystemDefined = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    AccuScreenAllTests = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccuScreenQuickTest = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccuScreenBasicTests = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccuScreenDeletePatients = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccuScreenEditPatients = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeviceAddEdit = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeviceConfigureTestModules = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeviceDelete = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeviceView = table.Column<bool>(type: "INTEGER", nullable: false),
                    PatientsAddEdit = table.Column<bool>(type: "INTEGER", nullable: false),
                    PatientsCommentMaintenance = table.Column<bool>(type: "INTEGER", nullable: false),
                    PatientsConfigurePatientManagement = table.Column<bool>(type: "INTEGER", nullable: false),
                    PatientsDelete = table.Column<bool>(type: "INTEGER", nullable: false),
                    PatientsReassignTests = table.Column<bool>(type: "INTEGER", nullable: false),
                    PatientsRiskFactorMaintenance = table.Column<bool>(type: "INTEGER", nullable: false),
                    PatientsView = table.Column<bool>(type: "INTEGER", nullable: false),
                    SitesAddEditFacilities = table.Column<bool>(type: "INTEGER", nullable: false),
                    SitesAddEditSites = table.Column<bool>(type: "INTEGER", nullable: false),
                    SitesConfigureSiteFacilityManagement = table.Column<bool>(type: "INTEGER", nullable: false),
                    SitesDeleteFacilities = table.Column<bool>(type: "INTEGER", nullable: false),
                    SitesDeleteSites = table.Column<bool>(type: "INTEGER", nullable: false),
                    SitesViewFacilities = table.Column<bool>(type: "INTEGER", nullable: false),
                    SitesViewSites = table.Column<bool>(type: "INTEGER", nullable: false),
                    SysConfigConfigureApplication = table.Column<bool>(type: "INTEGER", nullable: false),
                    SysConfigViewConfiguration = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsersProfilesAddEditProfiles = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsersProfilesAddEditUsers = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsersProfilesConfigureUserProfileManagement = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsersProfilesDeleteProfiles = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsersProfilesDeleteUsers = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsersProfilesResetUsers = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsersProfilesViewProfiles = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsersProfilesViewUsers = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AccountName = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    ProfileId = table.Column<string>(type: "TEXT", nullable: false),
                    UsernameHash = table.Column<string>(type: "TEXT", nullable: false),
                    ProfilePassword = table.Column<string>(type: "TEXT", nullable: false),
                    FirstLogin = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    FailedLoginAttemptCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    FailedResetAttemptCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    FirstFailedLoginTime = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L),
                    FirstResetLoginTime = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L),
                    CreationDate = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L),
                    ModificationDate = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L),
                    PasswordModificationDate = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L),
                    LastThreePasswords = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Profiles",
                columns: new[] { "Id", "AccuScreenAllTests", "AccuScreenBasicTests", "AccuScreenDeletePatients", "AccuScreenEditPatients", "AccuScreenQuickTest", "Description", "DeviceAddEdit", "DeviceConfigureTestModules", "DeviceDelete", "DeviceView", "IsSystemDefined", "Name", "PatientsAddEdit", "PatientsCommentMaintenance", "PatientsConfigurePatientManagement", "PatientsDelete", "PatientsReassignTests", "PatientsRiskFactorMaintenance", "PatientsView", "SitesAddEditFacilities", "SitesAddEditSites", "SitesConfigureSiteFacilityManagement", "SitesDeleteFacilities", "SitesDeleteSites", "SitesViewFacilities", "SitesViewSites", "SysConfigConfigureApplication", "SysConfigViewConfiguration", "UsersProfilesAddEditProfiles", "UsersProfilesAddEditUsers", "UsersProfilesConfigureUserProfileManagement", "UsersProfilesDeleteProfiles", "UsersProfilesDeleteUsers", "UsersProfilesResetUsers", "UsersProfilesViewProfiles", "UsersProfilesViewUsers" },
                values: new object[,]
                {
                    { "Admin", true, true, true, true, true, "Full access, including user and system administration.", true, true, true, true, true, "Admin", true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
                    { "Screener", true, true, false, true, true, "Least-privileged profile; sees only day-to-day screening features.", false, false, false, true, true, "Screener", true, true, false, false, false, false, true, false, false, false, false, false, true, true, false, true, false, false, false, false, false, false, true, true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_Name",
                table: "Profiles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProfileId",
                table: "Users",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UsernameHash",
                table: "Users",
                column: "UsernameHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
