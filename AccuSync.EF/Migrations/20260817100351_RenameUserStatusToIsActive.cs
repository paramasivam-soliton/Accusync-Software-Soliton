using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccuSync.EF.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserStatusToIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Users",
                newName: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Users",
                newName: "Status");
        }
    }
}
