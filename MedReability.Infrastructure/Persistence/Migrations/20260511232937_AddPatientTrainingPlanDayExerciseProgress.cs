using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedReability.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientTrainingPlanDayExerciseProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patient_training_plan_day_exercise_progresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_training_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_number = table.Column<int>(type: "integer", nullable: false),
                    patient_training_plan_day_exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_training_plan_day_exercise_progresses", x => x.id);
                    table.ForeignKey(
                        name: "FK_ptp_day_ex_progress_day_exercise",
                        column: x => x.patient_training_plan_day_exercise_id,
                        principalTable: "patient_training_plan_day_exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ptp_day_ex_progress_patient",
                        column: x => x.patient_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ptp_day_ex_progress_plan",
                        column: x => x.patient_training_plan_id,
                        principalTable: "patient_training_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ptp_day_ex_progress_day_exercise",
                table: "patient_training_plan_day_exercise_progresses",
                column: "patient_training_plan_day_exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_ptp_day_ex_progress_plan",
                table: "patient_training_plan_day_exercise_progresses",
                column: "patient_training_plan_id");

            migrationBuilder.CreateIndex(
                name: "UX_ptp_day_ex_progress_patient_plan_day_exercise",
                table: "patient_training_plan_day_exercise_progresses",
                columns: new[] { "patient_id", "patient_training_plan_id", "day_number", "patient_training_plan_day_exercise_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patient_training_plan_day_exercise_progresses");
        }
    }
}
