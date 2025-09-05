using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domurion.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrityHashToCredential : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IntegrityHash",
                table: "Credentials",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntegrityHash",
                table: "Credentials");
        }
    }
}
