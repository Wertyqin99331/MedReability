using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedReability.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDayProgressFeedbackFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_patient_training_plan_day_progresses_state_rating",
                table: "patient_training_plan_day_progresses");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "patient_training_plan_day_progresses");

            migrationBuilder.DropColumn(
                name: "state_rating",
                table: "patient_training_plan_day_progresses");

            migrationBuilder.AddColumn<bool>(
                name: "had_pain",
                table: "patient_training_plan_day_progresses",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "pain_intensity_rating",
                table: "patient_training_plan_day_progresses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "well_being_rating",
                table: "patient_training_plan_day_progresses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "workout_difficulty_rating",
                table: "patient_training_plan_day_progresses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ptp_day_progress_pain_consistency",
                table: "patient_training_plan_day_progresses",
                sql: "(had_pain IS TRUE AND pain_intensity_rating IS NOT NULL) OR (had_pain IS NOT TRUE AND pain_intensity_rating IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ptp_day_progress_pain_intensity_rating",
                table: "patient_training_plan_day_progresses",
                sql: "pain_intensity_rating IS NULL OR (pain_intensity_rating >= 1 AND pain_intensity_rating <= 10)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ptp_day_progress_well_being_rating",
                table: "patient_training_plan_day_progresses",
                sql: "well_being_rating IS NULL OR (well_being_rating >= 1 AND well_being_rating <= 10)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ptp_day_progress_workout_difficulty_rating",
                table: "patient_training_plan_day_progresses",
                sql: "workout_difficulty_rating IS NULL OR (workout_difficulty_rating >= 1 AND workout_difficulty_rating <= 10)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ptp_day_progress_pain_consistency",
                table: "patient_training_plan_day_progresses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ptp_day_progress_pain_intensity_rating",
                table: "patient_training_plan_day_progresses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ptp_day_progress_well_being_rating",
                table: "patient_training_plan_day_progresses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ptp_day_progress_workout_difficulty_rating",
                table: "patient_training_plan_day_progresses");

            migrationBuilder.DropColumn(
                name: "had_pain",
                table: "patient_training_plan_day_progresses");

            migrationBuilder.DropColumn(
                name: "pain_intensity_rating",
                table: "patient_training_plan_day_progresses");

            migrationBuilder.DropColumn(
                name: "workout_difficulty_rating",
                table: "patient_training_plan_day_progresses");

            migrationBuilder.DropColumn(
                name: "well_being_rating",
                table: "patient_training_plan_day_progresses");

            migrationBuilder.AddColumn<int>(
                name: "state_rating",
                table: "patient_training_plan_day_progresses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "patient_training_plan_day_progresses",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_patient_training_plan_day_progresses_state_rating",
                table: "patient_training_plan_day_progresses",
                sql: "state_rating IS NULL OR (state_rating >= 1 AND state_rating <= 5)");
        }
    }
}
