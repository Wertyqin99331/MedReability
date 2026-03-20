using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedReability.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDiagnosisFromDoctorPatientAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "diagnosis",
                table: "doctor_patient_assignments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "diagnosis",
                table: "doctor_patient_assignments",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");
        }
    }
}
