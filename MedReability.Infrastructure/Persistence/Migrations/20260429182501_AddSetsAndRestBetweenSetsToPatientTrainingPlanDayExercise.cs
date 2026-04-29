using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedReability.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSetsAndRestBetweenSetsToPatientTrainingPlanDayExercise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "rest_between_sets",
                table: "patient_training_plan_day_exercises",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sets",
                table: "patient_training_plan_day_exercises",
                type: "integer",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_patient_training_plan_day_exercises_sets_and_rest",
                table: "patient_training_plan_day_exercises",
                sql: "(sets IS NULL OR sets > 0) AND (rest_between_sets IS NULL OR rest_between_sets > 0) AND (rest_between_sets IS NULL OR (sets IS NOT NULL AND sets >= 2))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_patient_training_plan_day_exercises_sets_and_rest",
                table: "patient_training_plan_day_exercises");

            migrationBuilder.DropColumn(
                name: "rest_between_sets",
                table: "patient_training_plan_day_exercises");

            migrationBuilder.DropColumn(
                name: "sets",
                table: "patient_training_plan_day_exercises");
        }
    }
}
