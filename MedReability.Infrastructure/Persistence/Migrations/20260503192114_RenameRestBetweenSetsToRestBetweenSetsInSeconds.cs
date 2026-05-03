using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedReability.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameRestBetweenSetsToRestBetweenSetsInSeconds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_patient_training_plan_day_exercises_sets_and_rest",
                table: "patient_training_plan_day_exercises");

            migrationBuilder.RenameColumn(
                name: "rest_between_sets",
                table: "patient_training_plan_day_exercises",
                newName: "rest_between_sets_in_seconds");

            migrationBuilder.AddCheckConstraint(
                name: "CK_patient_training_plan_day_exercises_sets_and_rest",
                table: "patient_training_plan_day_exercises",
                sql: "(sets IS NULL OR sets > 0) AND (rest_between_sets_in_seconds IS NULL OR rest_between_sets_in_seconds > 0) AND (rest_between_sets_in_seconds IS NULL OR (sets IS NOT NULL AND sets >= 2))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_patient_training_plan_day_exercises_sets_and_rest",
                table: "patient_training_plan_day_exercises");

            migrationBuilder.RenameColumn(
                name: "rest_between_sets_in_seconds",
                table: "patient_training_plan_day_exercises",
                newName: "rest_between_sets");

            migrationBuilder.AddCheckConstraint(
                name: "CK_patient_training_plan_day_exercises_sets_and_rest",
                table: "patient_training_plan_day_exercises",
                sql: "(sets IS NULL OR sets > 0) AND (rest_between_sets IS NULL OR rest_between_sets > 0) AND (rest_between_sets IS NULL OR (sets IS NOT NULL AND sets >= 2))");
        }
    }
}
