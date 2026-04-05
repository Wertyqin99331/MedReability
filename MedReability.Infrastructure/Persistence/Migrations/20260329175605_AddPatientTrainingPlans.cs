using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedReability.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientTrainingPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patient_training_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clinic_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_training_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_patient_training_plans_clinics_clinic_id",
                        column: x => x.clinic_id,
                        principalTable: "clinics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_patient_training_plans_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_patient_training_plans_users_patient_id",
                        column: x => x.patient_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "patient_training_plan_days",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_training_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_number = table.Column<int>(type: "integer", nullable: false),
                    is_rest_day = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_training_plan_days", x => x.id);
                    table.ForeignKey(
                        name: "FK_patient_training_plan_days_patient_training_plans_patient_t~",
                        column: x => x.patient_training_plan_id,
                        principalTable: "patient_training_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "patient_training_plan_day_exercises",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_training_plan_day_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    repetitions = table.Column<int>(type: "integer", nullable: true),
                    duration_seconds = table.Column<int>(type: "integer", nullable: true),
                    comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_training_plan_day_exercises", x => x.id);
                    table.CheckConstraint("CK_patient_training_plan_day_exercises_prescription", "(repetitions IS NOT NULL AND duration_seconds IS NULL) OR (repetitions IS NULL AND duration_seconds IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_patient_training_plan_day_exercises_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_patient_training_plan_day_exercises_patient_training_plan_d~",
                        column: x => x.patient_training_plan_day_id,
                        principalTable: "patient_training_plan_days",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_patient_training_plan_day_exercises_exercise_id",
                table: "patient_training_plan_day_exercises",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_training_plan_day_exercises_patient_training_plan_d~",
                table: "patient_training_plan_day_exercises",
                columns: new[] { "patient_training_plan_day_id", "order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patient_training_plan_days_patient_training_plan_id_day_num~",
                table: "patient_training_plan_days",
                columns: new[] { "patient_training_plan_id", "day_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patient_training_plans_clinic_id",
                table: "patient_training_plans",
                column: "clinic_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_training_plans_created_by_user_id",
                table: "patient_training_plans",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_training_plans_patient_id",
                table: "patient_training_plans",
                column: "patient_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patient_training_plan_day_exercises");

            migrationBuilder.DropTable(
                name: "patient_training_plan_days");

            migrationBuilder.DropTable(
                name: "patient_training_plans");
        }
    }
}
