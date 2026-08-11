using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    LockoutDurationMinutes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 15)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
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
                });

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
        }
    }
}
