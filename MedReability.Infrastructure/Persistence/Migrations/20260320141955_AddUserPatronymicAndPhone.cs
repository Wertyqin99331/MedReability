using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedReability.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPatronymicAndPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "patronymic",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "patronymic",
                table: "users");

            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "users");
        }
    }
}
