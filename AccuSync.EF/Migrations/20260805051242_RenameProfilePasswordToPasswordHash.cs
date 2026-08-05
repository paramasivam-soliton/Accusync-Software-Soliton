using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccuSync.EF.Migrations
{
    /// <inheritdoc />
    public partial class RenameProfilePasswordToPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfilePassword",
                table: "Users",
                newName: "PasswordHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "ProfilePassword");
        }
    }
}
