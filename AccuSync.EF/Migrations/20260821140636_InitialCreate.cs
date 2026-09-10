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
                name: "AppSettings",
                columns: table => new
                {
                    SettingsId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LockoutDurationMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 15)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.SettingsId);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    ProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.ProfileId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AccountName = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    ProfileId = table.Column<int>(type: "INTEGER", nullable: false),
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
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Profiles",
                columns: new[] { "ProfileId", "Name" },
                values: new object[,]
                {
                    { 0, "Screener" },
                    { 1, "Admin" }
                });

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
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
