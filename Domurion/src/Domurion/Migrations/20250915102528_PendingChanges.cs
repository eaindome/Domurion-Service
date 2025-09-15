using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domurion.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Credentials",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsShared",
                table: "Credentials",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Credentials",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SharedAt",
                table: "Credentials",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SharedFromUserId",
                table: "Credentials",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Credentials",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "SharedCredentialInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CredentialId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToEmail = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false),
                    Rejected = table.Column<bool>(type: "boolean", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedCredentialInvitations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedCredentialInvitations");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "IsShared",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "SharedAt",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "SharedFromUserId",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Credentials");
        }
    }
}
