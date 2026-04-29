namespace MedReability.Domain.Entities;

public class DoctorPatientAssignmentEntity
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public ClinicEntity ClinicEntity { get; set; } = null!;

    public Guid DoctorId { get; set; }
    public UserEntity Doctor { get; set; } = null!;

    public Guid PatientId { get; set; }
    public UserEntity Patient { get; set; } = null!;
}
