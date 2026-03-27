using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedReability.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "exercises",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "exercises");
        }
    }
}
