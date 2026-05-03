using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedReability.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRestAfterInSecondsToPatientTrainingPlanDayExercise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "rest_after_in_seconds",
                table: "patient_training_plan_day_exercises",
                type: "integer",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_patient_training_plan_day_exercises_rest_after",
                table: "patient_training_plan_day_exercises",
                sql: "rest_after_in_seconds IS NULL OR rest_after_in_seconds > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_patient_training_plan_day_exercises_rest_after",
                table: "patient_training_plan_day_exercises");

            migrationBuilder.DropColumn(
                name: "rest_after_in_seconds",
                table: "patient_training_plan_day_exercises");
        }
    }
}
