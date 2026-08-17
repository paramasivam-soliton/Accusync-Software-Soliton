using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccuSync.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralAndContactFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Audiologist",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudiologyReferral",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Physician",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReferralDate",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address2",
                table: "PatientContacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeOfBirth",
                table: "PatientContacts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Audiologist",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AudiologyReferral",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Physician",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ReferralDate",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Address2",
                table: "PatientContacts");

            migrationBuilder.DropColumn(
                name: "TimeOfBirth",
                table: "PatientContacts");
        }
    }
}
