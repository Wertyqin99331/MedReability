using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedReability.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientTrainingPlanDayProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patient_training_plan_day_progresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_training_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_number = table.Column<int>(type: "integer", nullable: false),
                    state_rating = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_training_plan_day_progresses", x => x.id);
                    table.CheckConstraint("CK_patient_training_plan_day_progresses_state_rating", "state_rating IS NULL OR (state_rating >= 1 AND state_rating <= 5)");
                    table.ForeignKey(
                        name: "FK_patient_training_plan_day_progresses_patient_training_plans~",
                        column: x => x.patient_training_plan_id,
                        principalTable: "patient_training_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_patient_training_plan_day_progresses_users_patient_id",
                        column: x => x.patient_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_patient_training_plan_day_progresses_patient_id_patient_tra~",
                table: "patient_training_plan_day_progresses",
                columns: new[] { "patient_id", "patient_training_plan_id", "day_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patient_training_plan_day_progresses_patient_training_plan_~",
                table: "patient_training_plan_day_progresses",
                column: "patient_training_plan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patient_training_plan_day_progresses");
        }
    }
}
