using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccuSync.EF.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserStatusToBoolean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);
        }
    }
}
