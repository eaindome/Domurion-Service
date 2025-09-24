using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domurion.Migrations
{
    /// <inheritdoc />
    public partial class AddNewPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoLockEnabled",
                table: "UserPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LoginNotificationsEnabled",
                table: "UserPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoLockEnabled",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "LoginNotificationsEnabled",
                table: "UserPreferences");
        }
    }
}
