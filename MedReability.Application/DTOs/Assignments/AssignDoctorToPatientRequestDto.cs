using System.ComponentModel.DataAnnotations;

namespace MedReability.Application.DTOs.Assignments;

public class AssignDoctorToPatientRequestDto
{
    [Required]
    public Guid PatientId { get; set; }

    [Required]
    public Guid DoctorId { get; set; }
}
