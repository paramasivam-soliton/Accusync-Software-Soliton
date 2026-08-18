using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccuSync.EF.Migrations
{
    /// <inheritdoc />
    public partial class InitialPatientCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceId = table.Column<string>(type: "TEXT", nullable: true),
                    ImportBatchId = table.Column<int>(type: "INTEGER", nullable: true),
                    SiteId = table.Column<int>(type: "INTEGER", nullable: true),
                    AssignedUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    PatientRecordNumber = table.Column<string>(type: "TEXT", nullable: true),
                    HospitalId = table.Column<string>(type: "TEXT", nullable: true),
                    NicuStatus = table.Column<string>(type: "TEXT", nullable: true),
                    Discharged = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Deceased = table.Column<bool>(type: "INTEGER", nullable: true),
                    Medication = table.Column<string>(type: "TEXT", nullable: true),
                    ConsentState = table.Column<string>(type: "TEXT", nullable: true),
                    TrackingConsent = table.Column<string>(type: "TEXT", nullable: true),
                    ScreeningConsent = table.Column<string>(type: "TEXT", nullable: true),
                    GestationalAge = table.Column<int>(type: "INTEGER", nullable: true),
                    RaceReferenceId = table.Column<string>(type: "TEXT", nullable: true),
                    PatientRiskFactors = table.Column<string>(type: "TEXT", nullable: true),
                    ReferralFrom = table.Column<string>(type: "TEXT", nullable: true),
                    ReferralTo = table.Column<string>(type: "TEXT", nullable: true),
                    ReferralPhone = table.Column<string>(type: "TEXT", nullable: true),
                    FreeText1 = table.Column<string>(type: "TEXT", nullable: true),
                    FreeText2 = table.Column<string>(type: "TEXT", nullable: true),
                    FreeText3 = table.Column<string>(type: "TEXT", nullable: true),
                    FreeField1Value = table.Column<string>(type: "TEXT", nullable: true),
                    FreeField2Value = table.Column<string>(type: "TEXT", nullable: true),
                    FreeField3Value = table.Column<string>(type: "TEXT", nullable: true),
                    FreeField4Value = table.Column<string>(type: "TEXT", nullable: true),
                    PredefinedComments = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsExported = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ExportedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SourceCreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SourceModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                });

            migrationBuilder.CreateTable(
                name: "PatientContacts",
                columns: table => new
                {
                    ContactId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceId = table.Column<string>(type: "TEXT", nullable: true),
                    ContactType = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Forename1 = table.Column<string>(type: "TEXT", nullable: true),
                    Forename2 = table.Column<string>(type: "TEXT", nullable: true),
                    Surname = table.Column<string>(type: "TEXT", nullable: true),
                    SocialSecurityNumber = table.Column<string>(type: "TEXT", nullable: true),
                    IdNumber = table.Column<string>(type: "TEXT", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CalculatedDateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", nullable: true),
                    Height = table.Column<double>(type: "REAL", nullable: true),
                    Weight = table.Column<double>(type: "REAL", nullable: true),
                    BirthLocation = table.Column<string>(type: "TEXT", nullable: true),
                    LanguageCode = table.Column<string>(type: "TEXT", nullable: true),
                    NationalityCode = table.Column<string>(type: "TEXT", nullable: true),
                    Address1 = table.Column<string>(type: "TEXT", nullable: true),
                    Zip = table.Column<string>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    State = table.Column<string>(type: "TEXT", nullable: true),
                    Country = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    CellPhone = table.Column<string>(type: "TEXT", nullable: true),
                    Fax = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    SourceCreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SourceModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientContacts", x => x.ContactId);
                    table.ForeignKey(
                        name: "FK_PatientContacts_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestSessions",
                columns: table => new
                {
                    SessionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportBatchId = table.Column<int>(type: "INTEGER", nullable: true),
                    SessionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OverallResult = table.Column<string>(type: "TEXT", nullable: true),
                    IsExported = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ExportedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_TestSessions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestRecords",
                columns: table => new
                {
                    TestRecordId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportBatchId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceId = table.Column<string>(type: "TEXT", nullable: true),
                    TestTypeSignature = table.Column<string>(type: "TEXT", nullable: true),
                    TestType = table.Column<string>(type: "TEXT", nullable: false),
                    TestObject = table.Column<string>(type: "TEXT", nullable: true),
                    ScreeningMethod = table.Column<string>(type: "TEXT", nullable: true),
                    Application = table.Column<string>(type: "TEXT", nullable: true),
                    TestResult = table.Column<string>(type: "TEXT", nullable: true),
                    TestDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceInstrumentId = table.Column<string>(type: "TEXT", nullable: true),
                    InstrumentSerial = table.Column<string>(type: "TEXT", nullable: true),
                    InstrumentName = table.Column<string>(type: "TEXT", nullable: true),
                    TransducerSerial = table.Column<string>(type: "TEXT", nullable: true),
                    TransducerTypeName = table.Column<string>(type: "TEXT", nullable: true),
                    TransducerCalDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TransducerNextCalDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MatchedDeviceId = table.Column<int>(type: "INTEGER", nullable: true),
                    MatchedProtocolId = table.Column<int>(type: "INTEGER", nullable: true),
                    MatchedProtocolType = table.Column<string>(type: "TEXT", nullable: true),
                    SourceFacilityRefId = table.Column<string>(type: "TEXT", nullable: true),
                    SourceLocationRefId = table.Column<string>(type: "TEXT", nullable: true),
                    SourceUserRefId = table.Column<string>(type: "TEXT", nullable: true),
                    SourceBinauralRefId = table.Column<string>(type: "TEXT", nullable: true),
                    PredefinedComments = table.Column<string>(type: "TEXT", nullable: true),
                    TestDetail = table.Column<string>(type: "TEXT", nullable: true),
                    SummaryJson = table.Column<string>(type: "TEXT", nullable: true),
                    SourceCreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SourceModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRecords", x => x.TestRecordId);
                    table.ForeignKey(
                        name: "FK_TestRecords_TestSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "TestSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_PatientId",
                table: "PatientContacts",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_Type",
                table: "PatientContacts",
                column: "ContactType");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_AssignedUser",
                table: "Patients",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_IsDeleted",
                table: "Patients",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_IsExported",
                table: "Patients",
                column: "IsExported");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_SiteId",
                table: "Patients",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_SourceId",
                table: "Patients",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TestRec_ImportBatch",
                table: "TestRecords",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TestRec_PatientId",
                table: "TestRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TestRec_SessionId",
                table: "TestRecords",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestRec_SourceId",
                table: "TestRecords",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TestRec_TestDate",
                table: "TestRecords",
                column: "TestDate");

            migrationBuilder.CreateIndex(
                name: "IX_TestRec_TestType",
                table: "TestRecords",
                column: "TestType");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_Date",
                table: "TestSessions",
                column: "SessionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_PatientId",
                table: "TestSessions",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientContacts");

            migrationBuilder.DropTable(
                name: "TestRecords");

            migrationBuilder.DropTable(
                name: "TestSessions");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
