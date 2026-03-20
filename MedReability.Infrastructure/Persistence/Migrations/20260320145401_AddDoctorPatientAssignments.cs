using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedReability.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorPatientAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "doctor_patient_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clinic_id = table.Column<Guid>(type: "uuid", nullable: false),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    diagnosis = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor_patient_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_doctor_patient_assignments_clinics_clinic_id",
                        column: x => x.clinic_id,
                        principalTable: "clinics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_doctor_patient_assignments_users_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_doctor_patient_assignments_users_patient_id",
                        column: x => x.patient_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_doctor_patient_assignments_clinic_id_doctor_id_patient_id",
                table: "doctor_patient_assignments",
                columns: new[] { "clinic_id", "doctor_id", "patient_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_doctor_patient_assignments_doctor_id",
                table: "doctor_patient_assignments",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctor_patient_assignments_patient_id",
                table: "doctor_patient_assignments",
                column: "patient_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "doctor_patient_assignments");
        }
    }
}
